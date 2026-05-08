using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CollectibleItem))]
[RequireComponent(typeof(Collider2D))]
public class TransformableBlock : MonoBehaviour
{
    public bool isTransformed = false;
    public bool isGuard = false;

    private ItemType originalItemType;
    private bool originalIsPassable;
    
    public ItemType currentItemType; 

    private SpriteRenderer spriteRenderer;
    private Collider2D col2D; 
    private bool isMoving = false;

    void Start()
    {
        CollectibleItem item = GetComponent<CollectibleItem>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col2D = GetComponent<Collider2D>();
        
        originalItemType = item.itemType;
        originalIsPassable = col2D.isTrigger; 
        
        currentItemType = originalItemType; 
    }

    // --- İTTİRME MATEMATİĞİ (Duvardan Geçme Çözüldü) ---
    public void TryPush(Vector2 pushDir)
    {
        if (isMoving || col2D.isTrigger) return;
        if (currentItemType != null && currentItemType.category != ItemCategory.Stone) return;

        if (Mathf.Abs(pushDir.x) > Mathf.Abs(pushDir.y))
            pushDir = new Vector2(Mathf.Sign(pushDir.x), 0);
        else
            pushDir = new Vector2(0, Mathf.Sign(pushDir.y));

        // YENİ: İleriye ışın kılıcı (Raycast) atmak yerine, tam gideceğimiz HEDEF kareyi buluyoruz
        Vector2 targetPos = (Vector2)transform.position + pushDir;
        
        // Hedef karenin tam ortasına hayali bir kutu yerleştirip, orada duran her şeyi listeliyoruz
        Collider2D[] hits = Physics2D.OverlapBoxAll(targetPos, col2D.bounds.size * 0.5f, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == col2D) continue; // Kendimizi (Taşı) yoksayıyoruz
            if (!hit.isTrigger) return; // Eğer hedefte içinden geçilemeyen (KATI) bir obje varsa, ittirmeyi İPTAL ET!
        }

        StartCoroutine(MoveBlockCoroutine(pushDir));
    }

    private IEnumerator MoveBlockCoroutine(Vector2 direction)
    {
        isMoving = true;
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + (Vector3)direction;
        
        float elapsedTime = 0;
        float duration = 0.25f; 

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos; 
        
        if (AstarPath.active != null)
        {
            AstarPath.active.UpdateGraphs(col2D.bounds);
        }

        isMoving = false;
    }

    public void TransformBlock(ItemType newType)
    {
        isTransformed = true;
        spriteRenderer.sprite = newType.itemSprite; 
        gameObject.tag = newType.itemTag;
        col2D.isTrigger = newType.isPassable; 
        
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
        isTransformed = false;
        spriteRenderer.sprite = originalItemType.itemSprite;
        gameObject.tag = originalItemType.itemTag;
        col2D.isTrigger = originalIsPassable; 
        
        // Hafızadaki orijinal haline geri dön (İnceyse ince, kalınsa kalın)
        currentItemType = originalItemType;
        
        if (AstarPath.active != null)
        {
            AstarPath.active.UpdateGraphs(col2D.bounds);
        }
    }
}