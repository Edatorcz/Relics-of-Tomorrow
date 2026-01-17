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
    [SerializeField] private AudioClip portalOpenSound; // Zvuk otevření
    [SerializeField] private AudioClip portalEnterSound; // Zvuk průchodu
    
    [Header("Transition Settings")]
    [SerializeField] private float transitionDelay = 0.5f; // Čekání před přechodem
    [SerializeField] private bool destroyPlayerOnTransition = true; // Zničit hráče před přechodem
    
    private bool isActive = false;
    private bool isTransitioning = false;
    private AudioSource audioSource;
    private Collider portalCollider;
    
    void Start()
    {
        // Setup komponent
        portalCollider = GetComponent<Collider>();
        portalCollider.isTrigger = true;
        
        // Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D zvuk
        
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
        
        // Přehrát zvuk
        if (audioSource != null && portalOpenSound != null)
        {
            audioSource.PlayOneShot(portalOpenSound);
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
        if (audioSource != null && portalEnterSound != null)
        {
            audioSource.PlayOneShot(portalEnterSound);
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
}
