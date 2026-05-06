using UnityEngine;

[CreateAssetMenu(fileName = "ItemType", menuName = "Scriptable Objects/ItemType")]
public class ItemType : ScriptableObject
{
    public enum ItemCategory
    {
        None,
        Stone,
        River,
    }
}
