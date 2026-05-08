using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

 [RequireComponent(typeof(UIDocument))]
public class MainMenuUIManager : MonoBehaviour
{
    private UIDocument _uiDocument;
    private Button _playButton;
    private Button _exitButton;
    private Button _settingsButton;
    private Button _howToPlayButton;
    private Button _closeSettingsButton;
    private Button _closeHowToPlayButton;
    private VisualElement _settingsPanel;
    private VisualElement _howToPlayPanel;
    private Slider _musicSlider;
    private Slider _sfxSlider;
    private VisualElement _fadeOverlay;

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
        _settingsButton = root.Q<Button>("Ayarlar");
        _howToPlayButton = root.Q<Button>("NasilOynanir");
        _closeSettingsButton = root.Q<Button>("CloseSettings");
        _closeHowToPlayButton = root.Q<Button>("CloseHowToPlay");
        
        _settingsPanel = root.Q<VisualElement>("SettingsPanel");
        _howToPlayPanel = root.Q<VisualElement>("HowToPlayPanel");

        // Siyah kararma ekranını buluyoruz
        _fadeOverlay = root.Q<VisualElement>("FadeOverlay");

        // Butonlar bulunduysa Click (Tıklama) eventlerini bağlıyoruz
        if (_playButton != null) _playButton.clicked += OnPlayButtonClicked;
        else Debug.LogWarning("Play butonunun ismini UI Builder'da 'PlayButton' yaptığından emin ol!");

        if (_exitButton != null) _exitButton.clicked += OnExitButtonClicked;
        else Debug.LogWarning("Exit butonunun ismini UI Builder'da 'ExitButton' yaptığından emin ol!");
        
        if (_settingsButton != null) _settingsButton.clicked += OnSettingsButtonClicked;
        if (_closeSettingsButton != null) _closeSettingsButton.clicked += OnCloseSettingsButtonClicked;
        if (_howToPlayButton != null) _howToPlayButton.clicked += OnHowToPlayButtonClicked;
        if (_closeHowToPlayButton != null) _closeHowToPlayButton.clicked += OnCloseHowToPlayButtonClicked;
    }

    private void OnDisable()
    {
        // Script kapanırken veya obje yok olurken event aboneliklerini kaldırmak bellek yönetimi için iyi bir pratiktir.
        if (_playButton != null) _playButton.clicked -= OnPlayButtonClicked;
        if (_exitButton != null) _exitButton.clicked -= OnExitButtonClicked;
        if (_settingsButton != null) _settingsButton.clicked -= OnSettingsButtonClicked;
        if (_closeSettingsButton != null) _closeSettingsButton.clicked -= OnCloseSettingsButtonClicked;
        if (_howToPlayButton != null) _howToPlayButton.clicked -= OnHowToPlayButtonClicked;
        if (_closeHowToPlayButton != null) _closeHowToPlayButton.clicked -= OnCloseHowToPlayButtonClicked;
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
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(value);
        }
    }

    private void OnSfxVolumeChanged(float value)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSFXVolume(value);
        }
    }

    private void OnPlayButtonClicked()
    {
        Debug.Log("Oyna butonuna tıklandı, ekran kararıyor...");
        
        if (_fadeOverlay != null)
        {
            // Ekranı karartan CSS class'ını aktif ediyoruz
            _fadeOverlay.AddToClassList("fade-overlay--active");
            // Sahneyi yüklemeden önce animasyonun bitmesini bekleyen Coroutine'i başlatıyoruz
            StartCoroutine(LoadSceneAfterFade());
        }
        else
        {
            // Hata durumunda direkt yükle (Fail-safe)
            Debug.Log("Hata var");
            //SceneManager.LoadScene(0);
        }
    }

    private IEnumerator LoadSceneAfterFade()
    {
        // 1. UI Toolkit'e kararma animasyonunu başlatması için 1 kare (frame) izin veriyoruz.
        // Aksi takdirde asenkron yükleme ana iş parçacığını dondurup animasyonu bozabilir.
        yield return null;

        // 2. Sahneyi arka planda asenkron olarak yüklemeye başla
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(0);
        Debug.Log("Asenkron");
        if (asyncLoad != null)
        {
            // Sahne arkada yüklense bile biz izin verene kadar geçiş yapmasını engelle
            asyncLoad.allowSceneActivation = false;
        }
        else
        {
            Debug.LogError("Sahne Index 0 bulunamadı! File -> Build Settings menüsünden sahneleri eklediğine emin ol.");
        }

        // 3. Zaman durdurulmuş olsa bile animasyon süresi kadar beklemesi için Realtime kullanıyoruz.
        yield return new WaitForSecondsRealtime(1f);
        
        if (asyncLoad != null)
        {
            // 4. Eğer sahne çok büyükse ve 1 saniyede yüklenmesi bitmediyse, bitene kadar bekle
            while (asyncLoad.progress < 0.9f)
            {
                yield return null;
            }

            // 5. Ekran tamamen karardı ve sahne hazır, artık geçişe izin ver!
            asyncLoad.allowSceneActivation = true;
        }
    }

    private void OnExitButtonClicked()
    {
        // Build alınmış projede uygulamayı kapatır.
        Application.Quit();
        
        // Editörde çalıştığını görebilmen için log bırakalım.
        Debug.Log("Oyundan çıkış yapıldı!");
    }
}
