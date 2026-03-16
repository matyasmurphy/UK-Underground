using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AutoHarvester : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float hoverHeight = 1.5f;    // How high above crops it floats
    public float hoverAmplitude = 0.15f; // Bob up/down amount
    public float hoverFrequency = 2f;    // Bob speed

    [Header("Harvesting")]
    public float scanInterval = 1f;      // How often it looks for ready crops
    public float collectRange = 0.3f;    // How close it needs to be to collect

    [Header("Roaming")]
    public float roamRadius = 5f;       // Radius to roam within
    public float roamSpeed = 1.5f;      // Speed while roaming
    public float roamWaitMin = 1f;      // Min seconds before picking new roam point
    public float roamWaitMax = 3f;      // Max seconds before picking new roam point

    private Vector3 roamTarget;
    private float roamTimer = 0f;

    private Vector3 homePosition;
    private GrowCrop targetCrop;
    private List<GrowCrop> allCrops = new List<GrowCrop>();
    private float hoverBaseY;
    private bool isMoving = false;

    [Header("Zone")]
    public FarmZone assignedZone;

    void Start()
    {
        homePosition = assignedZone.transform.position;
        hoverBaseY = transform.position.y;

        // Find all farm plots in the scene
        allCrops.AddRange(FindObjectsByType<GrowCrop>(FindObjectsSortMode.None));

        StartCoroutine(ScanForCrops());
    }

    void Update()
    {
        // Hovering bob animation
        float targetY = hoverBaseY + Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;

        if (isMoving && targetCrop != null)
        {
            // Move toward the target crop (at hover height above it)
            Vector3 targetPos = targetCrop.transform.position + Vector3.up * hoverHeight;
            targetPos.y += Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            // Check if close enough to harvest
            float dist = Vector2.Distance(transform.position, targetPos);
            if (dist < collectRange)
            {
                if (targetCrop.IsReadyToHarvest())
                    targetCrop.Harvest();

                targetCrop = null;
                isMoving = false;
            }
        }
        else
        {
            // Roam randomly when idle
            roamTimer -= Time.deltaTime;

            if (roamTimer <= 0f)
            {
                // Pick a new random point within roamRadius of home
                Vector2 randomOffset = Random.insideUnitCircle * roamRadius;
                roamTarget = homePosition + new Vector3(randomOffset.x, randomOffset.y * 0.3f, 0);
                roamTarget.y = Mathf.Max(roamTarget.y, homePosition.y); // stay above ground
                roamTimer = Random.Range(roamWaitMin, roamWaitMax);
            }

            // Float toward roam target with bobbing
            roamTarget.y += Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, roamTarget, roamSpeed * Time.deltaTime);
        }
    }

    IEnumerator ScanForCrops()
    {
        while (true)
        {
            yield return new WaitForSeconds(scanInterval);

            if (!isMoving)
            {
                GrowCrop closest = FindNearestReadyCrop();
                if (closest != null)
                {
                    targetCrop = closest;
                    hoverBaseY = closest.transform.position.y + hoverHeight;
                    isMoving = true;
                }
            }
        }
    }

    GrowCrop FindNearestReadyCrop()
    {
        GrowCrop nearest = null;
        float nearestDist = float.MaxValue;

        foreach (GrowCrop crop in allCrops)
        {
            if (crop == null || !crop.IsReadyToHarvest()) continue;

            float dist = Vector2.Distance(transform.position, crop.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = crop;
            }
        }

        return nearest;
    }

    IEnumerator ReturnHome()
    {
        while (Vector2.Distance(transform.position, homePosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, homePosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }
}