using UnityEngine;

[RequireComponent(typeof(CollectibleItem))]
[RequireComponent(typeof(Collider2D))]
public class TransformableBlock : MonoBehaviour
{
    public bool isTransformed = false;
    public bool isGuard = false;

    private ItemType originalItemType;
    private bool originalIsPassable;
    
    // YENİ: Sahnede arama yaparken bu bloğun o anki türünü bilebilmemiz için
    public ItemType currentItemType; 

    private SpriteRenderer spriteRenderer;
    private Collider2D col2D; 

    void Start()
    {
        CollectibleItem item = GetComponent<CollectibleItem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col2D = GetComponent<Collider2D>();
        
        originalItemType = item.itemType;
        originalIsPassable = col2D.isTrigger; 
        
        // YENİ: Oyun başladığında güncel tür orijinal türdür
        currentItemType = originalItemType; 
    }

    public void TransformBlock(ItemType newType)
    {
        isTransformed = true;
        spriteRenderer.sprite = newType.itemSprite; 
        gameObject.tag = newType.itemTag;
        col2D.isTrigger = newType.isPassable; 
        
        // YENİ: Dönüştüğünde güncel türü değiştir
        currentItemType = newType;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPopSound();
        }
        
        if (AstarPath.active != null)
        {
            AstarPath.active.UpdateGraphs(col2D.bounds);
        }
    }

    public void RevertToOriginal()
    {
        Debug.Log(originalItemType + " + " + originalIsPassable + originalItemType.itemSprite);
        isTransformed = false;
        spriteRenderer.sprite = originalItemType.itemSprite;
        gameObject.tag = originalItemType.itemTag;
        col2D.isTrigger = originalIsPassable; 
        
        // YENİ: Bekçi düzelttiğinde güncel türü tekrar orijinaline eşitle
        currentItemType = originalItemType;
        
        if (AstarPath.active != null)
        {
            AstarPath.active.UpdateGraphs(col2D.bounds);
        }
    }
}