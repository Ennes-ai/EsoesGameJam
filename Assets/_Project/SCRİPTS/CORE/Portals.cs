using UnityEngine;

public class Portals : MonoBehaviour
{
    [Header("Gidilecek Sahne Adı")]
    public string LoadingPortalName;

    [Header("Seviye Sonu Ayarları")]
    public bool isVictoryPortal = false; // Seviye sonundaki portal mı?
    public int unlockLevelIndex = 0;    // Eğer bu victory portalsa, hangi leveli açıyor? (Örn: 2)
}