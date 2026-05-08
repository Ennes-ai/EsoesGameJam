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
    private Button _howToPlayButton;
    private Button _mainMenuButton;
    
    private VisualElement _settingsPanel;
    private VisualElement _howToPlayPanel;
    private Button _closeSettingsButton;
    private Button _closeHowToPlayButton;
    private Slider _musicSlider;
    private Slider _sfxSlider;
    private VisualElement _fadeOverlay;
    
    private VisualElement _inventoryPanel;
    private Label _levelText;
    private Button _hudRestartButton;
    private Button _hudExitButton;
    private int _lastInventoryCount = -1;
    private int _lastTotalItems = -1;
    private ItemType _lastSelectedItem = null;

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
        _howToPlayButton = root.Q<Button>("HowToPlayButton");
        _mainMenuButton = root.Q<Button>("MainMenuButton");

        // Butonlara tıklama olaylarını bağlıyoruz
        if (_resumeButton != null) _resumeButton.clicked += TogglePause;
        if (_mainMenuButton != null) _mainMenuButton.clicked += OnMainMenuButtonClicked;
        
        if (_howToPlayButton != null) _howToPlayButton.clicked += OnHowToPlayButtonClicked;
        else Debug.LogWarning("⚠️ LOBBY UI: 'HowToPlayButton' isminde bir buton bulunamadı! UI Builder'ı açıp adını tam olarak böyle yaptığından emin ol.");

        // Ayarlar menüsü elemanlarını UXML'den alıyoruz
        _settingsPanel = root.Q<VisualElement>("SettingsPanel");
        _howToPlayPanel = root.Q<VisualElement>("HowToPlayPanel");
        
        _closeSettingsButton = root.Q<Button>("CloseSettings");
        _closeHowToPlayButton = root.Q<Button>("CloseHowToPlay");
        
        _musicSlider = root.Q<Slider>("MusicSlider");
        _sfxSlider = root.Q<Slider>("SfxSlider");
        
        // Kararma efekti
        _fadeOverlay = root.Q<VisualElement>("FadeOverlay");

        if (_settingsButton != null) _settingsButton.clicked += OnSettingsButtonClicked;
        if (_closeSettingsButton != null) _closeSettingsButton.clicked += OnCloseSettingsButtonClicked;
        if (_closeHowToPlayButton != null) _closeHowToPlayButton.clicked += OnCloseHowToPlayButtonClicked;
        
        // Slider değiştiğinde eşzamanlı ses değiştirme tetikleyicisi
        if (_musicSlider != null) 
        {
            _musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 100f);
            _musicSlider.RegisterValueChangedCallback(evt => OnMusicVolumeChanged(evt.newValue));
        }
        else
        {
            Debug.LogWarning("⚠️ LOBBY UI: 'MusicSlider' isminde bir Slider bulunamadı! UI Builder'dan (Pause Menüsü) adını tam olarak 'MusicSlider' yaptığından emin ol.");
        }
        
        if (_sfxSlider != null) 
        {
            _sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 100f);
            _sfxSlider.RegisterValueChangedCallback(evt => OnSfxVolumeChanged(evt.newValue));
        }
        else
        {
            Debug.LogWarning("⚠️ LOBBY UI: 'SfxSlider' isminde bir Slider bulunamadı! UI Builder'dan (Pause Menüsü) adını tam olarak 'SfxSlider' yaptığından emin ol.");
        }
        
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

        BindButtonSounds(_resumeButton);
        BindButtonSounds(_settingsButton);
        BindButtonSounds(_mainMenuButton);
        BindButtonSounds(_closeSettingsButton);
        BindButtonSounds(_howToPlayButton);
        BindButtonSounds(_closeHowToPlayButton);
        BindButtonSounds(_hudRestartButton);
        BindButtonSounds(_hudExitButton);
    }

    private IEnumerator FadeInRoutine()
    {
        // UI Toolkit'in ilk karede siyah ekranı tam olarak çizip, CSS stillerini hesaplaması için
        // çok kısa bir bekleme ekliyoruz. Yoksa CSS Transition tetiklenmeden "pat" diye şeffaf olur.
        yield return new WaitForSecondsRealtime(0.15f);
        if (_fadeOverlay != null) _fadeOverlay.RemoveFromClassList("fade-overlay--active");
    }

    private void BindButtonSounds(Button btn)
    {
        if (btn == null) return;
        
        btn.RegisterCallback<PointerEnterEvent>(evt => {
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.hover);
        });
        btn.clicked += () => {
            if (AudioManager.instance != null) AudioManager.instance.PlaySFX(AudioManager.instance.tiklamaSesi);
        };
    }

    private void OnDisable()
    {
        // Bellek sızıntılarını önlemek için eventleri kaldırıyoruz
        if (_resumeButton != null) _resumeButton.clicked -= TogglePause;
        if (_settingsButton != null) _settingsButton.clicked -= OnSettingsButtonClicked;
        if (_howToPlayButton != null) _howToPlayButton.clicked -= OnHowToPlayButtonClicked;
        if (_closeSettingsButton != null) _closeSettingsButton.clicked -= OnCloseSettingsButtonClicked;
        if (_closeHowToPlayButton != null) _closeHowToPlayButton.clicked -= OnCloseHowToPlayButtonClicked;
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
            case "Level_1": _levelText.text = "L1"; break;
            case "Level_2": _levelText.text = "L2"; break;
            case "Level_3": _levelText.text = "L3"; break;
            case "Level_4": _levelText.text = "L4"; break;
            default: _levelText.text = "?"; break;
        }
    }

    private void UpdateInventoryUI()
    {
        if (_inventoryPanel == null) return;

        int currentCount = (PlayerEnvanter.Instance != null && PlayerEnvanter.Instance.inventorySlots != null) 
            ? PlayerEnvanter.Instance.inventorySlots.Count 
            : 0;
            
        int totalItems = 0;
        if (currentCount > 0)
        {
            foreach (var slot in PlayerEnvanter.Instance.inventorySlots) totalItems += slot.count;
        }

        ItemType currentItem = (PlayerEnvanter.Instance != null) ? PlayerEnvanter.Instance.currentItem : null;

        // İtem slotu sayısı, toplam eşya stack sayısı VEYA seçili eşya değiştiğinde UI'ı yenile
        if (currentCount != _lastInventoryCount || totalItems != _lastTotalItems || currentItem != _lastSelectedItem)
        {
            _lastInventoryCount = currentCount;
            _lastTotalItems = totalItems;
            _lastSelectedItem = currentItem;
            _inventoryPanel.Clear(); // Önceki UI itemlerini temizle

            if (currentCount == 0)
            {
                _inventoryPanel.style.display = DisplayStyle.None; // İtem yoksa paneli tamamen gizle
            }
            else
            {
                _inventoryPanel.style.display = DisplayStyle.Flex; // İtem varsa paneli göster

                int index = 1;
                foreach (var slot in PlayerEnvanter.Instance.inventorySlots)
                {
                    VisualElement itemUI = new VisualElement();
                    itemUI.AddToClassList("hud-inventory-item");
                    
                    // Eğer ScriptableObject'te sprite tanımlıysa arkaplan olarak ata
                    if (slot.itemType.itemSprite != null)
                    {
                        itemUI.style.backgroundImage = new StyleBackground(slot.itemType.itemSprite);
                    }

                    // İlgili eşyanın seçili olduğunu gösteren sarı çizgi efekti
                    if (slot.itemType == currentItem)
                    {
                        itemUI.style.borderBottomColor = Color.yellow;
                        itemUI.style.borderBottomWidth = 3;
                    }

                    // Sol üst köşede hangi tuş olduğunu belirten Label (1, 2, 3...)
                    Label keyLabel = new Label(index.ToString());
                    keyLabel.style.position = Position.Absolute;
                    keyLabel.style.top = 2; // Sol yukarıya hizalama
                    keyLabel.style.left = 4;
                    keyLabel.style.color = Color.white;
                    keyLabel.style.fontSize = 18;
                    keyLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

                    itemUI.Add(keyLabel);

                    // Eğer 1'den fazla aynı eşyadan varsa sağ üstte sayısını (Stack) göster
                    if (slot.count > 1)
                    {
                        Label stackLabel = new Label("x" + slot.count.ToString());
                        stackLabel.style.position = Position.Absolute;
                        stackLabel.style.top = 2; // Sağ yukarıya hizalama
                        stackLabel.style.right = 4;
                        stackLabel.style.color = Color.yellow;
                        stackLabel.style.fontSize = 16;
                        stackLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                        itemUI.Add(stackLabel);
                    }

                    _inventoryPanel.Add(itemUI);
                    index++;
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
    }

    private void OnCloseSettingsButtonClicked()
    {
        if (_settingsPanel != null) _settingsPanel.AddToClassList("settings-panel--hidden");
        if (_closeSettingsButton != null) _closeSettingsButton.pickingMode = PickingMode.Ignore;
    }

    private void OnHowToPlayButtonClicked()
    {
        if (_howToPlayPanel != null) 
        {
            _howToPlayPanel.RemoveFromClassList("settings-panel--hidden");
            _howToPlayPanel.pickingMode = PickingMode.Position;
        }
        if (_closeHowToPlayButton != null) _closeHowToPlayButton.pickingMode = PickingMode.Position;
    }

    private void OnCloseHowToPlayButtonClicked()
    {
        if (_howToPlayPanel != null) 
        {
            _howToPlayPanel.AddToClassList("settings-panel--hidden");
            _howToPlayPanel.pickingMode = PickingMode.Ignore;
        }
        if (_closeHowToPlayButton != null) _closeHowToPlayButton.pickingMode = PickingMode.Ignore;
    }

    private void OnMusicVolumeChanged(float value)
    {
        Debug.Log("Lobby UI Müzik Değişiyor: " + value);
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(value);
        }
        else Debug.LogWarning("⚠️ LOBBY UI: Sahnede AudioManager bulunamadı!");
    }

    private void OnSfxVolumeChanged(float value)
    {
        Debug.Log("Lobby UI SFX Değişiyor: " + value);
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSFXVolume(value);
        }
        else Debug.LogWarning("⚠️ LOBBY UI: Sahnede AudioManager bulunamadı!");
    }

    private void OnMainMenuButtonClicked()
    {
        StartCoroutine(FadeOutAndLoadMenu());
    }

    private void OnRestartButtonClicked()
    {
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

        // Sinematik başladığında kapanış müziğine geçiş yap
        if (AudioManager.instance != null && AudioManager.instance.endingMusic != null)
        {
            AudioManager.instance.PlayMusic(AudioManager.instance.endingMusic);
        }

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