using UnityEngine;

[RequireComponent(typeof(PlayerEnvanter))]
public class ThrowItem : MonoBehaviour
{
    public GameObject firePoint;
    public GameObject itemToThrowPrefab;
    public ItemType currentInventoryItemType;
    [SerializeField] private Player player; 
    private Vector2 lastDir = Vector2.down;

    void Start()
    {
        player = gameObject.GetComponent<Player>();
    }

    void Update()
    {
        Vector2 movementVec = player.GetLastLookingPoint();
        if(movementVec != Vector2.zero)
        {
            lastDir = new Vector2(movementVec.x, movementVec.y).normalized;
            UpdateFirePointRotation();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("space");
            Throw(PlayerEnvanter.Instance.GetItemAtHand());
        }                
    }

    void Throw(ItemType itemToThrowType)
    {
        if(itemToThrowType == null)
        {
            return;
        }

        if (AudioManager.instance != null && AudioManager.instance.itemThrowSound != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.itemThrowSound);
        }
        
        // Önce objeyi sahnede yaratıyoruz (Instantiate)
        GameObject thrownInstance = Instantiate(itemToThrowPrefab, firePoint.transform.position, firePoint.transform.rotation);
        
        // Sonra YARATILAN KOPYA (instance) üzerindeki scripti bulup ona ItemType'ı atıyoruz
        ThrownItem thrownScript = thrownInstance.GetComponent<ThrownItem>();
        if (thrownScript != null)
        {
            thrownScript.itemType = itemToThrowType;
            Debug.Log("Fırlatılan kopya tipi: " + thrownScript.itemType);
        }

        PlayerEnvanter.Instance.UseTheItem();
    }

    void UpdateFirePointRotation()
    {
        float angle = Mathf.Atan2(lastDir.y, lastDir.x) * Mathf.Rad2Deg;
        firePoint.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}