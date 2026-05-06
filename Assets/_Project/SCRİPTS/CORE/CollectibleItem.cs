using UnityEngine;
using System;

public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private PlayerEnvanter _playerEnvanter;
    [SerializeField] private ItemType.ItemCategory _thisItemType = ItemType.ItemCategory.Stone;

    [SerializeField] private bool IsCollectible;
    private bool _isCollected = false;

    private bool _isPlayerNear = false;

    void Update()
    {
        // Oyuncu yakındayken E'ye basarsa
        if (_isPlayerNear && Input.GetKeyDown(KeyCode.E) && IsCollectible)
        {
            if (_playerEnvanter.HasItem(_thisItemType))
            {
                Debug.Log("Zaten bu eşyaya sahipsin!");
                return;
            }
            else
            {
                 _playerEnvanter.AddToEnvanter(_thisItemType); // Tipi de gönderiyoruz
            }
            
            
            
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) _isPlayerNear = true;
        Debug.Log("Trigger'a girdi");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player")) _isPlayerNear = false;
        Debug.Log("Trigger'dan çıktı");
    }
}