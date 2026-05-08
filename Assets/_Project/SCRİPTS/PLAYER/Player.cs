using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private PlayerEnvanter playerEnvanter;
    private bool IsInPortal = false;
    private string portalName;
    private bool _isTransitioning = false; // Portala girerken spamlama olmasın diye kontrol değişkeni

    private Animator animator;
    private string currentAnimState = "Idle";

    public bool IsCanGoLobby = false;

    private Portals currentPortalData;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Kamera takibindeki titremeyi (jitter) önlemek için fiziği render ile senkronize ediyoruz
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        playerEnvanter = GetComponent<PlayerEnvanter>();
        animator = GetComponent<Animator>();

        if (!PlayerPrefs.HasKey("ReachedLevel"))  // oyun ilk defa aciliyorsa otomatik 0. level acik olsun
        {
            PlayerPrefs.SetInt("ReachedLevel", 0);
        }
    }

    void Update()
    {
        // Klavye veya gamepad girdilerini alıyoruz (Keskin dönüşler için GetAxisRaw)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Çapraz giderken hızlanmayı önlemek için vektörü normalize ediyoruz
        movement = movement.normalized;

        UpdateAnimations();

        if (Input.GetKeyDown(KeyCode.T))
        {
            playerEnvanter.UseTheItem();
        }

        if (IsInPortal && Input.GetKeyDown(KeyCode.E) && currentPortalData != null && !_isTransitioning)
        {
            HandlePortalLogic();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            IsCanGoLobby = true;
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        string newState = "Idle";

        // Yön önceliğini belirliyoruz (Önce yatay, sonra dikey)
        if (movement.x > 0) newState = "Right_Walk";
        else if (movement.x < 0) newState = "Left_Walk";
        else if (movement.y > 0) newState = "Down_Walk"; // W tuşu (yukarı) -> Down_Walk tetiklesin
        else if (movement.y < 0) newState = "Up_Walk";   // S tuşu (aşağı) -> Up_Walk tetiklesin

        if (newState == "Idle")
        {
            // Tuşu bıraktığımızda animasyon hızını 0 yaparak son karede donduruyoruz
            animator.speed = 0f;
        }
        else
        {
            // Hareket varken animasyonu normal hızında (1) oynat
            animator.speed = 1f;

            // Sadece yön değiştiğinde yeni trigger'ı tetikle
            if (newState != currentAnimState)
            {
                animator.ResetTrigger("Right_Walk");
                animator.ResetTrigger("Left_Walk");
                animator.ResetTrigger("Up_Walk");
                animator.ResetTrigger("Down_Walk");

                animator.SetTrigger(newState);
                currentAnimState = newState;
            }
        }
    }

    private void HandlePortalLogic()
    {
        _isTransitioning = true; // Geçiş başladı, başka tıklamaları engelle

        string targetLevel = currentPortalData.LoadingPortalName;
        int reachedLevel = PlayerPrefs.GetInt("ReachedLevel");

        // 1. Durum: Seviye sonundaki "Zafer" portalındaysak (Örn: Level 1 bitti)
        if (currentPortalData.isVictoryPortal)
        {
            // Eğer bitirdiğimiz level, açılacak olandan büyükse kilidi güncelle
            if (currentPortalData.unlockLevelIndex > reachedLevel)
            {
                PlayerPrefs.SetInt("ReachedLevel", currentPortalData.unlockLevelIndex);
                Debug.Log("YENİ LEVEL AÇILDI: Level " + currentPortalData.unlockLevelIndex);
            }
            LoadTheLevel(targetLevel); // Genelde "Lobby" olur
            return;
        }

        // 2. Durum: Lobideki levellere giriş portallarındaysak
        // Hedef sahne ismine göre kilit kontrolü yapalım
        bool canEnter = false;

        if (targetLevel == "Level_0") canEnter = true; // Level_0 her zaman açık
        else if (targetLevel == "Level_1" && reachedLevel >= 1) canEnter = true; // Level 1
        else if (targetLevel == "Level_2" && reachedLevel >= 2) canEnter = true;
        else if (targetLevel == "Level_3" && reachedLevel >= 3) canEnter = true;
        else if (targetLevel == "Level_4" && reachedLevel >= 4) canEnter = true;
        else if (targetLevel == "Lobby") canEnter = true;

        if (canEnter)
        {
            LoadTheLevel(targetLevel);
        }
        else
        {
            _isTransitioning = false; // Geçiş başarısız olursa tekrar E'ye basabilsin
            Debug.Log("Bu seviye henüz kilitli! Önceki seviyeyi bitirmelisin.");
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Portal"))
        {
            IsInPortal = true;
            currentPortalData = other.GetComponent<Portals>();
            if (currentPortalData != null) Debug.Log("Portalın önündesin: " + currentPortalData.LoadingPortalName);
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.CompareTag("Portal"))
        {
            IsInPortal = false;
            currentPortalData = null;
        }
    }

    private void LoadTheLevel(string levelName)
    {
        LobbyUIS lobbyUI = FindAnyObjectByType<LobbyUIS>();
        
        if (lobbyUI != null)
        {
            // Eğer bulunduğumuz sahne Level_4 ise portal bizi oyun sonu sinematiğine soksun
            if (SceneManager.GetActiveScene().name == "Level_4")
            {
                lobbyUI.PlayEndingSequence();
            }
            else
            {
                lobbyUI.LoadSceneWithFade(levelName);
            }
        }
        else
        {
            SceneManager.LoadScene(levelName); // UI bulunamazsa güvenli geçiş (Fail-safe)
        }
    }

    public Vector2 GetLastLookingPoint() => movement;
}
