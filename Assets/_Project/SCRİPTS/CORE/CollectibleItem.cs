using UnityEngine;
using System;

public class CollectibleItem : MonoBehaviour
{
    private PlayerEnvanter _playerEnvanter;
    //[SerializeField] private ItemType.ItemCategory _thisItemType = ItemType.ItemCategory.Stone;
    [SerializeField] public ItemType itemType;

    [SerializeField] private bool IsCollectible;

    private bool _isPlayerNear = false;

    void Start()
    {
        gameObject.tag = itemType.itemTag;
    } 

    void Update()
    {
        // Oyuncu yakındayken E'ye basarsa
        if (_isPlayerNear && Input.GetKeyDown(KeyCode.E) && IsCollectible && _playerEnvanter != null)
        {
            if (!_playerEnvanter.CanAddItem(itemType))
            {
                Debug.Log("Envanter tam dolu veya bu eşyadan maksimum sayıya (3) ulaştın!");
                return;
            }
            else
            {
                _playerEnvanter.AddToEnvanter(itemType); // Tipi de gönderiyoruz
                Debug.Log("item alindi");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) {
            _isPlayerNear = true;
            _playerEnvanter = collision.GetComponent<PlayerEnvanter>();
        }
        Debug.Log("Trigger'a girdi");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _isPlayerNear = false;
            _playerEnvanter = null;
        } 
        Debug.Log("Trigger'dan çıktı");
    }
}