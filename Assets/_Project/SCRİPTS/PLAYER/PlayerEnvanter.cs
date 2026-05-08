using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemType itemType;
    public int count;
    public InventorySlot(ItemType type, int amount) { itemType = type; count = amount; }
}

public class PlayerEnvanter : MonoBehaviour
{
    public ItemType currentItem = null;
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();
    public static PlayerEnvanter Instance;
    void Awake() 
    { 
        Instance = this; 
    }

    void Update()
    {
        // Klavyeden 1, 2, 3... tuşlarına basarak envanterdeki eşyayı seçme
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectItem(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectItem(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectItem(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SelectItem(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SelectItem(4);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) SelectItem(5);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) SelectItem(6);
        else if (Input.GetKeyDown(KeyCode.Alpha8)) SelectItem(7);
        else if (Input.GetKeyDown(KeyCode.Alpha9)) SelectItem(8);
    }

    public void SelectItem(int index)
    {
        if (inventorySlots != null && index >= 0 && index < inventorySlots.Count)
        {
            currentItem = inventorySlots[index].itemType;
            Debug.Log("Eşya seçildi: " + currentItem.name);
        }
    }

    public bool CanAddItem(ItemType incomingItem)
    {
        InventorySlot slot = inventorySlots.Find(s => s.itemType == incomingItem);
        if (slot != null) return slot.count < 3; // Aynı eşyadan maksimum 3 tane (Stack)
        return inventorySlots.Count < 3; // Maksimum 3 farklı eşya türü (Slot)
    }

    public void AddToEnvanter(ItemType incomingItem)
    {
        InventorySlot slot = inventorySlots.Find(s => s.itemType == incomingItem);
        if (slot != null)
        {
            slot.count++;
        }
        else
        {
            inventorySlots.Add(new InventorySlot(incomingItem, 1));
        }
        currentItem = incomingItem; // Yeni geleni aktif ele al
        Debug.Log("Envantere eklendi: " + incomingItem.name);

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
    }

    public void RemoveTheItem(ItemType itemToRemove)
    {
        InventorySlot slot = inventorySlots.Find(s => s.itemType == itemToRemove);
        if (slot != null)
        {
            slot.count--;
            if (slot.count <= 0)
            {
                inventorySlots.Remove(slot);
                if (currentItem == itemToRemove) currentItem = null;
            }
        }
    }

    public bool HasItem(ItemType itemToCheck)
    {
        return inventorySlots.Exists(s => s.itemType == itemToCheck);
    }

    public ItemType GetItemAtHand()
    {
        return currentItem;
    }
}
