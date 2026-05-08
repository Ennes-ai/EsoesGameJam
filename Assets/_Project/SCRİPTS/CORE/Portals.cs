using UnityEngine;

public class Portals : MonoBehaviour
{
    [Header("Gidilecek Sahne Adı")]
    public string LoadingPortalName;

    [Header("Seviye Sonu Ayarları")]
    public bool isVictoryPortal = false; // Seviye sonundaki portal mı?
    public int unlockLevelIndex = 0;    // Eğer bu victory portalsa, hangi leveli açıyor? (Örn: 2)

    private void Start()
    {
        // Portal bir zafer portalı değilse (yani lobi içerisindeki giriş portallarındansa) kilit durumunu kontrol et
        if (!isVictoryPortal)
        {
            int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 0);
            bool isLocked = false;

            if (LoadingPortalName == "Level_1" && reachedLevel < 1) isLocked = true;
            else if (LoadingPortalName == "Level_2" && reachedLevel < 2) isLocked = true;
            else if (LoadingPortalName == "Level_3" && reachedLevel < 3) isLocked = true;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (isLocked)
                    sr.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Kilitliyse karanlık/gri yap
                else
                    sr.color = Color.white; // Açıksa orijinal/normal renginde kalsın
            }
        }
    }
}