using UnityEngine;

public class GlobalTransformationManager : MonoBehaviour
{

    public static GlobalTransformationManager Instance;

    [Header("Dal/Çiçek yere düşerken kullanılacak toplanabilir Prefab")]
    public GameObject collectiblePrefab; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // --- YENİ: KURAL MOTORU (Mermi bir şeye çarptığında) ---
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

    // --- YENİ: Mermi hiçbir şeye çarpmadan durduğunda ---
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

    // Oyuncunun kullandığı: Bütün blokları yeni türe dönüştürür
    public static void TransformAllBlocksOfType(ItemType targetType, ItemType newType)
    {
        if (targetType == newType) return;

        // Unity 2023+ için FindObjectsByType kullanımı (FindObjectsOfType yerine)
        TransformableBlock[] allBlocks = FindObjectsByType<TransformableBlock>(FindObjectsSortMode.None);

        foreach (TransformableBlock block in allBlocks)
        {
            if (block.currentItemType == targetType)
            {
                block.TransformBlock(newType);
            }
        }
        
        Debug.Log($"Sahnede bulunan tüm '{targetType.name}' blokları '{newType.name}' olarak dönüştürüldü!");
    }

    // YENİ - Bekçinin (Enemy) Kullandığı: Bu türe dönüşmüş BÜTÜN blokları orijinal haline getirir
    public static void RevertAllBlocksOfCurrentType(ItemType currentType)
    {
        TransformableBlock[] allBlocks = FindObjectsByType<TransformableBlock>(FindObjectsSortMode.None);

        foreach (TransformableBlock block in allBlocks)
        {
            // Eğer blok dönüştürülmüşse VE şu anki türü bekçinin hedef aldığı türle aynıysa
            if (block.isTransformed && block.currentItemType == currentType)
            {
                block.RevertToOriginal();
            }
        }
        
        Debug.Log($"Sahnede '{currentType.name}' türüne dönüşmüş tüm bloklar topluca orijinal haline getirildi!");
    }
}