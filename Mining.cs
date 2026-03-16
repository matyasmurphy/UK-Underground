using Unity.VisualScripting;
using UnityEngine;

public class Mining : MonoBehaviour
{
    [Header("Radius")]
    Vector3 playerPos;
    Vector3 mineralPos;
    public float maxRadius = 2;
    public float distanceToMineralFromPlayer;
    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        int layerMask = ~LayerMask.GetMask("Player", "CameraConfiner");
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMask);
        if (hit.collider == null) return;

        if (!hit.collider.CompareTag("Mineral")) return;

        if (hit.collider == null) return;
        if (!hit.collider.CompareTag("Mineral")) return;

        playerPos = transform.position;
        mineralPos = hit.collider.bounds.center;
        Debug.Log($"Hit object: {hit.collider.gameObject.name}, transform pos: {hit.transform.position}, bounds center: {hit.collider.bounds.center}, player pos: {transform.position}");
        distanceToMineralFromPlayer = Vector3.Distance(mineralPos, playerPos);

        if (Input.GetKeyDown(KeyCode.E) && hit.collider.CompareTag("Mineral"))
        {

            if (hit.collider != null && distanceToMineralFromPlayer < maxRadius)
            {
                MinableObject minableObject = hit.collider.GetComponent<MinableObject>();
                minableObject.BreakMineral(1);
            }
        }
    }
}
