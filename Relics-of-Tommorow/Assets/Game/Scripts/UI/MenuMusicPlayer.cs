using UnityEngine;

/// <summary>
/// Přehrává background hudbu v menu
/// Umísti na hlavní Menu objekt nebo vytvoř samostatný GameObject "MenuMusicPlayer"
/// </summary>
public class MenuMusicPlayer : MonoBehaviour
{
    [Header("Nastavení")]
    [Tooltip("Přehrávat hudbu automaticky při načtení scény")]
    [SerializeField] private bool playOnStart = true;
    
    [Tooltip("Pokračovat v přehrávání při přechodu mezi scénami")]
    [SerializeField] private bool dontDestroyOnLoad = false;
    
    [Tooltip("Fade in čas při spuštění hudby (sekundy)")]
    [SerializeField] private float fadeInTime = 2f;
    
    [Tooltip("Fade out čas při zastavení hudby (sekundy)")]
    [SerializeField] private float fadeOutTime = 1f;
    
    [Header("Vlastní hudba (volitelné)")]
    [Tooltip("Vlastní AudioClip (pokud není nastaveno, použije se mainMenuMusic z MenuSoundManager)")]
    [SerializeField] private AudioClip customMusic;
    
    private AudioSource audioSource;
    private MenuSoundManager soundManager;
    private float targetVolume;
    private bool isFading = false;
    private float fadeDirection = 0f; // 1 = fade in, -1 = fade out
    
    void Start()
    {
        Initialize();
        
        if (playOnStart)
        {
            PlayMusic();
        }
    }
    
    void Initialize()
    {
        soundManager = MenuSoundManager.Instance;
        
        // Najít nebo vytvořit AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.volume = 0f; // Začít tichý pro fade in
        
        // Don't destroy on load pokud je nastaveno
        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
    
    void Update()
    {
        if (isFading)
        {
            HandleFade();
        }
    }
    
    void HandleFade()
    {
        float fadeSpeed = fadeDirection > 0 ? (targetVolume / fadeInTime) : (targetVolume / fadeOutTime);
        audioSource.volume += fadeDirection * fadeSpeed * Time.deltaTime;
        
        // Ukončit fade
        if (fadeDirection > 0 && audioSource.volume >= targetVolume)
        {
            audioSource.volume = targetVolume;
            isFading = false;
        }
        else if (fadeDirection < 0 && audioSource.volume <= 0f)
        {
            audioSource.volume = 0f;
            audioSource.Stop();
            isFading = false;
        }
    }
    
    /// <summary>
    /// Spustí přehrávání hudby s fade in
    /// </summary>
    public void PlayMusic()
    {
        AudioClip musicClip = customMusic != null ? customMusic : 
                             (soundManager != null ? soundManager.mainMenuMusic : null);
        
        if (musicClip == null)
        {
            Debug.LogWarning("MenuMusicPlayer: Žádná hudba nebyla přiřazena!");
            return;
        }
        
        audioSource.clip = musicClip;
        targetVolume = soundManager != null ? soundManager.musicVolume : 0.5f;
        
        audioSource.Play();
        
        // Fade in
        if (fadeInTime > 0)
        {
            audioSource.volume = 0f;
            isFading = true;
            fadeDirection = 1f;
        }
        else
        {
            audioSource.volume = targetVolume;
        }
        
        Debug.Log($"MenuMusicPlayer: Přehrávám hudbu - {musicClip.name}");
    }
    
    /// <summary>
    /// Zastaví přehrávání hudby s fade out
    /// </summary>
    public void StopMusic()
    {
        if (!audioSource.isPlaying) return;
        
        // Fade out
        if (fadeOutTime > 0)
        {
            isFading = true;
            fadeDirection = -1f;
            targetVolume = audioSource.volume;
        }
        else
        {
            audioSource.Stop();
        }
    }
    
    /// <summary>
    /// Pozastaví hudbu
    /// </summary>
    public void PauseMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
    
    /// <summary>
    /// Pokračuje v pozastaveném přehrávání
    /// </summary>
    public void ResumeMusic()
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.UnPause();
        }
    }
    
    /// <summary>
    /// Nastaví hlasitost hudby
    /// </summary>
    public void SetVolume(float volume)
    {
        targetVolume = Mathf.Clamp01(volume);
        if (!isFading)
        {
            audioSource.volume = targetVolume;
        }
    }
    
    /// <summary>
    /// Změní hudbu na jinou
    /// </summary>
    public void ChangeMusic(AudioClip newMusic, bool fadeTransition = true)
    {
        if (fadeTransition && audioSource.isPlaying)
        {
            // Fade out současné hudby
            StopMusic();
            // Po dokončení fade out spustit novou hudbu
            StartCoroutine(WaitAndPlayNewMusic(newMusic));
        }
        else
        {
            customMusic = newMusic;
            audioSource.Stop();
            PlayMusic();
        }
    }
    
    private System.Collections.IEnumerator WaitAndPlayNewMusic(AudioClip newMusic)
    {
        yield return new WaitForSeconds(fadeOutTime);
        customMusic = newMusic;
        PlayMusic();
    }
    
    /// <summary>
    /// Vrátí true pokud hudba hraje
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }
}
