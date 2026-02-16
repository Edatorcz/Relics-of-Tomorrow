using UnityEngine;

/// <summary>
/// Test script pro rychlé testování artifact systému
/// Připoj na GameObject ve scéně a použij klávesy pro testování
/// </summary>
public class ArtifactTester : MonoBehaviour
{
    [Header("Test Artifacts (Items)")]
    [SerializeField] private ItemData testArtifact1;
    [SerializeField] private ItemData testArtifact2;
    [SerializeField] private ItemData testArtifact3;
    
    [Header("Test Settings")]
    [SerializeField] private KeyCode activateKey1 = KeyCode.Alpha1;
    [SerializeField] private KeyCode activateKey2 = KeyCode.Alpha2;
    [SerializeField] private KeyCode activateKey3 = KeyCode.Alpha3;
    [SerializeField] private KeyCode clearAllKey = KeyCode.C;
    [SerializeField] private KeyCode spawnPickupKey = KeyCode.P;
    
    [Header("Pickup Settings")]
    [SerializeField] private GameObject artifactPickupPrefab;
    [SerializeField] private float spawnDistance = 5f;
    
    void Update()
    {
        // Testování aktivace artefaktů
        if (Input.GetKeyDown(activateKey1) && testArtifact1 != null)
        {
            TestActivateArtifact(testArtifact1);
        }
        
        if (Input.GetKeyDown(activateKey2) && testArtifact2 != null)
        {
            TestActivateArtifact(testArtifact2);
        }
        
        if (Input.GetKeyDown(activateKey3) && testArtifact3 != null)
        {
            TestActivateArtifact(testArtifact3);
        }
        
        // Clear všechny artefakty
        if (Input.GetKeyDown(clearAllKey))
        {
            TestClearAll();
        }
        
        // Spawn pickup před hráčem
        if (Input.GetKeyDown(spawnPickupKey))
        {
            TestSpawnPickup();
        }
    }
    
    void TestActivateArtifact(ItemData artifactItem)
    {
        if (artifactItem == null || artifactItem.artifactData == null)
        {
            Debug.LogError("[TEST] Item nebo ArtifactData je null!");
            return;
        }
        
        if (ArtifactManager.Instance != null)
        {
            Debug.Log($"[TEST] Aktivuji artefakt: {artifactItem.artifactData.artifactName}");
            ArtifactManager.Instance.ActivateArtifact(artifactItem.artifactData);
        }
        else
        {
            Debug.LogError("[TEST] ArtifactManager nenalezen!");
        }
    }
    
    void TestClearAll()
    {
        if (ArtifactManager.Instance != null)
        {
            Debug.Log("[TEST] Mažu všechny artefakty");
            ArtifactManager.Instance.ClearAllArtifacts();
        }
        else
        {
            Debug.LogError("[TEST] ArtifactManager nenalezen!");
        }
    }
    
    void TestSpawnPickup()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[TEST] Hráč nenalezen!");
            return;
        }
        
        // Spawn pickup před hráčem
        Vector3 spawnPos = player.transform.position + player.transform.forward * spawnDistance;
        spawnPos.y += 1f; // Trochu výš
        
        if (artifactPickupPrefab != null)
        {
            GameObject pickup = Instantiate(artifactPickupPrefab, spawnPos, Quaternion.identity);
            Debug.Log($"[TEST] Spawnut pickup na pozici {spawnPos}");
        }
        else
        {
            // Vytvoř default pickup
            GameObject pickup = new GameObject("Test_ArtifactPickup");
            pickup.transform.position = spawnPos;
            
            ArtifactPickup pickupScript = pickup.AddComponent<ArtifactPickup>();
            
            if (testArtifact1 != null)
                pickupScript.SetItemData(testArtifact1);
            
            // Vizuální reprezentace
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.parent = pickup.transform;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * 0.5f;
            
            Renderer renderer = visual.GetComponent<Renderer>();
            if (renderer != null && testArtifact1 != null && testArtifact1.artifactData != null)
            {
                renderer.material.color = testArtifact1.artifactData.auraColor;
            }
            
            Debug.Log($"[TEST] Vytvořen default pickup na pozici {spawnPos}");
        }
    }
    
    void OnGUI()
    {
        if (!Application.isPlaying) return;
        
        // Zobrazit nápovědu
        float startY = Screen.height - 200f;
        float lineHeight = 20f;
        
        GUI.color = Color.black;
        GUI.Label(new Rect(12, startY + 2, 400, lineHeight), "=== ARTIFACT TESTER ===");
        GUI.color = Color.yellow;
        GUI.Label(new Rect(10, startY, 400, lineHeight), "=== ARTIFACT TESTER ===");
        
        startY += lineHeight;
        
        string[] instructions = new string[]
        {
            $"1 - Aktivovat {(testArtifact1 != null ? testArtifact1.itemName : "Artifact 1")}",
            $"2 - Aktivovat {(testArtifact2 != null ? testArtifact2.itemName : "Artifact 2")}",
            $"3 - Aktivovat {(testArtifact3 != null ? testArtifact3.itemName : "Artifact 3")}",
            "C - Vymazat všechny artefakty",
            "P - Spawn pickup před hráčem"
        };
        
        GUI.color = Color.white;
        foreach (string instruction in instructions)
        {
            GUI.Label(new Rect(10, startY, 400, lineHeight), instruction);
            startY += lineHeight;
        }
        
        // Zobrazit aktivní artefakty
        if (ArtifactManager.Instance != null)
        {
            startY += lineHeight;
            GUI.color = Color.green;
            int count = ArtifactManager.Instance.GetActiveArtifactCount();
            GUI.Label(new Rect(10, startY, 400, lineHeight), $"Aktivní artefakty: {count}");
            
            var artifacts = ArtifactManager.Instance.GetActiveArtifacts();
            startY += lineHeight;
            foreach (var artifact in artifacts)
            {
                GUI.Label(new Rect(10, startY, 400, lineHeight), $"  - {artifact.artifactName}");
                startY += lineHeight;
            }
        }
    }
}
