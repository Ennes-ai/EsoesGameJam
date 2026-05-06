using NUnit.Framework;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private PlayerEnvanter playerEnvanter;

    public bool IsCanGoLobby = false;

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
    }

    void FixedUpdate()
    {
        // Fizik tabanlı hareketi burada uyguluyoruz
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
