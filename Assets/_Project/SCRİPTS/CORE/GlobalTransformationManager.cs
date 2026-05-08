using UnityEngine;
using System.Collections;
using System.Collections.Generic; // HashSet kullanabilmek için eklendi

public class GlobalTransformationManager : MonoBehaviour
{
    public static GlobalTransformationManager Instance;

    [Header("Dal/Çiçek yere düşerken kullanılacak toplanabilir Prefab")]
    public GameObject collectiblePrefab; 

    [Header("Dönüşüm Gecikme Ayarları")]
    public float minDelay = 0.0f;
    public float maxDelay = 0.4f;

    // Spam Koruması: Dönüşüm sırasında bekçinin sistemi çökertmesini engeller
    private HashSet<TransformableBlock> processingBlocks = new HashSet<TransformableBlock>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ProcessImpact(TransformableBlock hitBlock, ItemType thrownItem, Vector3 hitPos, GameObject projectile)
    {
        if (thrownItem.category == ItemCategory.Flower || thrownItem.category == ItemCategory.Stick)
        {
            DropItem(thrownItem, hitPos);
            Destroy(projectile);
            return;
        }

        if (hitBlock == null)
        {
            Destroy(projectile);
            return;
        }

        if (hitBlock.currentItemType.category == ItemCategory.Door && thrownItem.category == ItemCategory.Key)
        {
            Destroy(hitBlock.gameObject);
            Debug.Log("Anahtar kapıyı açtı!");
            Destroy(projectile);
            return;
        }

        if (hitBlock.currentItemType.category != ItemCategory.Door)
        {
            TransformAllBlocksOfType(hitBlock.currentItemType, thrownItem);
        }

        Destroy(projectile);
    }

    public void ProcessStop(ItemType thrownItem, Vector3 stopPos, GameObject projectile)
    {
        if (thrownItem.category == ItemCategory.Flower || thrownItem.category == ItemCategory.Stick)
        {
            DropItem(thrownItem, stopPos);
        }
        Destroy(projectile);
    }

    private void DropItem(ItemType itemType, Vector3 pos)
    {
        if (collectiblePrefab != null)
        {
            GameObject dropped = Instantiate(collectiblePrefab, pos, Quaternion.identity);
            CollectibleItem col = dropped.GetComponent<CollectibleItem>();
            if (col != null) col.itemType = itemType;
        }
    }

    public static void TransformAllBlocksOfType(ItemType targetType, ItemType newType)
    {
        if (targetType == newType) return;
        if (Instance == null) return;

        TransformableBlock[] allBlocks = FindObjectsByType<TransformableBlock>(FindObjectsSortMode.None);

        foreach (TransformableBlock block in allBlocks)
        {
            // YENİ: Dosyanın birebir aynısı olmasına gerek yok, KATEGORİSİ aynıysa (örn: ikisi de Wall) hepsi dönüşür
            if (block.currentItemType != null && block.currentItemType.category == targetType.category)
            {
                if (!Instance.processingBlocks.Contains(block))
                {
                    Instance.processingBlocks.Add(block); // Kilitle
                    float randomDelay = Random.Range(Instance.minDelay, Instance.maxDelay);
                    Instance.StartCoroutine(Instance.DelayedTransform(block, newType, randomDelay));
                }
            }
        }
    }

    public static void RevertAllBlocksOfCurrentType(ItemType currentType)
    {
        if (Instance == null) return;

        TransformableBlock[] allBlocks = FindObjectsByType<TransformableBlock>(FindObjectsSortMode.None);

        foreach (TransformableBlock block in allBlocks)
        {
            // YENİ: Taşa dönmüş olan BÜTÜN taşlar aynı anda eski (kendi kişisel) hallerine döner
            if (block.isTransformed && block.currentItemType != null && block.currentItemType.category == currentType.category)
            {
                if (!Instance.processingBlocks.Contains(block))
                {
                    Instance.processingBlocks.Add(block); // Kilitle
                    float randomDelay = Random.Range(Instance.minDelay, Instance.maxDelay);
                    Instance.StartCoroutine(Instance.DelayedRevert(block, randomDelay));
                }
            }
        }
    }

    private IEnumerator DelayedTransform(TransformableBlock block, ItemType newType, float delay)
    {
        yield return new WaitForSeconds(delay);
        processingBlocks.Remove(block); // Kilidi aç
        if (block != null) block.TransformBlock(newType);
    }

    private IEnumerator DelayedRevert(TransformableBlock block, float delay)
    {
        yield return new WaitForSeconds(delay);
        processingBlocks.Remove(block); // Kilidi aç
        if (block != null) block.RevertToOriginal();
    }
}