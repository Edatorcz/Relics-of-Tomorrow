using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Přidej tento komponet na tlačítko a automaticky přehraje zvuky při hover a click
/// Použije zvuky z MenuSoundManager
/// </summary>
[RequireComponent(typeof(Button))]
public class MenuButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Nastavení")]
    [Tooltip("Přehrát zvuk při najetí myší")]
    [SerializeField] private bool playHoverSound = true;
    
    [Tooltip("Přehrát zvuk při kliknutí")]
    [SerializeField] private bool playClickSound = true;
    
    [Header("Vlastní zvuky (volitelné)")]
    [Tooltip("Vlastní zvuk pro hover (pokud není nastaven, použije se z MenuSoundManager)")]
    [SerializeField] private AudioClip customHoverSound;
    
    [Tooltip("Vlastní zvuk pro click (pokud není nastaven, použije se z MenuSoundManager)")]
    [SerializeField] private AudioClip customClickSound;
    
    private Button button;
    private AudioSource audioSource;
    private MenuSoundManager soundManager;
    
    void Start()
    {
        button = GetComponent<Button>();
        
        // Najít nebo vytvořit AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound
        }
        
        // Načíst sound manager
        soundManager = MenuSoundManager.Instance;
    }
    
    /// <summary>
    /// Když myš najede na tlačítko
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHoverSound || !button.interactable) return;
        
        AudioClip sound = customHoverSound != null ? customHoverSound : 
                         (soundManager != null ? soundManager.buttonHover : null);
        
        if (sound != null && audioSource != null)
        {
            float volume = soundManager != null ? soundManager.uiVolume : 0.7f;
            audioSource.PlayOneShot(sound, volume);
        }
    }
    
    /// <summary>
    /// Když se klikne na tlačítko
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!playClickSound || !button.interactable) return;
        
        AudioClip sound = customClickSound != null ? customClickSound : 
                         (soundManager != null ? soundManager.buttonClick : null);
        
        if (sound != null && audioSource != null)
        {
            float volume = soundManager != null ? soundManager.uiVolume : 0.7f;
            audioSource.PlayOneShot(sound, volume);
        }
    }
    
    /// <summary>
    /// Veřejná metoda pro přehrání vlastního zvuku (můžeš volat z Button OnClick)
    /// </summary>
    public void PlayCustomSound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            float volume = soundManager != null ? soundManager.uiVolume : 0.7f;
            audioSource.PlayOneShot(clip, volume);
        }
    }
    
    /// <summary>
    /// Přehrát confirm sound
    /// </summary>
    public void PlayConfirmSound()
    {
        if (soundManager != null && soundManager.confirmSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundManager.confirmSound, soundManager.uiVolume);
        }
    }
    
    /// <summary>
    /// Přehrát error sound
    /// </summary>
    public void PlayErrorSound()
    {
        if (soundManager != null && soundManager.errorSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(soundManager.errorSound, soundManager.uiVolume);
        }
    }
}
