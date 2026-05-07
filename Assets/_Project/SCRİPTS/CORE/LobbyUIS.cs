using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UIDocument))]
public class LobbyUIS : MonoBehaviour
{
    private VisualElement _pausePanel;
    private VisualElement _pauseOverlay;
    private Button _resumeButton;
    private Button _settingsButton;
    private Button _mainMenuButton;
    
    private VisualElement _settingsPanel;
    private Button _closeSettingsButton;
    private Slider _musicSlider;
    private Slider _sfxSlider;
    private VisualElement _fadeOverlay;
    
    private VisualElement _inventoryPanel;
    private Label _levelText;
    private Button _hudRestartButton;
    private Button _hudExitButton;
    private int _lastInventoryCount = -1;

    private VisualElement _endingScreen;
    private Label _endingMessage;
    private VisualElement _endingCredits;

    private bool _isPaused = false;

    private void OnEnable()
    {
        // Sahne ilk yüklendiğinde oyunun duraklatılmış kalmadığından emin oluyoruz
        Time.timeScale = 1f;

        var root = GetComponent<UIDocument>().rootVisualElement;

        // Pause menüsü elemanlarını UXML'den alıyoruz
        _pausePanel = root.Q<VisualElement>("PausePanel");
        _pauseOverlay = root.Q<VisualElement>("PauseOverlay");
        _resumeButton = root.Q<Button>("ResumeButton");
        _settingsButton = root.Q<Button>("SettingsButton");
        _mainMenuButton = root.Q<Button>("MainMenuButton");

        // Butonlara tıklama olaylarını bağlıyoruz
        if (_resumeButton != null) _resumeButton.clicked += TogglePause;
        if (_mainMenuButton != null) _mainMenuButton.clicked += OnMainMenuButtonClicked;

        // Ayarlar menüsü elemanlarını UXML'den alıyoruz
        _settingsPanel = root.Q<VisualElement>("SettingsPanel");
        _closeSettingsButton = root.Q<Button>("CloseSettings");
        _musicSlider = root.Q<Slider>("MusicSlider");
        _sfxSlider = root.Q<Slider>("SfxSlider");
        
        // Kararma efekti
        _fadeOverlay = root.Q<VisualElement>("FadeOverlay");

        if (_settingsButton != null) _settingsButton.clicked += OnSettingsButtonClicked;
        if (_closeSettingsButton != null) _closeSettingsButton.clicked += OnCloseSettingsButtonClicked;
        
        // Slider değiştiğinde eşzamanlı ses değiştirme tetikleyicisi
        if (_musicSlider != null) _musicSlider.RegisterValueChangedCallback(evt => OnMusicVolumeChanged(evt.newValue));
        if (_sfxSlider != null) _sfxSlider.RegisterValueChangedCallback(evt => OnSfxVolumeChanged(evt.newValue));
        
        // Oyun Sonu (Ending) Elemanları
        _endingScreen = root.Q<VisualElement>("EndingScreen");
        _endingMessage = root.Q<Label>("EndingMessage");
        _endingCredits = root.Q<VisualElement>("EndingCredits");

        // HUD Elemanlarını UXML'den alıyoruz
        _inventoryPanel = root.Q<VisualElement>("InventoryPanel");
        _levelText = root.Q<Label>("LevelText");
        _hudRestartButton = root.Q<Button>("RestartButton");
        _hudExitButton = root.Q<Button>("ExitMenuButton");

        if (_hudRestartButton != null) _hudRestartButton.clicked += OnRestartButtonClicked;
        if (_hudExitButton != null) _hudExitButton.clicked += OnMainMenuButtonClicked; // Exit butonu Ana Menüye atar

        SetLevelText();

        // Sahne başlarken karanlıktan aydınlanma animasyonunu (Fade In) başlat
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        // UI Toolkit'in ilk karede siyah ekranı tam olarak çizip, CSS stillerini hesaplaması için
        // çok kısa bir bekleme ekliyoruz. Yoksa CSS Transition tetiklenmeden "pat" diye şeffaf olur.
        yield return new WaitForSecondsRealtime(0.15f);
        if (_fadeOverlay != null) _fadeOverlay.RemoveFromClassList("fade-overlay--active");
    }

    private void OnDisable()
    {
        // Bellek sızıntılarını önlemek için eventleri kaldırıyoruz
        if (_resumeButton != null) _resumeButton.clicked -= TogglePause;
        if (_settingsButton != null) _settingsButton.clicked -= OnSettingsButtonClicked;
        if (_closeSettingsButton != null) _closeSettingsButton.clicked -= OnCloseSettingsButtonClicked;
        if (_mainMenuButton != null) _mainMenuButton.clicked -= OnMainMenuButtonClicked;

        if (_hudRestartButton != null) _hudRestartButton.clicked -= OnRestartButtonClicked;
        if (_hudExitButton != null) _hudExitButton.clicked -= OnMainMenuButtonClicked;
    }

    void Update()
    {
        // ESC tuşuna basıldığında pause menüsünü aç/kapat
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        UpdateInventoryUI();
    }

    private void SetLevelText()
    {
        if (_levelText == null) return;
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName)
        {
            case "Lobby": _levelText.text = "L"; break;
            case "Level_0": _levelText.text = "L0"; break;
            case "SampleScene": _levelText.text = "L1"; break;
            case "Level_2": _levelText.text = "L2"; break;
            case "Level_3": _levelText.text = "L3"; break;
            case "Level_4": _levelText.text = "L4"; break;
            default: _levelText.text = "?"; break;
        }
    }

    private void UpdateInventoryUI()
    {
        if (_inventoryPanel == null) return;

        // PlayerEnvanter.Instance yoksa veya liste null ise sayıyı 0 kabul et
        int currentCount = (PlayerEnvanter.Instance != null && PlayerEnvanter.Instance.collectedItems != null) 
            ? PlayerEnvanter.Instance.collectedItems.Count 
            : 0;

        if (currentCount != _lastInventoryCount)
        {
            _lastInventoryCount = currentCount;
            _inventoryPanel.Clear(); // Önceki UI itemlerini temizle

            if (currentCount == 0)
            {
                _inventoryPanel.style.display = DisplayStyle.None; // İtem yoksa paneli tamamen gizle
            }
            else
            {
                _inventoryPanel.style.display = DisplayStyle.Flex; // İtem varsa paneli göster

                foreach (var item in PlayerEnvanter.Instance.collectedItems)
                {
                    VisualElement itemUI = new VisualElement();
                    itemUI.AddToClassList("hud-inventory-item");
                    _inventoryPanel.Add(itemUI);
                }
            }
        }
    }

    private void TogglePause()
    {
        _isPaused = !_isPaused;

        // Zamanı durdur/başlat ve panelleri göster/gizle
        Time.timeScale = _isPaused ? 0f : 1f;

        if (_isPaused)
        {
            _pausePanel.RemoveFromClassList("pause-panel--hidden");
            _pauseOverlay.RemoveFromClassList("pause-overlay--hidden");
        }
        else
        {
            _pausePanel.AddToClassList("pause-panel--hidden");
            _pauseOverlay.AddToClassList("pause-overlay--hidden");
        }
    }

    private void OnSettingsButtonClicked()
    {
        if (_settingsPanel != null) _settingsPanel.RemoveFromClassList("settings-panel--hidden");
        if (_closeSettingsButton != null) _closeSettingsButton.pickingMode = PickingMode.Position;
        AudioManager.instance?.PlaySFX(AudioManager.instance.tiklamaSesi);
    }

    private void OnCloseSettingsButtonClicked()
    {
        if (_settingsPanel != null) _settingsPanel.AddToClassList("settings-panel--hidden");
        if (_closeSettingsButton != null) _closeSettingsButton.pickingMode = PickingMode.Ignore;
        AudioManager.instance?.PlaySFX(AudioManager.instance.tiklamaSesi);
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.instance != null && AudioManager.instance.musicSource != null)
            AudioManager.instance.musicSource.volume = value / 100f;
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (AudioManager.instance != null && AudioManager.instance.sfxSource != null)
            AudioManager.instance.sfxSource.volume = value / 100f;
    }

    private void OnMainMenuButtonClicked()
    {
        AudioManager.instance?.PlaySFX(AudioManager.instance.tiklamaSesi);
        StartCoroutine(FadeOutAndLoadMenu());
    }

    private void OnRestartButtonClicked()
    {
        AudioManager.instance?.PlaySFX(AudioManager.instance.tiklamaSesi);
        StartCoroutine(FadeOutAndReloadScene());
    }

    private IEnumerator FadeOutAndLoadMenu()
    {
        Time.timeScale = 1f; // Sahneden ayrılmadan önce zamanı normale döndürdüğümüzden emin olalım
        
        if (_fadeOverlay != null)
        {
            _fadeOverlay.AddToClassList("fade-overlay--active");
        }
        
        // Gerçek zamanlı bekleme (TimeScale 0 olsa bile çalışır)
        yield return new WaitForSecondsRealtime(1f);
        
        SceneManager.LoadScene("MainMenu"); // Main Menu geçişi
    }

    private IEnumerator FadeOutAndReloadScene()
    {
        Time.timeScale = 1f;
        
        if (_fadeOverlay != null)
        {
            _fadeOverlay.AddToClassList("fade-overlay--active");
        }
        
        yield return new WaitForSecondsRealtime(1f);
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Aktif sahneyi yeniden yükle
    }

    // Player portalden geçtiğinde dışarıdan çağrılacak yeni fonksiyon
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(FadeOutAndLoadSceneRoutine(sceneName));
    }

    private IEnumerator FadeOutAndLoadSceneRoutine(string sceneName)
    {
        Time.timeScale = 1f;
        if (_fadeOverlay != null) _fadeOverlay.AddToClassList("fade-overlay--active");
        yield return new WaitForSecondsRealtime(1f);
        SceneManager.LoadScene(sceneName);
    }

    // Level_4 bittiğinde çağrılacak özel sinematik geçiş
    public void PlayEndingSequence()
    {
        StartCoroutine(EndingSequenceRoutine());
    }

    private IEnumerator EndingSequenceRoutine()
    {
        Time.timeScale = 1f;

        // 1. Ekran yavaşça kararır
        if (_fadeOverlay != null) _fadeOverlay.AddToClassList("fade-overlay--active");
        yield return new WaitForSecondsRealtime(1.5f);

        // 2. Kararma bitince siyah renkli Oyun Sonu ekranı görünür yapılır
        if (_endingScreen != null) _endingScreen.RemoveFromClassList("ending-screen--hidden");
        
        // 3. Ekranda sadece oyunun adı (VIOLATOR) varken 2 saniye bekle
        yield return new WaitForSecondsRealtime(2f);

        // 4. "Son..." yazısı ve Yapımcılar yavaşça belirsin (CSS sayesinde 2 saniye sürecek)
        if (_endingMessage != null) _endingMessage.RemoveFromClassList("ending-hidden-text");
        if (_endingCredits != null) _endingCredits.RemoveFromClassList("ending-hidden-text");

        // 5. Oyuncu yazıları okusun diye 8 saniye bekle
        yield return new WaitForSecondsRealtime(8f);

        // 6. Sadece yazıları karartıp ekranı simsiyah yap (Fade Out)
        if (_endingScreen != null) _endingScreen.AddToClassList("ending-screen--hidden");
        yield return new WaitForSecondsRealtime(1.5f);

        SceneManager.LoadScene("MainMenu");
    }
}