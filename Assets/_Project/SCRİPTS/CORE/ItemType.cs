using UnityEngine;

public enum ItemCategory
{
    None,
    Stone,
    Wall,
    River,
    Door,
    Key,
    Stick,
    Flower
}

[CreateAssetMenu(fileName = "ItemType", menuName = "Scriptable Objects/ItemType")]
public class ItemType : ScriptableObject
{
    public ItemCategory category;
    public Sprite itemSprite;
    public string itemTag;
    public bool canBePickedUp;
    
    [Header("Fizik Ayarları")]
    public bool isPassable; 
}