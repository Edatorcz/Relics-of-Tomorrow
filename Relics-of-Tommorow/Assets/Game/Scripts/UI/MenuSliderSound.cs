using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Přidej tento komponent na Slider pro přehrání zvuku při pohybu
/// </summary>
[RequireComponent(typeof(Slider))]
public class MenuSliderSound : MonoBehaviour
{
    [Header("Nastavení")]
    [Tooltip("Minimální interval mezi přehráními zvuku (sekundy)")]
    [SerializeField] private float soundCooldown = 0.1f;
    
    [Tooltip("Přehrát zvuk pouze při uvolnění slideru (ne při pohybu)")]
    [SerializeField] private bool onlyPlayOnRelease = false;
    
    [Header("Vlastní zvuk (volitelné)")]
    [Tooltip("Vlastní zvuk (pokud není nastaven, použije se sliderMove z MenuSoundManager)")]
    [SerializeField] private AudioClip customSound;
    
    private Slider slider;
    private AudioSource audioSource;
    private MenuSoundManager soundManager;
    private float lastSoundTime = -999f;
    private float previousValue;
    
    void Start()
    {
        slider = GetComponent<Slider>();
        previousValue = slider.value;
        
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
        
        // Přidat listener na slider
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }
    
    void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }
    
    void OnSliderValueChanged(float value)
    {
        // Přehrát zvuk pouze pokud se hodnota změnila
        if (Mathf.Abs(value - previousValue) < 0.001f) return;
        
        previousValue = value;
        
        // Pokud je nastaveno pouze při release, přeskoč
        if (onlyPlayOnRelease) return;
        
        // Cooldown check
        if (Time.time - lastSoundTime < soundCooldown) return;
        
        PlaySound();
    }
    
    void PlaySound()
    {
        AudioClip sound = customSound != null ? customSound : 
                         (soundManager != null ? soundManager.sliderMove : null);
        
        if (sound != null && audioSource != null)
        {
            float volume = soundManager != null ? soundManager.uiVolume : 0.7f;
            audioSource.PlayOneShot(sound, volume * 0.5f); // Trochu tišší pro slider
            lastSoundTime = Time.time;
        }
    }
    
    /// <summary>
    /// Volej tuto metodu při uvolnění slideru (přidej ji na OnPointerUp event)
    /// </summary>
    public void OnSliderReleased()
    {
        if (onlyPlayOnRelease)
        {
            PlaySound();
        }
    }
}
