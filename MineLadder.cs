using Unity.VisualScripting;
using UnityEngine;

public class MineLadder : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private MineManager mineManager;
    private bool mouseOver;

    public GameObject mineExit;
    public Collider2D mainArea;

    private void Start()
    {
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        mineManager = FindAnyObjectByType<MineManager>();
    }
    void Update()
    {
        if (mouseOver && Input.GetKeyDown(KeyCode.E))
        {
            GameObject.FindAnyObjectByType<FadePanel>().Fade(playerMovement.transform, transform, gameObject, true);
            mineManager.currentMineLevel++;
        }
        else if (mouseOver && Input.GetKeyDown(KeyCode.Q))
        {
            GameObject mineExitObj = GameObject.Find("OutsideMineTeleport");
            Collider2D mainAreaCollider = GameObject.Find("MainAreaCameraThing").GetComponent<Collider2D>();
            GameObject.FindAnyObjectByType<FadePanel>().Fade(playerMovement.transform, mineExitObj.transform, gameObject, true, mainAreaCollider);

            mineManager.currentMineLevel = 0;
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
