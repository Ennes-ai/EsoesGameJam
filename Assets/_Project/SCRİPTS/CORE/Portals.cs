using UnityEngine;

public class Portals : MonoBehaviour
{
    [Header("Gidilecek Sahne Adı")]
    public string LoadingPortalName;

    [Header("Seviye Sonu Ayarları")]
    public bool isVictoryPortal = false; // Seviye sonundaki portal mı?
    public int unlockLevelIndex = 0;    // Eğer bu victory portalsa, hangi leveli açıyor? (Örn: 2)

    private bool isLocked = false;
    private SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Portal bir zafer portalı değilse (yani lobi içerisindeki giriş portallarındansa) kilit durumunu kontrol et
        if (!isVictoryPortal)
        {
            int reachedLevel = PlayerPrefs.GetInt("ReachedLevel", 0);
            
            // İsimlerde yanlışlıkla bırakılmış boşluklar varsa (örn: "Level_1 ") onları temizliyoruz
            string pName = LoadingPortalName != null ? LoadingPortalName.Trim() : "";

            if (pName == "Level_1" && reachedLevel < 1) isLocked = true;
            else if (pName == "Level_2" && reachedLevel < 2) isLocked = true;
            else if (pName == "Level_3" && reachedLevel < 3) isLocked = true;
        }
    }

    private void Update()
    {
        // Eğer portalda animasyon (Animator) varsa rengi ezmesini engellemek için rengi zorla uyguluyoruz
        if (!isVictoryPortal && sr != null)
        {
            if (isLocked) sr.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Kilitliyse karanlık yap
            else sr.color = Color.white; // Açıksa normal kalsın
        }
    }
}