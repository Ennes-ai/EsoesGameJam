using UnityEngine;

public class ThrownItem : MonoBehaviour
{
    public ItemType itemType;
    public float speed = 10f;
    public float decelRate = 3f;
    public float stopThreshold = .1f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.up * speed;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, decelRate * Time.fixedDeltaTime);

        if(rb.linearVelocity.magnitude < stopThreshold)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player")) return; 

        TransformableBlock hitBlock = collision.GetComponent<TransformableBlock>();        
        if(hitBlock != null)
        {
            Debug.Log("item to transform into: " + itemType);
            
            // YENİ: Sadece çarpılanı değil, o hedefin türündeki BÜTÜN blokları dönüştür
            GlobalTransformationManager.TransformAllBlocksOfType(hitBlock.currentItemType, itemType);
        }

        Destroy(gameObject);
    }
}