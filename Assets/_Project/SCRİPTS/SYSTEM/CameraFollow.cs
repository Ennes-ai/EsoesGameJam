using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Hedef Ayarları")]
    [Tooltip("Kameranın takip edeceği obje (genellikle Player)")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Yumuşaklık Ayarları")]
    [Range(0.01f, 1f)]
    [Tooltip("Kameranın hedefe ulaşma süresi. Değer düştükçe kamera daha sert takip eder.")]
    [SerializeField] private float smoothTime = 0.125f;
    private Vector3 velocity = Vector3.zero;

    [Header("Sınır (Bounds) Ayarları")]
    [Tooltip("Kameranın harita dışına çıkmasını engellemek istiyorsanız aktifleştirin.")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private void LateUpdate()
    {
        // Hedef yoksa otomatik olarak sahnede Player etiketli objeyi bulmaya çalış
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) target = player.transform;
            else return; // Hâlâ yoksa hata vermemek için bekle
        }

        // Kameranın gitmesi gereken ideal pozisyon
        Vector3 desiredPosition = target.position + offset;

        // Eğer sınırlar aktifse, hedeflenen pozisyonu sınırlar içinde tut (Clamp)
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        // Kamerayı şu anki konumundan hedeflenen konuma pürüzsüzce hareket ettir
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }

    // Unity Editöründe sınırları görsel olarak çizer (Sahne tasarımı yaparken çok işine yarar)
    private void OnDrawGizmos()
    {
        if (useBounds)
        {
            Gizmos.color = Color.green;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}