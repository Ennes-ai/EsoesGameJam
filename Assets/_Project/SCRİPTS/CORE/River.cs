using UnityEngine;
using System;


public class River : MonoBehaviour
{
    [SerializeField] private ItemType.ItemCategory _thisItemType = ItemType.ItemCategory.River;
    [SerializeField] private Sprite stoneSprite; 

    private bool _isPlayerNear = false;

    private void OnEnable()
    {
        GameEvents.OnItemUsed += HandleItemUsed;
    }
    private void OnDisable()
    {
        GameEvents.OnItemUsed -= HandleItemUsed;
    }

    private void HandleItemUsed(ItemType.ItemCategory UsedItemCategory)
    {
        if ( _isPlayerNear)
        {
            // Taş kullanıldı, nehir taşına dönüş
            //GetComponent<SpriteRenderer>().sprite = stoneSprite;
            Debug.Log("Nehir taşı oldu! taş kullanıldı: " + UsedItemCategory.ToString());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _isPlayerNear = true;
            Debug.Log("Nehir yakınında!");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            _isPlayerNear = false;
            Debug.Log("Nehirden uzaklaşıldı!");
        }
    }
}