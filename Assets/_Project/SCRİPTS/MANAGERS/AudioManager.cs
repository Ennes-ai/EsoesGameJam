using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

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
    void Start()
    {
        PlayMusic(mainMenuMusic);
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicSource.clip == musicClip) return;
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
