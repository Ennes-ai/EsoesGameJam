using UnityEngine;
using System.Collections; // Coroutine (IEnumerator) kullanmak için gerekli

public class GlobalTransformationManager : MonoBehaviour
{
    public static GlobalTransformationManager Instance;

    [Header("Dal/Çiçek yere düşerken kullanılacak toplanabilir Prefab")]
    public GameObject collectiblePrefab; 

    [Header("Dönüşüm Gecikme Ayarları")]
    [Tooltip("Dönüşümün başlayacağı minimum gecikme süresi (saniye)")]
    public float minDelay = 0.0f;
    [Tooltip("Dönüşümün başlayacağı maksimum gecikme süresi (saniye)")]
    public float maxDelay = 0.4f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // --- KURAL MOTORU (Mermi bir şeye çarptığında) ---
    public void ProcessImpact(TransformableBlock hitBlock, ItemType thrownItem, Vector3 hitPos, GameObject projectile)
    {
        // KURAL 1: Dal veya Çiçek fırlatıldıysa hiçbir şeyi dönüştürmez, olduğu yere düşer.
        if (thrownItem.category == ItemCategory.Flower || thrownItem.category == ItemCategory.Stick)
        {
            DropItem(thrownItem, hitPos);
            Destroy(projectile);
            return;
        }

        // Eğer fırlatılan eşya dönüştürülebilir bir şeye çarpmadıysa (örneğin düz duvara çarptıysa) boşa gider.
        if (hitBlock == null)
        {
            Destroy(projectile);
            return;
        }

        // KURAL 2: Anahtar itemi kapıya fırlatıldığında kapı yok olur.
        if (hitBlock.currentItemType.category == ItemCategory.Door && thrownItem.category == ItemCategory.Key)
        {
            Destroy(hitBlock.gameObject);
            Debug.Log("Anahtar kapıyı açtı!");
            // İsterseniz buraya AudioManager.instance.PlaySFX(kapiSesi) eklenebilir.
            Destroy(projectile);
            return;
        }

        // KURAL 3: Normal Dönüşüm (Arkadaşının yazdığı tüm blokları dönüştüren fonksiyonu çalıştırıyoruz)
        if (hitBlock.currentItemType.category != ItemCategory.Door) // Kapılar taşa vs. dönüşmesin diye koruma
        {
            TransformAllBlocksOfType(hitBlock.currentItemType, thrownItem);
        }

        Destroy(projectile);
    }

    // --- Mermi hiçbir şeye çarpmadan durduğunda ---
    public void ProcessStop(ItemType thrownItem, Vector3 stopPos, GameObject projectile)
    {
        // Dal veya Çiçek fırlatıldı ve menzili bitip durduysa yine yere düşsün
        if (thrownItem.category == ItemCategory.Flower || thrownItem.category == ItemCategory.Stick)
        {
            DropItem(thrownItem, stopPos);
        }
        Destroy(projectile);
    }

    // Eşyayı yere düşürme yardımcı fonksiyonu
    private void DropItem(ItemType itemType, Vector3 pos)
    {
        if (collectiblePrefab != null)
        {
            GameObject dropped = Instantiate(collectiblePrefab, pos, Quaternion.identity);
            
            // Eğer CollectibleItem içinden türü atamak isterseniz:
            CollectibleItem col = dropped.GetComponent<CollectibleItem>();
            if (col != null) col.itemType = itemType;
        }
        Debug.Log($"Etkisiz eşya ({itemType.category}) yere düştü.");
    }

    // --- DALGA DALGA DÖNÜŞÜM FONKSİYONLARI ---

    // Oyuncunun kullandığı: Bütün blokları yeni türe dönüştürür (Gecikmeli)
    public static void TransformAllBlocksOfType(ItemType targetType, ItemType newType)
    {
        if (targetType == newType) return;
        if (Instance == null) return;

        TransformableBlock[] allBlocks = FindObjectsByType<TransformableBlock>(FindObjectsSortMode.None);

        foreach (TransformableBlock block in allBlocks)
        {
            if (block.currentItemType == targetType)
            {
                // Rastgele gecikme süresini hesapla
                float randomDelay = Random.Range(Instance.minDelay, Instance.maxDelay);
                
                // Gecikmeli dönüştürme işlemini başlat
                Instance.StartCoroutine(Instance.DelayedTransform(block, newType, randomDelay));
            }
        }
        
        Debug.Log($"'{targetType.name}' blokları rastgele sürelerde '{newType.name}' türüne dönüşmeye başladı!");
    }

    // Bekçinin (Enemy) Kullandığı: Bu türe dönüşmüş BÜTÜN blokları orijinal haline getirir (Gecikmeli)
    public static void RevertAllBlocksOfCurrentType(ItemType currentType)
    {
        if (Instance == null) return;

        TransformableBlock[] allBlocks = FindObjectsByType<TransformableBlock>(FindObjectsSortMode.None);

        foreach (TransformableBlock block in allBlocks)
        {
            if (block.isTransformed && block.currentItemType == currentType)
            {
                // Rastgele gecikme süresini hesapla
                float randomDelay = Random.Range(Instance.minDelay, Instance.maxDelay);
                
                // Gecikmeli geri döndürme işlemini başlat
                Instance.StartCoroutine(Instance.DelayedRevert(block, randomDelay));
            }
        }
        
        Debug.Log($"Sahnede '{currentType.name}' türüne dönüşmüş tüm bloklar topluca orijinal haline getiriliyor!");
    }

    // --- COROUTINE (GECİKTİRİCİ) METOTLAR ---

    private IEnumerator DelayedTransform(TransformableBlock block, ItemType newType, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (block != null) // Obje bekleme süresinde kapı gibi yok edilmiş olabilir, kontrol edelim
        {
            block.TransformBlock(newType);
        }
    }

    private IEnumerator DelayedRevert(TransformableBlock block, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (block != null)
        {
            block.RevertToOriginal();
        }
    }
}