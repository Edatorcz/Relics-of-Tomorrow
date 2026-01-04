using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

/// <summary>
/// Generátor místností - postupná progrese obtížnosti
/// </summary>
public abstract class RoomBasedGenerator : MonoBehaviour
{
    [Header("Room Layout")]
    [SerializeField] protected int minRooms = 5;
    [SerializeField] protected int maxRooms = 10;
    [SerializeField] protected float baseRoomSize = 25f; // Základní velikost místnosti
    [SerializeField] protected float maxRoomSize = 50f; // Maximální velikost místnosti
    [SerializeField] protected float roomSizeIncreasePerEnemy = 3f; // O kolik se zvětší místnost za každého enemáka
    [SerializeField] protected float corridorWidth = 3f;
    [SerializeField] protected Vector3 startPosition = Vector3.zero;
    [SerializeField] [Range(0f, 1f)] protected float branchChance = 0.3f; // Šance na odbočku
    [SerializeField] [Range(0f, 1f)] protected float emptyRoomChance = 0.2f; // Šance na prázdnou místnost s itemy
    
    protected int numberOfRooms; // Aktuální počet místností (randomizovaný)
    
    [Header("Room Prefabs")]
    [SerializeField] protected GameObject floorPrefab;
    [SerializeField] protected GameObject wallPrefab;
    [SerializeField] protected GameObject doorPrefab;
    
    [Header("Decoration Prefabs")]
    [SerializeField] protected GameObject[] treePrefabs;
    [SerializeField] protected GameObject[] rockPrefabs;
    [SerializeField] protected GameObject[] bonePrefabs;
    [SerializeField] protected GameObject[] plantPrefabs;
    [SerializeField] protected GameObject[] miscDecorationPrefabs;
    [SerializeField] protected int decorationsPerRoom = 5;
    
    [Header("Enemy Progression")]
    [SerializeField] protected GameObject[] enemyPrefabs;
    [SerializeField] protected int startingEnemyCount = 2;
    [SerializeField] protected float enemyIncreasePerRoom = 1.5f; // Každá místnost +50% enemáků
    
    [Header("Player Settings")]
    [SerializeField] protected GameObject playerPrefab;
    [SerializeField] protected float playerSpawnHeight = 1f;
    
    [Header("Item Settings")]
    [SerializeField] protected GameObject[] itemPrefabs; // Itemy pro prázdné místnosti
    [SerializeField] protected int itemsPerTreasureRoom = 3;
    
    [Header("Boss Settings")]
    [SerializeField] protected GameObject bossPrefab; // Boss pro finální místnost
    [SerializeField] protected float bossRoomSizeMultiplier = 2.0f; // Násobič velikosti boss místnosti
    
    protected List<Room> rooms = new List<Room>();
    protected GameObject levelParent;
    
    [System.Serializable]
    public class Room
    {
        public Vector3 center;
        public int roomNumber;
        public List<GameObject> enemies;
        public List<GameObject> decorations;
        public GameObject floor;
        public float calculatedSize; // Vypočtená velikost místnosti
        public RoomType roomType; // Typ místnosti
        [System.NonSerialized] public List<Room> connectedRooms; // Připojené místnosti
        
        public Room(Vector3 center, int number)
        {
            this.center = center;
            this.roomNumber = number;
            this.enemies = new List<GameObject>();
            this.decorations = new List<GameObject>();
            this.floor = null;
            this.calculatedSize = 0f;
            this.roomType = RoomType.Normal;
            this.connectedRooms = new List<Room>();
        }
    }
    
    public enum RoomType
    {
        Start,      // Startovací místnost
        Normal,     // Normální místnost s enemáky
        Treasure,   // Prázdná místnost s itemy
        Branch,     // Odbočka
        Boss        // Finální místnost s bossem
    }
    
    void Start()
    {
        GenerateLevel();
    }
    
    public virtual void GenerateLevel()
    {
        // Randomizovat počet místností
        numberOfRooms = Random.Range(minRooms, maxRooms + 1);
        
        ClearLevel();
        CreateLevelParent();
        GenerateRooms(); // Vytvoří místnosti a propojení
        CreateBossRoom(); // Přidá boss místnost na konec
        BuildRoomStructures(); // Postaví stěny, podlahy atd.
        BakeNavMesh(); // Vybakovat NavMesh pro pohyb nepřátel
        PopulateRooms();
        SpawnPlayer();
        
        Debug.Log($"Vygenerováno {numberOfRooms} místností (+ případné odbočky + boss room)");
    }
    
    protected virtual void ClearLevel()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        rooms.Clear();
    }
    
    protected virtual void CreateLevelParent()
    {
        levelParent = new GameObject("Level");
        levelParent.transform.parent = transform;
    }
    
    protected virtual void GenerateRooms()
    {
        Vector3 currentPosition = startPosition;
        Vector3 currentDirection = Vector3.right; // Začínáme směrem doprava
        
        for (int i = 0; i < numberOfRooms; i++)
        {
            // Určit typ místnosti
            RoomType roomType = DetermineRoomType(i);
            
            // Random počet enemáků, který postupně roste
            int enemyCount = 0;
            if (roomType != RoomType.Start && roomType != RoomType.Treasure)
            {
                // Minimální a maximální počet podle progression
                int minEnemies = Mathf.Max(1, i); // Minimálně 1, roste s indexem
                int maxEnemies = Mathf.RoundToInt(startingEnemyCount * Mathf.Pow(enemyIncreasePerRoom, i));
                enemyCount = Random.Range(minEnemies, maxEnemies + 1);
            }
            
            // Velikost místnosti se zvětšuje podle počtu enemáků, ale s limitem
            float roomSize = Mathf.Min(baseRoomSize + (enemyCount * roomSizeIncreasePerEnemy), maxRoomSize);
            
            // Zkontrolovat kolize s existujícími místnostmi
            Vector3 testPosition = currentPosition;
            int maxAttempts = 10;
            int attempts = 0;
            bool validPosition = false;
            
            while (!validPosition && attempts < maxAttempts)
            {
                validPosition = true;
                foreach (Room existingRoom in rooms)
                {
                    float minDistance = (roomSize + (existingRoom.calculatedSize > 0 ? existingRoom.calculatedSize : baseRoomSize)) / 2f + corridorWidth;
                    if (Vector3.Distance(existingRoom.center, testPosition) < minDistance)
                    {
                        validPosition = false;
                        // Posunout pozici trochu dál
                        testPosition += currentDirection * (corridorWidth + 5f);
                        break;
                    }
                }
                attempts++;
            }
            
            currentPosition = testPosition;
            
            Room room = new Room(currentPosition, i);
            room.calculatedSize = roomSize;
            room.roomType = roomType;
            rooms.Add(room);
            
            // Připojit k předchozí místnosti
            if (i > 0)
            {
                rooms[i - 1].connectedRooms.Add(room);
                room.connectedRooms.Add(rooms[i - 1]);
            }
            
            // Rozhodnout o směru další místnosti
            if (i < numberOfRooms - 1)
            {
                // Šance na zatočení (ne v prvních 2 místnostech)
                if (i >= 2 && Random.Range(0f, 1f) < 0.4f)
                {
                    // Zatočit o 90 stupňů (náhodně vlevo nebo vpravo)
                    float angle = Random.Range(0, 2) == 0 ? 90f : -90f;
                    currentDirection = Quaternion.Euler(0, angle, 0) * currentDirection;
                }
                
                // Posunout pozici v aktuálním směru
                currentPosition += currentDirection * (roomSize + corridorWidth);
                
                // Šance na vedlejší místnost (odbočku)
                if (i >= 1 && i < numberOfRooms - 2 && Random.Range(0f, 1f) < branchChance)
                {
                    CreateBranchRoom(room, currentDirection);
                }
            }
        }
    }
    
    protected virtual RoomType DetermineRoomType(int roomIndex)
    {
        if (roomIndex == 0) return RoomType.Start;
        
        // Šance na treasure room (ne hned v druhé místnosti)
        if (roomIndex >= 2 && Random.Range(0f, 1f) < emptyRoomChance)
        {
            return RoomType.Treasure;
        }
        
        return RoomType.Normal;
    }
    
    protected virtual void CreateBossRoom()
    {
        if (rooms.Count == 0)
        {
            Debug.LogWarning("Nelze vytvořit boss room - žádné místnosti neexistují!");
            return;
        }
        
        // Najít poslední místnost
        Room lastRoom = rooms[rooms.Count - 1];
        
        // Určit směr od poslední místnosti
        Vector3 direction = Vector3.right;
        if (rooms.Count >= 2)
        {
            direction = (lastRoom.center - rooms[rooms.Count - 2].center).normalized;
        }
        
        // Velikost boss místnosti
        float bossRoomSize = maxRoomSize * bossRoomSizeMultiplier;
        float distance = lastRoom.calculatedSize / 2f + corridorWidth + bossRoomSize / 2f;
        Vector3 bossRoomPosition = lastRoom.center + direction * distance;
        
        // Vytvořit boss místnost
        Room bossRoom = new Room(bossRoomPosition, rooms.Count);
        bossRoom.calculatedSize = bossRoomSize;
        bossRoom.roomType = RoomType.Boss;
        rooms.Add(bossRoom);
        
        // Propojit s poslední místností
        lastRoom.connectedRooms.Add(bossRoom);
        bossRoom.connectedRooms.Add(lastRoom);
        
        Debug.Log($"Boss room vytvořena na pozici {bossRoomPosition} s velikostí {bossRoomSize}");
    }
    
    protected virtual void CreateBranchRoom(Room parentRoom, Vector3 mainDirection)
    {
        // Vytvoř odbočku kolmo na hlavní směr
        Vector3 branchDirection = Quaternion.Euler(0, 90, 0) * mainDirection;
        if (Random.Range(0, 2) == 0) branchDirection = -branchDirection;
        
        float branchDistance = parentRoom.calculatedSize / 2f + corridorWidth + baseRoomSize / 2f;
        Vector3 branchPosition = parentRoom.center + branchDirection * branchDistance;
        
        // Kontrola, jestli už tam není jiná místnost
        bool tooClose = false;
        foreach (Room existingRoom in rooms)
        {
            float minDistance = (baseRoomSize + (existingRoom.calculatedSize > 0 ? existingRoom.calculatedSize : baseRoomSize)) / 2f + corridorWidth;
            if (Vector3.Distance(existingRoom.center, branchPosition) < minDistance)
            {
                tooClose = true;
                break;
            }
        }
        
        if (tooClose) return;
        
        Room branchRoom = new Room(branchPosition, rooms.Count);
        branchRoom.calculatedSize = baseRoomSize * 0.8f; // Menší odbočka
        branchRoom.roomType = Random.Range(0f, 1f) < 0.6f ? RoomType.Treasure : RoomType.Branch;
        rooms.Add(branchRoom);
        
        // Propojit s rodičovskou místností
        parentRoom.connectedRooms.Add(branchRoom);
        branchRoom.connectedRooms.Add(parentRoom);
    }
    
    protected virtual void BuildRoomStructures()
    {
        // Nejdřív vytvoř chodby mezi všemi propojenými místnostmi
        HashSet<string> createdCorridors = new HashSet<string>();
        
        foreach (Room room in rooms)
        {
            foreach (Room connected in room.connectedRooms)
            {
                // Vytvoř unikátní klíč pro chodbu (aby se nevytvořila dvakrát)
                string corridorKey = room.roomNumber < connected.roomNumber 
                    ? $"{room.roomNumber}_{connected.roomNumber}" 
                    : $"{connected.roomNumber}_{room.roomNumber}";
                
                if (!createdCorridors.Contains(corridorKey))
                {
                    CreateCorridor(room, connected);
                    createdCorridors.Add(corridorKey);
                }
            }
        }
        
        // Pak vytvoř struktury všech místností (teď už vědí o všech propojeních)
        foreach (Room room in rooms)
        {
            CreateRoomStructure(room);
        }
    }
    
    protected virtual void CreateRoomStructure(Room room)
    {
        GameObject roomParent = new GameObject($"Room_{room.roomNumber}");
        roomParent.transform.parent = levelParent.transform;
        roomParent.transform.position = room.center;
        
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        
        // Podlaha (buď z prefabu nebo procedurálně)
        if (floorPrefab != null)
        {
            GameObject floor = Instantiate(floorPrefab, room.center, Quaternion.identity, roomParent.transform);
            floor.name = "Floor";
            floor.transform.localScale = new Vector3(roomSize / 10f, 1f, roomSize / 10f);
            floor.layer = LayerMask.NameToLayer("Ignore Raycast");
            room.floor = floor;
        }
        else
        {
            // Procedurální podlaha
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = room.center + new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(roomSize, 1f, roomSize);
            floor.GetComponent<Renderer>().material.color = GetFloorColor();
            floor.layer = LayerMask.NameToLayer("Ignore Raycast");
            floor.transform.SetParent(roomParent.transform);
            room.floor = floor;
        }
        
        // Stěny (4 strany)
        if (wallPrefab != null)
        {
            CreateWalls(room, roomParent.transform);
        }
        else
        {
            // Procedurální stěny
            CreateWalls(room, roomParent.transform);
        }
    }
    
    protected virtual void CreateWalls(Room room, Transform parent)
    {
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        float halfSize = roomSize / 2f;
        float wallHeight = 3f;
        
        // Zjistit, ve kterých směrech jsou připojené místnosti
        bool hasNorth = false, hasSouth = false, hasEast = false, hasWest = false;
        
        Debug.Log($"Room {room.roomNumber} má {room.connectedRooms.Count} připojení:");
        
        foreach (Room connected in room.connectedRooms)
        {
            Vector3 direction = (connected.center - room.center);
            float distance = direction.magnitude;
            direction = direction.normalized;
            
            Debug.Log($"  -> Room {connected.roomNumber}: dir=({direction.x:F2}, {direction.z:F2}), distance={distance:F1}");
            
            // Použít větší toleranci pro detekci směru
            if (Mathf.Abs(direction.x) > 0.3f || Mathf.Abs(direction.z) > 0.3f)
            {
                // Pokud je X výraznější
                if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.z))
                {
                    if (direction.x > 0) { hasEast = true; Debug.Log($"     -> Detekováno EAST"); }
                    else { hasWest = true; Debug.Log($"     -> Detekováno WEST"); }
                }
                // Pokud je Z výraznější
                if (Mathf.Abs(direction.z) >= Mathf.Abs(direction.x))
                {
                    if (direction.z > 0) { hasNorth = true; Debug.Log($"     -> Detekováno NORTH"); }
                    else { hasSouth = true; Debug.Log($"     -> Detekováno SOUTH"); }
                }
            }
        }
        
        Debug.Log($"Room {room.roomNumber} má dveře: N={hasNorth}, S={hasSouth}, E={hasEast}, W={hasWest}");
        
        // Severní stěna
        if (hasNorth)
        {
            CreateWallWithDoor(room.center + new Vector3(0, wallHeight/2f, halfSize), 
                             new Vector3(roomSize, wallHeight, 0.5f), parent, true);
        }
        else
        {
            CreateWall(room.center + new Vector3(0, wallHeight/2f, halfSize), 
                      new Vector3(roomSize, wallHeight, 0.5f), parent);
        }
        
        // Jižní stěna
        if (hasSouth)
        {
            CreateWallWithDoor(room.center + new Vector3(0, wallHeight/2f, -halfSize), 
                             new Vector3(roomSize, wallHeight, 0.5f), parent, false);
        }
        else
        {
            CreateWall(room.center + new Vector3(0, wallHeight/2f, -halfSize), 
                      new Vector3(roomSize, wallHeight, 0.5f), parent);
        }
        
        // Východní stěna
        if (hasEast)
        {
            CreateWallWithDoor(room.center + new Vector3(halfSize, wallHeight/2f, 0), 
                             new Vector3(0.5f, wallHeight, roomSize), parent, true);
        }
        else
        {
            CreateWall(room.center + new Vector3(halfSize, wallHeight/2f, 0), 
                      new Vector3(0.5f, wallHeight, roomSize), parent);
        }
        
        // Západní stěna
        if (hasWest)
        {
            CreateWallWithDoor(room.center + new Vector3(-halfSize, wallHeight/2f, 0), 
                             new Vector3(0.5f, wallHeight, roomSize), parent, false);
        }
        else
        {
            CreateWall(room.center + new Vector3(-halfSize, wallHeight/2f, 0), 
                      new Vector3(0.5f, wallHeight, roomSize), parent);
        }
    }
    
    protected virtual void CreateWall(Vector3 position, Vector3 scale, Transform parent)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.transform.parent = parent;
        wall.name = "Wall";
        
        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = GetWallColor();
        }
    }
    
    protected virtual void CreateWallWithDoor(Vector3 position, Vector3 scale, Transform parent, bool isExit)
    {
        float doorWidth = corridorWidth + 0.5f;
        
        // Zjistit orientaci stěny
        bool isHorizontal = scale.x > scale.z; // North/South stěna
        
        if (isHorizontal)
        {
            // Horizontální stěna (North/South) - průchod uprostřed
            float wallLength = scale.x;
            float wallThickness = scale.z;
            float sideWallWidth = (wallLength - doorWidth) / 2f;
            
            if (sideWallWidth > 0.5f)
            {
                GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leftWall.transform.position = position + new Vector3(-wallLength/2f + sideWallWidth/2f, 0, 0);
                leftWall.transform.localScale = new Vector3(sideWallWidth, scale.y, wallThickness);
                leftWall.transform.parent = parent;
                leftWall.name = "Wall_Part";
                leftWall.GetComponent<Renderer>().material.color = GetWallColor();
                
                GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rightWall.transform.position = position + new Vector3(wallLength/2f - sideWallWidth/2f, 0, 0);
                rightWall.transform.localScale = new Vector3(sideWallWidth, scale.y, wallThickness);
                rightWall.transform.parent = parent;
                rightWall.name = "Wall_Part";
                rightWall.GetComponent<Renderer>().material.color = GetWallColor();
            }
        }
        else
        {
            // Vertikální stěna (East/West) - průchod uprostřed
            float wallLength = scale.z;
            float wallThickness = scale.x;
            float sideWallWidth = (wallLength - doorWidth) / 2f;
            
            if (sideWallWidth > 0.5f)
            {
                GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leftWall.transform.position = position + new Vector3(0, 0, wallLength/2f - sideWallWidth/2f);
                leftWall.transform.localScale = new Vector3(wallThickness, scale.y, sideWallWidth);
                leftWall.transform.parent = parent;
                leftWall.name = "Wall_Part";
                leftWall.GetComponent<Renderer>().material.color = GetWallColor();
                
                GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rightWall.transform.position = position + new Vector3(0, 0, -wallLength/2f + sideWallWidth/2f);
                rightWall.transform.localScale = new Vector3(wallThickness, scale.y, sideWallWidth);
                rightWall.transform.parent = parent;
                rightWall.name = "Wall_Part";
                rightWall.GetComponent<Renderer>().material.color = GetWallColor();
            }
        }
        
        if (doorPrefab != null)
        {
            GameObject door = Instantiate(doorPrefab, position, Quaternion.Euler(0, 90, 0), parent);
            door.name = isExit ? "Exit" : "Entrance";
        }
    }
    
    protected virtual void ConnectRooms()
    {
        for (int i = 0; i < Mathf.Min(numberOfRooms - 1, rooms.Count - 1); i++)
        {
            if (!rooms[i].connectedRooms.Contains(rooms[i + 1]))
            {
                CreateCorridor(rooms[i], rooms[i + 1]);
            }
        }
    }
    
    protected virtual void CreateCorridor(Room roomA, Room roomB)
    {
        float roomASizeHalf = (roomA.calculatedSize > 0 ? roomA.calculatedSize : baseRoomSize) / 2f;
        float roomBSizeHalf = (roomB.calculatedSize > 0 ? roomB.calculatedSize : baseRoomSize) / 2f;
        
        Vector3 direction = (roomB.center - roomA.center).normalized;
        Vector3 start = roomA.center + direction * roomASizeHalf;
        Vector3 end = roomB.center - direction * roomBSizeHalf;
        
        float distance = Vector3.Distance(start, end);
        Vector3 midpoint = (start + end) / 2f;
        
        GameObject corridor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        corridor.transform.position = midpoint;
        
        // Rotace chodbičky podle směru
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            // Chodba ve směru X
            corridor.transform.localScale = new Vector3(distance, 0.1f, corridorWidth);
        }
        else
        {
            // Chodba ve směru Z
            corridor.transform.localScale = new Vector3(corridorWidth, 0.1f, distance);
        }
        
        corridor.transform.parent = levelParent.transform;
        corridor.name = $"Corridor_{roomA.roomNumber}_to_{roomB.roomNumber}";
        corridor.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        Renderer renderer = corridor.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = GetFloorColor();
        }
    }
    
    protected virtual void PopulateRooms()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            Room room = rooms[i];
            
            // Podle typu místnosti
            switch (room.roomType)
            {
                case RoomType.Start:
                    // Startovací místnost - bez enemáků
                    break;
                    
                case RoomType.Treasure:
                    // Treasure room - itemy místo enemáků
                    SpawnItemsInRoom(room);
                    break;
                    
                case RoomType.Normal:
                case RoomType.Branch:
                    // Random počet enemáků s postupnou progression
                    int minEnemies = Mathf.Max(1, room.roomNumber);
                    int maxEnemies = Mathf.RoundToInt(startingEnemyCount * Mathf.Pow(enemyIncreasePerRoom, Mathf.Max(0, room.roomNumber - 1)));
                    int enemyCount = Random.Range(minEnemies, maxEnemies + 1);
                    
                    SpawnEnemiesInRoom(room, enemyCount);
                    break;
                    
                case RoomType.Boss:
                    // Boss místnost - spawnovat bosse
                    SpawnBossInRoom(room);
                    break;
            }
            
            // Dekorace ve všech místnostech (AŽ PO enemácích aby mohly využít info o nepřátelích)
            SpawnDecorationsInRoom(room);
        }
    }
    
    protected virtual void SpawnItemsInRoom(Room room)
    {
        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogWarning($"Room {room.roomNumber}: Žádné item prefaby nejsou přiřazené");
            return;
        }
        
        GameObject itemParent = new GameObject("Items");
        itemParent.transform.parent = room.floor.transform.parent;
        
        for (int i = 0; i < itemsPerTreasureRoom; i++)
        {
            Vector3 position = GetRandomPositionInRoom(room);
            GameObject itemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
            
            if (itemPrefab != null)
            {
                GameObject item = Instantiate(itemPrefab, position, Quaternion.identity, itemParent.transform);
                item.name = $"{itemPrefab.name}_{i}";
            }
        }
        
        Debug.Log($"Treasure room {room.roomNumber} - spawned {itemsPerTreasureRoom} items");
    }
    
    protected virtual void SpawnEnemiesInRoom(Room room, int count)
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"Room {room.roomNumber}: Žádné enemy prefaby nejsou přiřazené");
            return;
        }
        
        GameObject enemyParent = new GameObject("Enemies");
        enemyParent.transform.parent = room.floor.transform.parent;
        
        for (int i = 0; i < count; i++)
        {
            Vector3 position = GetRandomPositionInRoom(room);
            
            // Výběr typu enemáka podle místnosti
            GameObject prefab = SelectEnemyType(room.roomNumber);
            
            if (prefab != null)
            {
                GameObject enemy = Instantiate(prefab, position, Quaternion.identity, enemyParent.transform);
                enemy.name = $"{prefab.name}_{i}";
                room.enemies.Add(enemy);
            }
        }
    }
    
    /// <summary>
    /// Vybere typ enemáka podle čísla místnosti s progresivní obtížností
    /// </summary>
    protected virtual GameObject SelectEnemyType(int roomNumber)
    {
        if (enemyPrefabs.Length == 0) return null;
        if (enemyPrefabs.Length == 1) return enemyPrefabs[0];
        
        // Room 1: Jen první typ (warrior apes)
        if (roomNumber == 1)
        {
            return enemyPrefabs[0];
        }
        
        // Room 2-3: 70% šance na základní, 30% na pokročilé
        if (roomNumber <= 3)
        {
            float chance = Random.Range(0f, 1f);
            if (chance < 0.7f || enemyPrefabs.Length < 2)
            {
                return enemyPrefabs[0];
            }
            else
            {
                return enemyPrefabs[1];
            }
        }
        
        // Room 4+: Postupně přidávat těžší typy
        float roll = Random.Range(0f, 1f);
        
        if (roll < 0.4f)
        {
            return enemyPrefabs[0];
        }
        else if (roll < 0.7f && enemyPrefabs.Length >= 2)
        {
            return enemyPrefabs[1];
        }
        else if (roll < 0.9f && enemyPrefabs.Length >= 3)
        {
            return enemyPrefabs[2];
        }
        else if (enemyPrefabs.Length >= 4)
        {
            return enemyPrefabs[Mathf.Min(3, enemyPrefabs.Length - 1)];
        }
        
        return enemyPrefabs[Random.Range(0, Mathf.Min(enemyPrefabs.Length, roomNumber))];
    }
    
    protected virtual void SpawnDecorationsInRoom(Room room)
    {
        // Sloučit všechny typy dekorací
        List<GameObject> allDecorations = new List<GameObject>();
        if (treePrefabs != null && treePrefabs.Length > 0) allDecorations.AddRange(treePrefabs);
        if (rockPrefabs != null && rockPrefabs.Length > 0) allDecorations.AddRange(rockPrefabs);
        if (bonePrefabs != null && bonePrefabs.Length > 0) allDecorations.AddRange(bonePrefabs);
        if (plantPrefabs != null && plantPrefabs.Length > 0) allDecorations.AddRange(plantPrefabs);
        if (miscDecorationPrefabs != null && miscDecorationPrefabs.Length > 0) allDecorations.AddRange(miscDecorationPrefabs);
        
        if (allDecorations.Count == 0)
        {
            // Tiše přeskočit - dekorace jsou volitelné
            return;
        }
        
        GameObject decorationParent = new GameObject("Decorations");
        decorationParent.transform.parent = room.floor.transform.parent;
        
        // Zvýšený počet dekorací pro větší hustotu (3-5x více)
        int totalDecorations = decorationsPerRoom * Random.Range(3, 6);
        
        for (int i = 0; i < totalDecorations; i++)
        {
            Vector3 position = GetRandomPositionInRoom(room);
            GameObject prefab = allDecorations[Random.Range(0, allDecorations.Count)];
            
            if (prefab != null)
            {
                GameObject decoration = Instantiate(prefab, position, Quaternion.Euler(0, Random.Range(0f, 360f), 0), decorationParent.transform);
                
                // Nastavit velikost podle typu dekorace
                float scale = GetDecorationScale(prefab);
                decoration.transform.localScale = Vector3.one * scale;
                
                room.decorations.Add(decoration);
            }
        }
        
        // Přidat taktické platformy/vývytáky pro ranged enemáky (NA ZEMI, ne ve vzduchu)
        bool hasRangedEnemies = room.enemies.Exists(e => e != null && e.name.Contains("Ranged"));
        if (hasRangedEnemies && rockPrefabs != null && rockPrefabs.Length > 0)
        {
            // Přidej 2-4 velké kameny/platformy pro ranged pozice (NA ZEMI)
            int platformCount = Random.Range(2, 5);
            for (int i = 0; i < platformCount; i++)
            {
                Vector3 position = GetRandomPositionInRoom(room);
                position.y = 0f; // NA ZEMI, ne ve vzduchu!
                GameObject platform = Instantiate(rockPrefabs[Random.Range(0, rockPrefabs.Length)], position, Quaternion.identity, decorationParent.transform);
                platform.name = "TacticalPlatform";
                platform.transform.localScale = Vector3.one * Random.Range(2f, 3.5f); // Větší platforma
            }
        }
    }
    
    protected virtual float GetDecorationScale(GameObject prefab)
    {
        // Určit velikost podle jména prefabu
        string name = prefab.name.ToLower();
        
        // Stromy - velké
        if (treePrefabs != null && System.Array.Exists(treePrefabs, t => t == prefab))
            return Random.Range(1.5f, 3.0f);
        
        // Kameny - střední
        if (rockPrefabs != null && System.Array.Exists(rockPrefabs, r => r == prefab))
            return Random.Range(0.8f, 1.5f);
        
        // Kosti - střední až malé
        if (bonePrefabs != null && System.Array.Exists(bonePrefabs, b => b == prefab))
            return Random.Range(0.5f, 1.2f);
        
        // Kytky/rostliny - malé
        if (plantPrefabs != null && System.Array.Exists(plantPrefabs, p => p == prefab))
            return Random.Range(0.3f, 0.8f);
        
        // Ostatní - střední
        return Random.Range(0.8f, 1.5f);
    }
    
    protected virtual Vector3 GetRandomPositionInRoom(Room room)
    {
        float roomSize = room.calculatedSize > 0 ? room.calculatedSize : baseRoomSize;
        float halfSize = roomSize / 2f - 2f;
        float x = room.center.x + Random.Range(-halfSize, halfSize);
        float z = room.center.z + Random.Range(-halfSize, halfSize);
        return new Vector3(x, 0f, z); // 0f místo 1f aby nestáli ve vzduchu
    }
    
    protected virtual void SpawnBossInRoom(Room room)
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("Boss prefab není nastaven v inspectoru! Vytvořen spawn point marker.");
            // Vytvoř marker pro boss spawn point
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "BossSpawnPoint";
            marker.transform.position = room.center + new Vector3(0, 1f, 0);
            marker.transform.localScale = Vector3.one * 2f;
            marker.GetComponent<Renderer>().material.color = Color.red;
            marker.transform.parent = room.floor.transform.parent;
            return;
        }
        
        GameObject bossParent = new GameObject("Boss");
        bossParent.transform.parent = room.floor.transform.parent;
        
        // Spawn bosse uprostřed místnosti (lehce nad zemí)
        Vector3 bossPosition = room.center + new Vector3(0, 1.5f, 0);
        GameObject boss = Instantiate(bossPrefab, bossPosition, Quaternion.identity, bossParent.transform);
        boss.name = "Boss";
        room.enemies.Add(boss);
        
        Debug.Log($"Boss spawnován v Boss Room na pozici {bossPosition}");
    }
    
    protected virtual void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("Player prefab není nastaven!");
            return;
        }
        
        if (rooms.Count == 0)
        {
            Debug.LogError("Žádné místnosti nebyly vygenerovány!");
            return;
        }
        
        Room startRoom = rooms[0];
        Vector3 spawnPosition = startRoom.center + new Vector3(0, playerSpawnHeight, 0);
        
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        player.name = "Player";
        
        Debug.Log($"Hráč spawnován v Room 0 na pozici {spawnPosition}");
    }
    
    protected abstract Color GetWallColor();
    protected abstract Color GetFloorColor();
    
    protected virtual void BakeNavMesh()
    {
        // Přidat NavMeshSurface komponentu pokud neexistuje
        NavMeshSurface surface = levelParent.GetComponent<NavMeshSurface>();
        if (surface == null)
        {
            surface = levelParent.AddComponent<NavMeshSurface>();
        }
        
        // Nastavení NavMeshSurface
        surface.collectObjects = CollectObjects.Children; // Scanovat jen children
        
        // Vybakovat NavMesh
        surface.BuildNavMesh();
        
        Debug.Log("NavMesh vybakován pomocí NavMeshSurface!");
    }
    
    [ContextMenu("Regenerate Level")]
    public void RegenerateLevel()
    {
        GenerateLevel();
    }
}
