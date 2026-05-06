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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
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
            LoadTheLevel(levelName : portalName);
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

                
                // Burada sahne geçişi veya diğer işlemleri yapabilirsiniz
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
