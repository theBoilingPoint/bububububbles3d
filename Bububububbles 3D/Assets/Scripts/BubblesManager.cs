using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubblesManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject floor;
    [SerializeField] private GameObject normalBubble;
    [SerializeField] private GameObject dangerBubble;
    [SerializeField] private GameObject addTimeBubble;
    [SerializeField] private Transform bubblesParent; // optional: where to parent spawned bubbles
    
    [Header("Params")]
    [SerializeField, Range(1, 10000)]
    private int totalNumberOfBubbles = 100;
    [SerializeField, Range(0f, 1f)]
    private float dangerBubblePercentage = 0.3f;
    [SerializeField, Range(0f, 1f)]
    private float addTimeBubblePercentage = 0.2f;
    [SerializeField] private float minDistanceFromPlayer = 3f; // r: no-spawn radius in meters
    [SerializeField] private float densityFalloff = 0.35f;     // larger = stronger clustering near r_min
    [SerializeField] private float surfaceOffsetY = 0.1f; // how high above the floor to place a bubble
    [SerializeField] private float minSeparation = 1.0f;  // avoid overlap between bubbles
    [SerializeField] private LayerMask floorMask = ~0;    // which layers count as "floor" for raycast
    [SerializeField] private LayerMask bubbleObstacles = 0; // layers to avoid when checking separation

    private int numberOfNormalBubbles = -1;
    private int numberOfDangerBubbles = -1;
    private int numberOfAddTimeBubbles = -1;
    private List<GameObject> bubbles = new List<GameObject>();
    private uint randomSeed = 12345;
    private int maxPlacementAttempts = 10;
    
    // private int currentNumberOfNormalBubbles = -1;
    // private int currentNumberOfDangerBubbles = -1;
    // private int currentNumberOfAddTimeBubbles = -1;
    
    void Awake()
    {
        numberOfDangerBubbles = Mathf.RoundToInt(totalNumberOfBubbles * dangerBubblePercentage);
        numberOfAddTimeBubbles = Mathf.Min(Mathf.RoundToInt(totalNumberOfBubbles * addTimeBubblePercentage), totalNumberOfBubbles - numberOfDangerBubbles);
        numberOfNormalBubbles = Mathf.Max(0, totalNumberOfBubbles - numberOfDangerBubbles - numberOfAddTimeBubbles);
    }

    void Start()
    {
        ClearAllBubbles();
        SpawnAllBubbles(numberOfNormalBubbles, numberOfDangerBubbles, numberOfAddTimeBubbles);
    }

    // Update is called once per frame
    void Update()
    {
        // RecalculateCounts();
        // int normalBubbles = numberOfNormalBubbles - currentNumberOfNormalBubbles;
        // int dangerBubbles = numberOfDangerBubbles - currentNumberOfDangerBubbles;
        // int addTimeBubbles = numberOfAddTimeBubbles - currentNumberOfAddTimeBubbles;
        //
        // SpawnAllBubbles(normalBubbles, dangerBubbles, addTimeBubbles);
    }

    public void RemoveBubble(GameObject bubble)
    {
        Vector2 xzSize = GetWorldXZSize();
        System.Random rng = new System.Random((int)randomSeed);
        
        if (bubble.CompareTag("NormalBubble"))
        {
            SpawnBubblesNearPlayer(normalBubble, 1, xzSize, rng, out int remainingNormalBubbles);
        }

        if (bubble.CompareTag("DangerBubble"))
        {
            SpawnBubblesNearPlayer(dangerBubble, 1, xzSize, rng, out int remainingDangerBubbles);
        }

        if (bubble.CompareTag("AddTimeBubble"))
        {
            SpawnBubblesNearPlayer(addTimeBubble, 1, xzSize, rng, out int remainingAddTimeBubbles);
        }

        bubbles.Remove(bubble);
    }
    
    // --- Recompute counts from current percentages/total ---
    // public void RecalculateCounts()
    // {
    //     currentNumberOfDangerBubbles = Mathf.RoundToInt(totalNumberOfBubbles * dangerBubblePercentage);
    //     currentNumberOfAddTimeBubbles = Mathf.Min(
    //         Mathf.RoundToInt(totalNumberOfBubbles * addTimeBubblePercentage),
    //         totalNumberOfBubbles - numberOfDangerBubbles
    //     );
    //     currentNumberOfNormalBubbles = Mathf.Max(0, totalNumberOfBubbles - numberOfDangerBubbles - numberOfAddTimeBubbles);
    // }
    
    // Try to sample a point around the player with higher density near rMin (no-spawn radius).
    private bool TrySampleSpawnPointBiased(Vector2 xzSize, System.Random rng, out Vector3 result)
    {
        result = default;
        if (player == null)
        {
            throw new System.Exception("Player prefab is not assigned!");
        }

        // floor half-extents in local space
        float halfX = xzSize.x * 0.5f;
        float halfZ = xzSize.y * 0.5f;

        // player position in floor's local space
        Vector3 pLocal = transform.InverseTransformPoint(player.transform.position);

        // compute a conservative outer radius so the circle stays inside the rectangle
        float margin = 0.95f;
        float maxRLocal = Mathf.Min(
            Mathf.Max(0f, (halfX - Mathf.Abs(pLocal.x)) * margin),
            Mathf.Max(0f, (halfZ - Mathf.Abs(pLocal.z)) * margin)
        );

        float rMin = Mathf.Max(0.0001f, minDistanceFromPlayer);
        float rMax = Mathf.Max(rMin + 0.01f, maxRLocal);
        if (rMax <= rMin + 0.001f)
            return false; // no room to place around player

        // attempt a few samples
        for (int attempt = 0; attempt < maxPlacementAttempts; ++attempt)
        {
            // angle uniform
            float theta = (float)(rng.NextDouble() * (Mathf.PI * 2f));

            // radius with exponential bias near rMin:
            // r = rMin - (1/lambda) * ln( 1 - u*(1 - e^{-lambda (rMax-rMin)}) )
            float u = (float)rng.NextDouble();
            float lambda = Mathf.Max(0.0001f, densityFalloff);
            float span = rMax - rMin;
            float expSpan = Mathf.Exp(-lambda * span);
            float r = rMin - Mathf.Log(1f - u * (1f - expSpan)) / lambda;

            // local point relative to player
            Vector3 local = pLocal + new Vector3(r * Mathf.Cos(theta), 0f, r * Mathf.Sin(theta));
            Vector3 worldPoint = transform.TransformPoint(local);

            // raycast to floor and apply your existing checks
            Vector3 rayOrigin = worldPoint + Vector3.up * 5f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f, floorMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 candidate = hit.point + Vector3.up * surfaceOffsetY;

                // separation from other bubbles (optional)
                if (minSeparation > 0f && bubbleObstacles.value != 0)
                {
                    var overlaps = Physics.OverlapSphere(candidate, minSeparation, bubbleObstacles, QueryTriggerInteraction.Ignore);
                    if (overlaps != null && overlaps.Length > 0) continue;
                }

                result = candidate;
                return true;
            }
        }
        return false;
    }
    
    private void SpawnBubblesGrid(GameObject prefab, int count, Vector2 xzSize, System.Random rng)
    {
        if (prefab == null || count <= 0) return;

        // choose a cell size so any two points from adjacent cells are >= minSeparation
        float cell = Mathf.Max(minSeparation / 1.41421356f, 0.01f); // r / sqrt(2)

        int cols = Mathf.Max(1, Mathf.FloorToInt(xzSize.x / cell));
        int rows = Mathf.Max(1, Mathf.FloorToInt(xzSize.y / cell));

        // iterate grid cells in random order
        List<(int c,int r)> cells = new List<(int,int)>(cols*rows);
        for (int rIdx = 0; rIdx < rows; ++rIdx)
            for (int cIdx = 0; cIdx < cols; ++cIdx)
                cells.Add((cIdx, rIdx));

        // shuffle
        for (int i = cells.Count - 1; i > 0; --i)
        {
            int j = rng.Next(i + 1);
            (cells[i], cells[j]) = (cells[j], cells[i]);
        }

        float halfX = xzSize.x * 0.5f;
        float halfZ = xzSize.y * 0.5f;

        int placed = 0;
        foreach (var (cIdx, rIdx) in cells)
        {
            if (placed >= count) break;

            // cell bounds in local floor space
            float x0 = -halfX + cIdx * cell;
            float z0 = -halfZ + rIdx * cell;

            // jitter inside the cell
            float rx = x0 + (float)rng.NextDouble() * cell;
            float rz = z0 + (float)rng.NextDouble() * cell;

            Vector3 worldPoint = transform.TransformPoint(new Vector3(rx, 0f, rz));
            Vector3 rayOrigin  = worldPoint + Vector3.up * 5f;

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f, floorMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 candidate = hit.point + Vector3.up * surfaceOffsetY;

                // separation check vs already spawned bubbles (optional if cell sizing is trusted)
                if (minSeparation > 0f && bubbleObstacles.value != 0)
                {
                    var overlaps = Physics.OverlapSphere(candidate, minSeparation, bubbleObstacles, QueryTriggerInteraction.Ignore);
                    if (overlaps != null && overlaps.Length > 0)
                        continue;
                }

                var go = Instantiate(prefab, candidate, Quaternion.identity, bubblesParent);
                bubbles.Add(go);
                placed++;
            }
        }

        if (placed < count)
            Debug.LogWarning($"[BubblesManager] Grid placed {placed}/{count}. Reduce minSeparation or count, or enlarge floor.");
    }

    public void SpawnAllBubbles(int normalBubbles, int dangerBubbles, int addTimeBubbles)
    {
        Vector2 xzSize = GetWorldXZSize();
        System.Random rng = new System.Random((int)randomSeed);

        SpawnBubblesNearPlayer(normalBubble, normalBubbles, xzSize, rng, out int remainingNormalBubbles);
        SpawnBubblesNearPlayer(dangerBubble, dangerBubbles, xzSize, rng, out int  remainingDangerBubbles);
        SpawnBubblesNearPlayer(addTimeBubble, addTimeBubbles, xzSize, rng, out int remainingAddTimeBubbles);
        SpawnBubblesGrid(normalBubble, remainingNormalBubbles, xzSize, rng);
        SpawnBubblesGrid(dangerBubble, remainingDangerBubbles, xzSize, rng);
        SpawnBubblesGrid(addTimeBubble, remainingAddTimeBubbles, xzSize, rng);
    }

    // --- Remove any previously spawned bubble instances tracked in 'bubbles' ---
    public void ClearAllBubbles()
    {
        for (int i = bubbles.Count - 1; i >= 0; --i)
        {
            if (bubbles[i] != null)
            {
                Destroy(bubbles[i]);
            }
        }
        bubbles.Clear();
    }

    private void SpawnBubblesNearPlayer(GameObject prefab, int count, Vector2 xzSize, System.Random rng, out int remainingBubblesCount)
    {
        if (prefab == null || count <= 0)
        {
            remainingBubblesCount = 0;
            return;
        }

        int spawnedBubbles = 0;
        for (int i = 0; i < count; ++i)
        {
            if (TrySampleSpawnPointBiased(xzSize, rng, out Vector3 pos))
            {
                var go = Instantiate(prefab, pos, Quaternion.identity, bubblesParent);
                bubbles.Add(go);
            }
            else
            {
                spawnedBubbles++;
                Debug.LogWarning($"[{nameof(BubblesManager)}] Failed to place '{prefab.name}' after {maxPlacementAttempts} attempts.");
            }
        }
        
        remainingBubblesCount = count - spawnedBubbles;
    }

    private Vector2 GetWorldXZSize()
    {
        var mf = floor.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
            return new Vector2(10f * floor.transform.lossyScale.x, 10f * floor.transform.lossyScale.z);

        Vector3 localSize = mf.sharedMesh.bounds.size;
        Vector3 worldSize = Vector3.Scale(localSize, floor.transform.lossyScale);
        return new Vector2(Mathf.Abs(worldSize.x), Mathf.Abs(worldSize.z));
    }
}
