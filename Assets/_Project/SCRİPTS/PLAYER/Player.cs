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

        if (!PlayerPrefs.HasKey("ReachedLevel"))  // oyun ilk defa aciliyorsa otomatik 1. level acik olsun
        {
            PlayerPrefs.SetInt("ReachedLevel", 1);
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

        if (IsInPortal && Input.GetKeyDown(KeyCode.E) && currentPortalData != null)
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
            // LEVEL 3 (Son Seviye) BİTİRİLDİYSE FİNAL SİNEMATİĞİNİ OYNAT
            if (SceneManager.GetActiveScene().name == "Level_3")
            {
                LobbyUIS lobbyUI = FindAnyObjectByType<LobbyUIS>();
                if (lobbyUI != null)
                {
                    lobbyUI.PlayEndingSequence();
                    return; // Klasik lobiye geçişi iptal et, sinematiği bekle
                }
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
        else if (targetLevel == "Lobby") canEnter = true;

        if (canEnter)
        {
            LoadTheLevel(targetLevel);
        }
        else
        {
            Debug.Log("Bu seviye henüz kilitli! Önceki seviyeyi bitirmelisin.");
        }
    }

    void FixedUpdate()
    {
        // Fizik tabanlı hareketi burada uyguluyoruz
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
            lobbyUI.LoadSceneWithFade(levelName);
        }
        else
        {
            SceneManager.LoadScene(levelName);
        }
    }

    public Vector2 GetLastLookingPoint()
    {
        return movement;
    }

    // --- YENİ: KARE KARE İTTİRME İÇİN ÇARPIŞMA KONTROLÜ ---
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Eğer karakter bir yere yürüyorsa (W,A,S,D basıyorsa)
        if (movement != Vector2.zero)
        {
            TransformableBlock block = collision.gameObject.GetComponent<TransformableBlock>();
            
            if (block != null)
            {
                // Karakterin gidiş yönü, taşa doğru mu bakıyoruz?
                Vector2 dirToBlock = (collision.transform.position - transform.position).normalized;
                
                // Karakter gerçekten taşa doğru güç uyguluyorsa ittirmeyi tetikle
                if (Vector2.Dot(movement, dirToBlock) > 0.5f)
                {
                    block.TryPush(movement);
                }
            }
        }
    }
}