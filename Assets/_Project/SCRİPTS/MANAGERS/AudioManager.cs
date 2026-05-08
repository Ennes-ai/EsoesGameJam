using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;

    [Header("Audio Sources (Ses Kaynaklari)")]
    public AudioSource musicSource; // ambiyans ve main menu icin loop
    public AudioSource sfxSource;  // efektler icin

    [Header("muzik loop ve ambiyans")]
    public AudioClip mainMenuMusic;
    public AudioClip ambians;

    [Header("Ses Efektleri (SFX)")]
    public AudioClip homurdanmaSesi;
    public AudioClip hover;
    public AudioClip itemPickUp;
    public AudioClip itemThrowSound;
    public AudioClip popSound; // donusum sesi
    public AudioClip stepsSound;
    public AudioClip teleportSound;
    public AudioClip tiklamaSesi;

private void Awake()
    {
        // Sahneler arasi geciste yok olmasin
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Sahne yüklendiğinde tetiklenecek eventi dinliyoruz
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        LoadVolumeSettings();

        // Oyun direkt Editör'den herhangi bir levelde başlatıldığında da doğru müziği çalsın
        CheckAndPlayMusic(SceneManager.GetActiveScene().name);
    }

    private void LoadVolumeSettings()
    {
        if (audioMixer != null)
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 100f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 100f);
            
            SetMusicVolume(musicVol);
            SetSFXVolume(sfxVol);
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer == null) return;
        Debug.Log($"Setting Music Volume: {volume}");
        float linearVolume = Mathf.Clamp(volume / 100f, 0.0001f, 1f);
        audioMixer.SetFloat("MusicParam", Mathf.Log10(linearVolume) * 20f);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer == null) return;
        Debug.Log($"Setting SFX Volume: {volume}");
        float linearVolume = Mathf.Clamp(volume / 100f, 0.0001f, 1f);
        audioMixer.SetFloat("SFXParam", Mathf.Log10(linearVolume) * 20f);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unity AudioMixer sahne geçişlerinde değerini unutabileceği için ayarları tekrar uyguluyoruz
        LoadVolumeSettings();
        
        // Yeni sahne yüklendiğinde müziği kontrol et
        CheckAndPlayMusic(scene.name);
    }

    public void CheckAndPlayMusic(string sceneName)
    {
        Debug.Log($"Checking music for scene: {sceneName}");
        if (sceneName == "MainMenu") PlayMusic(mainMenuMusic);
        else PlayMusic(ambians); // MainMenu haricindeki tüm levellerde ve Lobide Ambiyans çalar
    }

    public void PlayMusic(AudioClip musicClip)
    {
        // Eğer çalınması istenen müzik null ise uyarı ver ve müziği durdur.
        if (musicClip == null)
        {
            Debug.LogWarning("PlayMusic fonksiyonuna 'null' bir klip verildi. Müzik durdurulacak. AudioManager objesinde ilgili müzik klibinin (örn: Ambians) atandığından emin olun.");
            musicSource.Stop();
            return;
        }

        // Eğer doğru müzik zaten çalıyorsa, tekrar başlatmaya gerek yok.
        if (musicSource.clip == musicClip && musicSource.isPlaying) return;

        // Müziği değiştir ve çal. Bu, klibin değiştiği veya aynı klibin durduğu durumları çözer.
        musicSource.clip = musicClip;
        musicSource.loop = true; // muzikler dongude kalsin
        musicSource.Play();
    }

    // standart ses efekti calmak icin (ust uste binebilir)
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayPopSound()
    {
        sfxSource.pitch = Random.Range(0.85f, 1.15f); // Sesi rastgele inceltip kalinlastirir
        sfxSource.PlayOneShot(popSound);
        
        // Bir sonraki standart sesler bozulmasın diye pitch'i 0.1 saniye sonra normale (1f) dondurmek icin ufak bir reset gerekmis
        Invoke(nameof(ResetPitch), 0.1f);
    }

    public void PlayStepSound()
    {
        sfxSource.pitch = Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(stepsSound);
        Invoke(nameof(ResetPitch), 0.1f);
    }

    private void ResetPitch()
    {
        sfxSource.pitch = 1f;
    }
}
