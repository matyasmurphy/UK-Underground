using System.Collections.Generic;
using UnityEngine;

public class MineManager : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private MineManager mineManager;

    public Collider2D platform;

    public GameObject rockPrefab;
    public GameObject copperPrefab;
    public GameObject ironPrefab;

    public GameObject ladderPrefab;

    public float leftEdge;
    public float rightEdge;
    public float groundY;

    public Vector2 randomPos;
    public Vector2 previousPos;

    public int amountOfRocks;
    public int amountOfCopper;
    public int amountOfIron;

    public float distaneBetweenMinerals;
    public float minDistaneBetweenMinerals;

    public int amountOfMinerals;

    private List<Vector2> spawnedPositions = new List<Vector2>();

    [Header("Collapse")]
    public int currentMineLevel;
    public int levelDangerStarts = 5;      // Level where collapses start happening
    public float baseCollapseChance = 0.05f; // 5% chance 
    public float increaseRate = 0.02f;     // Adds 2% danger per level
    public float maxCollapseChance = 0.60f;  // Caps danger at 60%

    [Header("Mineral Spawns by Level")]
    public AnimationCurve rockSpawnCurve;
    public AnimationCurve copperSpawnCurve;
    public AnimationCurve ironSpawnCurve;

    private void Start()
    {
        playerMovement = GameObject.FindAnyObjectByType<PlayerMovement>();
        mineManager = GameObject.FindAnyObjectByType<MineManager>();

        leftEdge = platform.bounds.min.x;
        rightEdge = platform.bounds.max.x;

        groundY = platform.bounds.max.y;

        SpawnMinerals();
    }

    private Vector2? GetValidPosition()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector2 pos = new Vector2(Random.Range(leftEdge + 2f, rightEdge - 2f), groundY + 0.5f);
            bool tooClose = false;

            foreach (Vector2 spawnedPos in spawnedPositions)
            {
                if (Vector2.Distance(pos, spawnedPos) < minDistaneBetweenMinerals)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                spawnedPositions.Add(pos);
                return pos;
            }
        }

        Debug.LogWarning("No valid position found, stopping spawn");

        return null;
    }

    public void SpawnMinerals()
    {
        int maxTotalMinerals = Mathf.Min(15, 5 + (currentMineLevel / 2));

        // 1. Calculate the DESIRED amounts (including your randomness)
        int desiredRocks = Mathf.Max(3, 15 - (currentMineLevel / 2)) + Random.Range(-1, 2);
        int desiredCopper = (1 + (currentMineLevel / 3)) + Random.Range(-1, 2);

        int desiredIron = 0;
        if (currentMineLevel >= 5)
        {
            desiredIron = 1 + ((currentMineLevel - 5) / 4) + Random.Range(0, 2);
        }

        // Prevent negative numbers just in case
        desiredRocks = Mathf.Max(0, desiredRocks);
        desiredCopper = Mathf.Max(0, desiredCopper);
        desiredIron = Mathf.Max(0, desiredIron);

        // 2. Allocate the actual amounts from the Pool
        int remainingSpace = maxTotalMinerals;

        // Give Iron first dibs on the space
        amountOfIron = Mathf.Min(desiredIron, remainingSpace);
        remainingSpace -= amountOfIron;

        // Give Copper second dibs on whatever is left
        amountOfCopper = Mathf.Min(desiredCopper, remainingSpace);
        remainingSpace -= amountOfCopper;

        // Rocks get the leftovers
        amountOfRocks = Mathf.Min(desiredRocks, remainingSpace);

        spawnedPositions.Clear();
        amountOfMinerals = 0;
        for (int i = 0; i < amountOfRocks; i++)
        {
            Vector2? pos = GetValidPosition();
            if (pos == null) break;
            Instantiate(rockPrefab, pos.Value, Quaternion.identity);
            amountOfMinerals++;
        }

        for (int i = 0; i < amountOfCopper; i++)
        {
            Vector2? pos = GetValidPosition();
            if (pos == null) break;
            Instantiate(copperPrefab, pos.Value, Quaternion.identity);
            amountOfMinerals++;
        }

        for (int i = 0; i < amountOfIron; i++)
        {
            Vector2? pos = GetValidPosition();
            if (pos == null) break;
            Instantiate(ironPrefab, pos.Value, Quaternion.identity);
            amountOfMinerals++;
        }
    }
    public void OnMineralBroken()
    {
        amountOfMinerals--;

        if (currentMineLevel >= levelDangerStarts)
        {
            RollForCollapse();
        }
    }

    private void RollForCollapse()
    {
        // Calculate how many levels deep into the "danger zone" we are
        int activeDangerLevels = currentMineLevel - levelDangerStarts;

        // Calculate chance: Base + (Levels * Increase)
        float collapseChance = baseCollapseChance + (activeDangerLevels * increaseRate);

        // Mathf.Min ensures the chance never goes higher than our maxCollapseChance
        collapseChance = Mathf.Min(collapseChance, maxCollapseChance);

        // Random.value generates a random float between 0.0 and 1.0 in Unity
        float diceRoll = Random.value;

        Debug.Log($"Collapse Chance: {collapseChance * 100}% | We rolled: {diceRoll}");

        // Check if the roll was lower than our chance
        if (diceRoll <= collapseChance)
        {
            TriggerMineCollapse();
        }
    }

    // ---> NEW METHOD: What happens when it collapses? <---
    private void TriggerMineCollapse()
    {
        Debug.Log("RUMBLE RUMBLE! THE MINE HAS COLLAPSED!");

        GameObject mineExitObj = GameObject.Find("OutsideMineTeleport");
        Collider2D mainAreaCollider = GameObject.Find("MainAreaCameraThing").GetComponent<Collider2D>();
        GameObject.FindAnyObjectByType<FadePanel>().Fade(playerMovement.transform, mineExitObj.transform, null, false, mainAreaCollider);
        InventorySystem.Instance.money = InventorySystem.Instance.money/2;

        mineManager.currentMineLevel = 0;
    }
}
