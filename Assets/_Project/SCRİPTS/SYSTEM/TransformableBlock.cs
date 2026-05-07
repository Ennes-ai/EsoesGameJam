using UnityEngine;
using System.Collections; // YENİ: Coroutine kullanmak için eklendi

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

    // YENİ: Taş şu an kayıyor mu kontrolü
    private bool isMoving = false;

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

    // --- YENİ: İTTİRME MATEMATİĞİ ---
    public void TryPush(Vector2 pushDir)
    {
        // Zaten hareket ediyorsa VEYA içinden geçilebilir bir şeyse (su vs.) ittirilemez.
        if (isMoving || col2D.isTrigger) return;

        // İsteğe bağlı: Sadece Taşlar ittirilebilsin, kapı/duvar ittirilemesin kuralı.
        if (currentItemType != null && currentItemType.category != ItemCategory.Stone) return;

        // Sadece tam 4 ana yöne (X veya Y) gitmesini sağla, çapraz gitmeyi engelle.
        if (Mathf.Abs(pushDir.x) > Mathf.Abs(pushDir.y))
            pushDir = new Vector2(Mathf.Sign(pushDir.x), 0);
        else
            pushDir = new Vector2(0, Mathf.Sign(pushDir.y));

        // İttireceğimiz yönde duvar var mı diye BoxCast atıyoruz.
        col2D.enabled = false; // Kendi kendimize çarpmamak için geçici kapat
        RaycastHit2D hit = Physics2D.BoxCast(col2D.bounds.center, col2D.bounds.size * 0.9f, 0f, pushDir, 1f);
        col2D.enabled = true; // Geri aç

        // Önü tamamen boşsa VEYA önündeki obje içinden geçilebilir (tetikleyici) bir şeyse ilerle
        if (hit.collider == null || hit.collider.isTrigger)
        {
            StartCoroutine(MoveBlockCoroutine(pushDir));
        }
    }

    private IEnumerator MoveBlockCoroutine(Vector2 direction)
    {
        isMoving = true;
        
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + (Vector3)direction;
        
        float elapsedTime = 0;
        float duration = 0.25f; // Bloğun 1 kareyi kayarak geçme süresi

        // Ses Eklentisi (Sürüklenme Sesi) - Eğer AudioManager'da varsa
        // if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.stonePushSound);

        // Pürüzsüz kaydırma (Lerp)
        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos; // Küsurat kalmasın, tam 1x1 kareye oturt.
        
        // Yapay Zeka (A*) Ağını Güncelle
        if (AstarPath.active != null)
        {
            AstarPath.active.UpdateGraphs(col2D.bounds);
        }

        isMoving = false;
    }
    // ---------------------------------

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