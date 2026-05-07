using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class Main : MonoBehaviour
{
    private UIDocument _uiDocument;
    private Button _playButton;
    private Button _creditsButton;
    private Button _closeCreditsButton;
    private Button _settingsButton;
    private Button _closeSettingsButton;
    private Button _exitButton;
    private VisualElement _fadeOverlay;
    private VisualElement _loadingScreen;
    private VisualElement _loadingBarFill;
    private VisualElement _buttonsContainer;
    private VisualElement _creditsPanel;
    private VisualElement _settingsPanel;
    private Slider _musicSlider;
    private Slider _sfxSlider;

    private void OnEnable()
    {
        // Önceki sahneden oyun durdurulmuş (Pause) olarak dönüldüyse zamanı normale alıyoruz.
        Time.timeScale = 1f;

        _uiDocument = GetComponent<UIDocument>();
        var root = _uiDocument.rootVisualElement;

        // UI Builder'da (UXML) butonlarına verdiğin Name değerlerini buraya yazmalısın.
        // Şimdilik "PlayButton" ve "ExitButton" olarak farz ediyorum.
        _playButton = root.Q<Button>("Play");
        _exitButton = root.Q<Button>("Exit");
        _creditsButton = root.Q<Button>("Yapimcilar");
        _settingsButton = root.Q<Button>("Ayarlar");
        _closeCreditsButton = root.Q<Button>("CloseCredits");
        _closeSettingsButton = root.Q<Button>("CloseSettings");
        
        _creditsPanel = root.Q<VisualElement>("CreditsPanel");
        _settingsPanel = root.Q<VisualElement>("SettingsPanel");
        _musicSlider = root.Q<Slider>("MusicSlider");
        _sfxSlider = root.Q<Slider>("SfxSlider");

        // Siyah kararma ekranını buluyoruz
        _fadeOverlay = root.Q<VisualElement>("FadeOverlay");

        // Yükleme ekranı elemanlarını buluyoruz
        _loadingScreen = root.Q<VisualElement>("LoadingScreen");
        _loadingBarFill = root.Q<VisualElement>("LoadingBarFill");

        // Butonların konteynerini buluyoruz
        _buttonsContainer = root.Q<VisualElement>("Buttons");

        // Butonlar bulunduysa Click (Tıklama) eventlerini bağlıyoruz
        if (_playButton != null) _playButton.clicked += OnPlayButtonClicked;
        else Debug.LogWarning("Play butonunun ismini UI Builder'da 'PlayButton' yaptığından emin ol!");

        if (_exitButton != null) _exitButton.clicked += OnExitButtonClicked;
        else Debug.LogWarning("Exit butonunun ismini UI Builder'da 'ExitButton' yaptığından emin ol!");
        
        if (_creditsButton != null) _creditsButton.clicked += OnCreditsButtonClicked;
        if (_closeCreditsButton != null) _closeCreditsButton.clicked += OnCloseCreditsButtonClicked;
        if (_settingsButton != null) _settingsButton.clicked += OnSettingsButtonClicked;
        if (_closeSettingsButton != null) _closeSettingsButton.clicked += OnCloseSettingsButtonClicked;
        
        // Slider değiştiğinde eşzamanlı ses değiştirme tetikleyicisi
        if (_musicSlider != null) _musicSlider.RegisterValueChangedCallback(evt => OnMusicVolumeChanged(evt.newValue));
        if (_sfxSlider != null) _sfxSlider.RegisterValueChangedCallback(evt => OnSfxVolumeChanged(evt.newValue));

    }

    private void OnDisable()
    {
        // Script kapanırken veya obje yok olurken event aboneliklerini kaldırmak bellek yönetimi için iyi bir pratiktir.
        if (_playButton != null) _playButton.clicked -= OnPlayButtonClicked;
        if (_exitButton != null) _exitButton.clicked -= OnExitButtonClicked;
        
        if (_creditsButton != null) _creditsButton.clicked -= OnCreditsButtonClicked;
        if (_closeCreditsButton != null) _closeCreditsButton.clicked -= OnCloseCreditsButtonClicked;
        
        if (_settingsButton != null) _settingsButton.clicked -= OnSettingsButtonClicked;
        if (_closeSettingsButton != null) _closeSettingsButton.clicked -= OnCloseSettingsButtonClicked;
    }

    private void OnPlayButtonClicked()
    {
        Debug.Log("Oyna butonuna tıklandı, ekran kararıyor...");

        if (_buttonsContainer != null)
        {
            // Sahneyi yüklemeden önce animasyonun bitmesini bekleyen Coroutine'i başlatıyoruz
            StartCoroutine(LoadSceneAfterFade());
        }
        else
        {
            // Hata durumunda direkt yükle (Fail-safe)
            Debug.LogWarning("Hata: Buton konteyneri bulunamadı, direkt geçiş yapılıyor.");
            SceneManager.LoadScene(0); // Eski kodda "Lobby" yazıyordu, index 0 olarak güncelledik.
        }
    }

    private IEnumerator LoadSceneAfterFade()
    {
        // 1. Butonlar gizlenirken eş zamanlı olarak Yükleme Barı belirsin
        Debug.Log("Adım 1: Butonlar gizleniyor ve yükleme barı beliriyor...");
        _buttonsContainer.AddToClassList("buttons-container--hidden");
        if (_loadingScreen != null) _loadingScreen.AddToClassList("loading-screen--visible");
        
        yield return new WaitForSecondsRealtime(0.5f); // Butonların gizlenme animasyon süresi kadar bekle

        // 3. Arkada asenkron yüklemeyi başlat
        Debug.Log("Adım 3: 2 Saniyelik simülasyon barı ve arka plan yüklemesi başladı...");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Lobby");
        if (asyncLoad != null) asyncLoad.allowSceneActivation = false;
        
        // 4. Barın dolması için Timer (Min 2 Saniye sürecek)
        float timer = 0f;
        float loadingDuration = 2f;

        // Eğer 2 saniye dolmadıysa VEYA arka plandaki yükleme henüz bitmediyse beklemeye ve barı doldurmaya devam et
        while (timer < loadingDuration || (asyncLoad != null && asyncLoad.progress < 0.9f))
        {
            timer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(timer / loadingDuration);
            
            // Lineer olmayan (SmoothStep) dolum eğrisi: Yavaş başlar, ortada hızlanır, sona doğru yavaşlayıp durur.
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            if (_loadingBarFill != null)
                _loadingBarFill.style.width = new StyleLength(Length.Percent(easedProgress * 100f));

            yield return null;
        }

        // Dolum bitince tam dolu gözükmesi için minik bir bekleme
        if (_loadingBarFill != null) _loadingBarFill.style.width = new StyleLength(Length.Percent(100f));
        yield return new WaitForSecondsRealtime(0.2f);

        // 5. Tekrar karart
        Debug.Log("Adım 4: Yükleme bitti, oyun sahnesi için tekrar kararıyor...");
        _fadeOverlay.AddToClassList("fade-overlay--active");
        yield return new WaitForSecondsRealtime(1f);

        // 6. Yeni sahneye geç
        Debug.Log("Adım 5: Sahne aktif ediliyor, iyi oyunlar!");
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = true;
        }
    }

    private void OnCreditsButtonClicked()
    {
        // Panelin gizli class'ını silerek görünür yapan animasyonu tetikliyoruz
        if (_creditsPanel != null)
        {
            _creditsPanel.RemoveFromClassList("credits-panel--hidden");
        }
        AudioManager.instance?.PlaySFX(AudioManager.instance.tiklamaSesi); // İsteğe bağlı, varsa tıklama sesi çalar
    }

    private void OnCloseCreditsButtonClicked()
    {
        // Paneli tekrar küçültüp yok eden class'ı ekliyoruz
        if (_creditsPanel != null)
        {
            _creditsPanel.AddToClassList("credits-panel--hidden");
        }
        AudioManager.instance?.PlaySFX(AudioManager.instance.tiklamaSesi); // İsteğe bağlı
    }

    private void OnSettingsButtonClicked()
    {
        // Ayarlar menüsünü görünür yapar
        if (_settingsPanel != null)
        {
            _settingsPanel.RemoveFromClassList("settings-panel--hidden");
        }
        AudioManager.instance?.PlaySFX(AudioManager.instance.tiklamaSesi);
    }

    private void OnCloseSettingsButtonClicked()
    {
        // Ayarlar menüsünü gizler
        if (_settingsPanel != null)
        {
            _settingsPanel.AddToClassList("settings-panel--hidden");
        }
        AudioManager.instance?.PlaySFX(AudioManager.instance.tiklamaSesi);
    }

    private void OnMusicVolumeChanged(float value)
    {
        // AudioManager bulunduysa Müzik sesini slider değerine göre (0.0 - 1.0 arası) ayarlar
        if (AudioManager.instance != null && AudioManager.instance.musicSource != null)
        {
            AudioManager.instance.musicSource.volume = value / 100f;
        }
    }

    private void OnSfxVolumeChanged(float value)
    {
        // AudioManager bulunduysa Efekt sesini slider değerine göre ayarlar
        if (AudioManager.instance != null && AudioManager.instance.sfxSource != null)
        {
            AudioManager.instance.sfxSource.volume = value / 100f;
        }
    }

    private void OnExitButtonClicked()
    {
        // Build alınmış projede uygulamayı kapatır.

        Debug.Log("Oyundan çıkış yapıldı!");
        Application.Quit();
        
        // Editörde çalıştığını görebilmen için log bırakalım.
        
    }
}
