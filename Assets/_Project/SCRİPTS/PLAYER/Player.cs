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

    public bool CanGoSampleScene  = true , CanGoLevel_2 = false , CanGoLevel_3 = false, CanGoLevel_4 = false ;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Kamera takibindeki titremeyi (jitter) önlemek için fiziği render ile senkronize ediyoruz
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        
        playerEnvanter = GetComponent<PlayerEnvanter>();
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
        if (IsInPortal && Input.GetKeyDown(KeyCode.E))
        {
            // Portala girdiğinde E tuşuna basılırsa sahne geçişi yap
            if (SceneManager.GetActiveScene().name != portalName) // Aynı sahneye geçiş yapmayı önlemek için
            {
                // Hedef portal 'Lobby' ise ve iznimiz yoksa geçişi engelle
                #region Sahne Geçiş Kontrolleri
                if (portalName == "Lobby" && !IsCanGoLobby)
                {
                    Debug.Log("Lobiye dönmek için henüz iznin yok!");
                    return;
                }
                else if (portalName == "Lobby" && IsCanGoLobby)
                {
                    LoadTheLevel(levelName : portalName);
                }else if (portalName == "SampleScene" && !CanGoSampleScene)
                {
                    Debug.Log("SampleScene'e geçmek için henüz iznin yok!");
                    return;
                }
                else if (portalName == "SampleScene" && CanGoSampleScene)
                {
                    LoadTheLevel(levelName : portalName);
                }else if (portalName == "Level_2" && !CanGoLevel_2)
                {
                    Debug.Log("Level_2'ye geçmek için henüz iznin yok!");
                    return;
                }
                else if (portalName == "Level_2" && CanGoLevel_2)
                {
                    LoadTheLevel(levelName : portalName);
                }else if (portalName == "Level_3" && !CanGoLevel_3)
                {
                    Debug.Log("Level_3'e geçmek için henüz iznin yok!");
                    return; 
                }
                else if (portalName == "Level_3" && CanGoLevel_3)
                {
                    LoadTheLevel(levelName : portalName);
                }else if (portalName == "Level_4" && !CanGoLevel_4)
                {
                    Debug.Log("Level_4'e geçmek için henüz iznin yok!");
                    return;
                }
                else if (portalName == "Level_4" && CanGoLevel_4)
                {
                    LoadTheLevel(levelName : portalName);
                }
                #endregion
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            IsCanGoLobby = true;
        }
    }

    void FixedUpdate()
    {
        // Fizik tabanlı hareketi burada uyguluyoruz
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
          if (other.gameObject.CompareTag("Portal"))
        {
            IsInPortal = true;
            Portals portalData = other.gameObject.GetComponent<Portals>();
            if (portalData != null)
            {
                portalName = portalData.LoadingPortalName;

                Debug.Log("Portala girdi: " + portalName);

                
              
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) 
    {
        if (other.gameObject.CompareTag("Portal"))
        {
            IsInPortal = false;
            Debug.Log("Portaldan çıktı");
        }
    }

    private void LoadTheLevel(string levelName)
    {
        SceneManager.LoadScene(levelName);
    }
}
