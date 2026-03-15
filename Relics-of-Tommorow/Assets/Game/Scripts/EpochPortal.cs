using UnityEngine;
using System.Collections;

/// <summary>
/// Portál do další epochy
/// Objeví se po zabití bosse, hráč projde a přesune se do další epochy
/// </summary>
[RequireComponent(typeof(Collider))]
public class EpochPortal : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private float activationDelay = 1f; // Delay před aktivací po spawn
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem portalParticles; // Efekt portálu
    [SerializeField] private Light portalLight; // Světlo portálu
    [SerializeField] private Color portalColor = new Color(0.2f, 0.5f, 1f); // Modrá barva
    
    [Header("Audio")]
    [Tooltip("Volitelné - automaticky se najde PortalSoundManager.Instance pokud není přiřazeno")]
    [SerializeField] private PortalSoundManager soundManager;
    [Tooltip("Vlastní zvuky (volitelné - přepíšou PortalSoundManager)")]
    [SerializeField] private AudioClip customOpenSound;
    [SerializeField] private AudioClip customEnterSound;
    
    private PortalSoundManager SoundManager
    {
        get
        {
            if (soundManager == null)
            {
                soundManager = PortalSoundManager.Instance;
            }
            return soundManager;
        }
    }
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDelay = 0.5f; // Čekání před přechodem
    [SerializeField] private bool destroyPlayerOnTransition = true; // Zničit hráče před přechodem
    
    private bool isActive = false;
    private bool isTransitioning = false;
    private AudioSource audioSource;
    private AudioSource ambientAudioSource; // Samostatný AudioSource pro ambient loop
    private Collider portalCollider;
    
    void Start()
    {
        // Setup komponent
        portalCollider = GetComponent<Collider>();
        portalCollider.isTrigger = true;
        
        // Audio - hlavní AudioSource pro one-shot zvuky
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D zvuk
        
        // Druhý AudioSource pro ambient loop
        GameObject ambientObj = new GameObject("PortalAmbientSound");
        ambientObj.transform.SetParent(transform);
        ambientObj.transform.localPosition = Vector3.zero;
        ambientAudioSource = ambientObj.AddComponent<AudioSource>();
        ambientAudioSource.playOnAwake = false;
        ambientAudioSource.loop = true;
        ambientAudioSource.spatialBlend = 1f; // 3D zvuk
        ambientAudioSource.volume = 0f; // Začíná tichý (fade in)
        if (SoundManager != null && SoundManager.portalAmbientLoop != null)
        {
            ambientAudioSource.clip = SoundManager.portalAmbientLoop;
        }
        
        // Nastavení barvy světla
        if (portalLight != null)
        {
            portalLight.color = portalColor;
        }
        
        // Nastavení barvy částic
        if (portalParticles != null)
        {
            var main = portalParticles.main;
            main.startColor = portalColor;
        }
        
        // Deaktivovat vizuály dokud není aktivován (portál se aktivuje až po smrti bosse)
        SetVisualsActive(false);
        
        Debug.Log("EpochPortal: Portál vytvořen na pozici " + transform.position + " (deaktivovaný)");
    }
    
    /// <summary>
    /// Aktivovat portál po zpoždění
    /// </summary>
    private IEnumerator ActivatePortalAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        ActivatePortal();
    }
    
    /// <summary>
    /// Aktivovat portál
    /// </summary>
    public void ActivatePortal()
    {
        if (isActive) return;
        
        isActive = true;
        SetVisualsActive(true);
        
        // Přehrát zvuk otevření
        AudioClip openSound = customOpenSound != null ? customOpenSound : 
                              (SoundManager != null ? SoundManager.portalOpen : null);
        
        if (audioSource != null && openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }
        
        // Spustit ambient loop s fade in
        if (ambientAudioSource != null && ambientAudioSource.clip != null)
        {
            ambientAudioSource.Play();
            StartCoroutine(FadeAmbientSound(true));
        }
        
        Debug.Log("EpochPortal: Portál aktivován!");
    }
    
    /// <summary>
    /// Nastavit vizuály portálu
    /// </summary>
    private void SetVisualsActive(bool active)
    {
        if (portalParticles != null)
        {
            if (active)
                portalParticles.Play();
            else
                portalParticles.Stop();
        }
        
        if (portalLight != null)
        {
            portalLight.enabled = active;
        }
    }
    
    /// <summary>
    /// Hráč vešel do portálu
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (!isActive || isTransitioning) return;
        
        if (other.CompareTag("Player"))
        {
            Debug.Log("EpochPortal: Hráč vstoupil do portálu!");
            StartCoroutine(TransitionToNextEpoch(other.gameObject));
        }
    }
    
    /// <summary>
    /// Přejít do další epochy
    /// </summary>
    private IEnumerator TransitionToNextEpoch(GameObject player)
    {
        isTransitioning = true;
        
        // Přehrát zvuk průchodu
        AudioClip enterSound = customEnterSound != null ? customEnterSound : 
                               (SoundManager != null ? SoundManager.portalEnter : null);
        
        if (audioSource != null && enterSound != null)
        {
            audioSource.PlayOneShot(enterSound);
        }
        
        // Zakázat pohyb hráče
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        
        // Počkat chvíli (pro efekt)
        yield return new WaitForSeconds(transitionDelay);
        
        // POZNÁMKA: Inventář ZŮSTÁVÁ (portál = úspěšný přechod)
        // Inventář se maže pouze při smrti (EpochManager.OnPlayerDeath)
        Debug.Log("EpochPortal: Inventář zůstává (portál přechod)");
        
        // Smazat hráče před přechodem (aby se nezduplikoval)
        if (destroyPlayerOnTransition)
        {
            Debug.Log("EpochPortal: Ničím hráče před přechodem...");
            Destroy(player);
        }
        
        // Načíst další epochu
        if (EpochManager.Instance != null)
        {
            EpochManager.Instance.GoToNextEpoch();
        }
        else
        {
            Debug.LogError("EpochPortal: EpochManager.Instance není k dispozici! Zkusím najít...");
            EpochManager manager = FindFirstObjectByType<EpochManager>();
            if (manager != null)
            {
                Debug.Log("EpochPortal: EpochManager nalezen přes FindFirstObjectByType!");
                manager.GoToNextEpoch();
            }
            else
            {
                Debug.LogError("EpochPortal: EpochManager nenalezen! Načítám přímo scénu...");
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }
    
    /// <summary>
    /// Vizuální gizmo pro debug
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = portalColor;
        Gizmos.DrawWireSphere(transform.position, 2f);
        Gizmos.color = new Color(portalColor.r, portalColor.g, portalColor.b, 0.3f);
        Gizmos.DrawSphere(transform.position, 2f);
    }
    
    /// <summary>
    /// Fade in/out pro ambient zvuk
    /// </summary>
    private IEnumerator FadeAmbientSound(bool fadeIn)
    {
        if (ambientAudioSource == null || SoundManager == null) yield break;
        
        float targetVolume = fadeIn ? SoundManager.ambientLoopVolume : 0f;
        float startVolume = ambientAudioSource.volume;
        float fadeTime = SoundManager.ambientFadeTime;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            ambientAudioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / fadeTime);
            yield return null;
        }
        
        ambientAudioSource.volume = targetVolume;
        
        // Zastavit přehrávání pokud je fade out dokončený
        if (!fadeIn && ambientAudioSource.volume == 0f)
        {
            ambientAudioSource.Stop();
        }
    }
    
    /// <summary>
    /// Zavřít portál (fade out ambient + zvuk zavření)
    /// </summary>
    public void ClosePortal()
    {
        if (!isActive) return;
        
        isActive = false;
        
        // Přehrát zvuk zavření
        if (audioSource != null && SoundManager != null && SoundManager.portalClose != null)
        {
            audioSource.PlayOneShot(SoundManager.portalClose);
        }
        
        // Fade out ambient sound
        if (ambientAudioSource != null && ambientAudioSource.isPlaying)
        {
            StartCoroutine(FadeAmbientSound(false));
        }
        
        // Vypnout vizuály po krátkém zpoždění
        StartCoroutine(DelayedDeactivateVisuals());
    }
    
    /// <summary>
    /// Zpožděné vypnutí vizuálů
    /// </summary>
    private IEnumerator DelayedDeactivateVisuals()
    {
        yield return new WaitForSeconds(0.5f);
        SetVisualsActive(false);
    }
    
    void OnDestroy()
    {
        // Zastavit všechny zvuky při zničení
        if (ambientAudioSource != null)
        {
            ambientAudioSource.Stop();
        }
    }
}
