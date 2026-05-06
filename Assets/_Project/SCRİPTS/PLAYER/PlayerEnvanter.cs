using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlayerEnvanter : MonoBehaviour
{
    public ItemType.ItemCategory currentItem = ItemType.ItemCategory.None;
    public List<ItemType.ItemCategory> collectedItems = new List<ItemType.ItemCategory>();
    public static PlayerEnvanter Instance;
    void Awake() 
    { 
        Instance = this; 
    }

    public void AddToEnvanter(ItemType.ItemCategory incomingItem)
    {
        currentItem = incomingItem;
        collectedItems.Add(incomingItem);
        Debug.Log("Envantere eklendi: " + incomingItem);
    }

   public void UseTheItem()
{
    // 1. Önce kontrol et: Elimde gerçekten bir şey var mı?
    if (currentItem == ItemType.ItemCategory.None) return;

    // 2. Değeri bir geçici değişkende tut (Sıfırlamadan önce silmek için)
    ItemType.ItemCategory itemToBeUsed = currentItem;

    // 3. Dünyaya duyur (Nehir bunu duyacak)
    GameEvents.TriggerItemUsed(itemToBeUsed);
    
    // 4. Listeden sil (Hala 'Stone' iken silmelisin)
    RemoveTheItem(itemToBeUsed);

    // 5. En son eldeki eşyayı sıfırla
    currentItem = ItemType.ItemCategory.None;
}

    public void RemoveTheItem(ItemType.ItemCategory itemToRemove)
    {
        if (HasItem(itemToRemove))
        {
            collectedItems.Remove(itemToRemove);
            Debug.Log("Envanterden çıkarıldı: " + itemToRemove);
        }
    }

    public bool HasItem(ItemType.ItemCategory itemToCheck)
    {
        return collectedItems.Contains(itemToCheck);
    }
}
