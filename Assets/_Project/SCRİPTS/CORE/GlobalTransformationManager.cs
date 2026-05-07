using UnityEngine;

public class GlobalTransformationManager : MonoBehaviour
{
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