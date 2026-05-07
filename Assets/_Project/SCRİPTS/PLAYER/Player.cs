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

    public bool IsCanGoLobby = false;

    private Portals currentPortalData;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Kamera takibindeki titremeyi (jitter) önlemek için fiziği render ile senkronize ediyoruz
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        playerEnvanter = GetComponent<PlayerEnvanter>();

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
            LoadTheLevel(targetLevel); // Genelde "Lobby" olur
            return;
        }

        // 2. Durum: Lobideki levellere giriş portallarındaysak
        // Hedef sahne ismine göre kilit kontrolü yapalım
        bool canEnter = false;

        if (targetLevel == "SampleScene") canEnter = true; // Level 1 her zaman açık
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
            lobbyUI.LoadSceneWithFade(levelName);
        }
        else
        {
            SceneManager.LoadScene(levelName); // UI bulunamazsa güvenli geçiş (Fail-safe)
        }
    }

    public Vector2 GetLastLookingPoint() => movement;
}
