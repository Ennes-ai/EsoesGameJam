using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerEnvanter : MonoBehaviour
{
    public ItemType currentItem = null;
    public List<ItemType> collectedItems = new List<ItemType>();
    public static PlayerEnvanter Instance;
    void Awake() 
    { 
        Instance = this; 
    }

    public void AddToEnvanter(ItemType incomingItem)
    {
        currentItem = incomingItem;
        collectedItems.Add(incomingItem);
        Debug.Log("Envantere eklendi: " + incomingItem);

        if (AudioManager.instance != null && AudioManager.instance.itemPickUp != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.itemPickUp);
        }
    }

   public void UseTheItem()
    {
        // 1. Önce kontrol et: Elimde gerçekten bir şey var mı?
        if (currentItem == null) return;

        // 2. Değeri bir geçici değişkende tut (Sıfırlamadan önce silmek için)
        ItemType itemToBeUsed = currentItem;

        // 3. Dünyaya duyur (Nehir bunu duyacak)
        GameEvents.TriggerItemUsed(itemToBeUsed);
        
        // 4. Listeden sil (Hala 'Stone' iken silmelisin)
        RemoveTheItem(itemToBeUsed);

        // 5. En son eldeki eşyayı sıfırla
        currentItem = null;
    }

    public void RemoveTheItem(ItemType itemToRemove)
    {
        if (HasItem(itemToRemove))
        {
            collectedItems.Remove(itemToRemove);
            Debug.Log("Envanterden çıkarıldı: " + itemToRemove);
        }
    }

    public bool HasItem(ItemType itemToCheck)
    {
        return collectedItems.Contains(itemToCheck);
    }

    public ItemType GetItemAtHand()
    {
        return currentItem;
    }
}
