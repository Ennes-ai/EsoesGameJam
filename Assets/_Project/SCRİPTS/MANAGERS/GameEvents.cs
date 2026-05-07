using UnityEngine;
using System;


public class GameEvents : MonoBehaviour
{
    // Eşyayı kullandık, dünyaya duyuruyoruz (Observer Pattern)
    public static Action<ItemType> OnItemUsed;

    public static void TriggerItemUsed(ItemType itemCategory)
    {
        OnItemUsed?.Invoke(itemCategory);
        Debug.Log("Eşya kullanıldı: " + itemCategory);
    }
}