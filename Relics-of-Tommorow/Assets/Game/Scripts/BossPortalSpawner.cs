using UnityEngine;

/// <summary>
/// Komponent pro bosse - po smrti spawne portál do další epochy
/// Připoj tento script na každého bosse ve hře
/// </summary>
public class BossPortalSpawner : MonoBehaviour
{
    [Header("Portal Settings")]
    [SerializeField] private float activationDelay = 2f; // Delay před aktivací portálu po smrti bosse
    
    [Header("Boss Reference")]
    [SerializeField] private EnemyBase bossEnemy; // Reference na boss enemáka
    
    private GameObject portal; // Reference na existující portál (nastaveno generátorem)
    private bool hasActivatedPortal = false;
    
    void Start()
    {
        // Najít boss komponenty
        if (bossEnemy == null)
        {
            bossEnemy = GetComponent<EnemyBase>();
        }
        
        if (bossEnemy == null)
        {
            Debug.LogError("BossPortalSpawner: Nelze najít EnemyBase komponent!");
            return;
        }
        
        // Přihlásit se k události smrti
        bossEnemy.OnDeath += OnBossDeath;
        
        Debug.Log("BossPortalSpawner: Připraven na " + gameObject.name);
    }
    
    void OnDestroy()
    {
        // Odhlásit se od události
        if (bossEnemy != null)
        {
            bossEnemy.OnDeath -= OnBossDeath;
        }
    }
    
    /// <summary>
    /// Nastavit referenci na portál (volá level generator)
    /// </summary>
    public void SetPortal(GameObject portalObject)
    {
        portal = portalObject;
        Debug.Log($"BossPortalSpawner: Portál přiřazen na pozici {portal.transform.position}, aktivní: {portal.activeSelf}");
    }
    
    /// <summary>
    /// Zavolá se když boss umře
    /// </summary>
    private void OnBossDeath(EnemyBase deadBoss)
    {
        if (hasActivatedPortal) return;
        
        Debug.Log("BossPortalSpawner: Boss " + deadBoss.name + " byl poražen! Aktivuji portál...");
        
        hasActivatedPortal = true;
        
        // Aktivovat portál s delayem
        Invoke(nameof(ActivatePortal), activationDelay);
    }
    
    /// <summary>
    /// Aktivovat portál
    /// </summary>
    private void ActivatePortal()
    {
        Debug.Log("BossPortalSpawner: ActivatePortal() ZAVOLÁNA!");
        
        if (portal == null)
        {
            Debug.LogError("BossPortalSpawner: Portál není přiřazen! Zavolej SetPortal() z generátoru.");
            return;
        }
        
        Debug.Log($"BossPortalSpawner: Aktivuji portál na pozici {portal.transform.position}");
        
        // Aktivovat GameObject portálu
        portal.SetActive(true);
        Debug.Log($"BossPortalSpawner: portal.SetActive(true) provedeno, aktivní: {portal.activeSelf}");
        
        // Aktivovat EpochPortal script (spustí částice, světlo atd.)
        EpochPortal portalScript = portal.GetComponent<EpochPortal>();
        if (portalScript != null)
        {
            Debug.Log("BossPortalSpawner: Volám portalScript.ActivatePortal()");
            portalScript.ActivatePortal();
        }
        else
        {
            Debug.LogError("BossPortalSpawner: EpochPortal script nebyl nalezen na portálu!");
        }
        
        Debug.Log("BossPortalSpawner: Portál aktivován!");
    }
    
    /// <summary>
    /// Vizuální gizmo pro debug
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (portal != null)
        {
            Gizmos.color = hasActivatedPortal ? Color.green : Color.yellow;
            Gizmos.DrawLine(transform.position, portal.transform.position);
            Gizmos.DrawWireSphere(portal.transform.position, 2f);
        }
    }
}
