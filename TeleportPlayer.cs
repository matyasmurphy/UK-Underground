using Unity.Cinemachine;
using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    private PlayerMovement playerMovement;

    public GameObject player;
    public GameObject teleportTo;

    private bool mouseOver;
    public GameObject tooltip;

    [Header("Camera")]
    public Collider2D switchtoCameraConfiner;

    private void Start()
    {
        playerMovement = GameObject.FindAnyObjectByType<PlayerMovement>();
    }
    void Update()
    {
        if (mouseOver)
        {
            if (tooltip != null)
                tooltip.SetActive(true);

            if (Input.GetKeyDown(KeyCode.E))
            {
                FadePanel fadePanel = FindAnyObjectByType<FadePanel>();

                if (fadePanel.IsFading) return; // block if already fading

                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;

                playerMovement.enabled = false;
                fadePanel.Fade(player.transform, teleportTo.transform, null, false, switchtoCameraConfiner);
            }
        }
        else
        {
            if (tooltip != null)
                tooltip.SetActive(false);
        }
    }

    private void OnMouseEnter()
    {
        mouseOver = true;
    }

    private void OnMouseExit()
    {
        mouseOver = false;
    }
}
