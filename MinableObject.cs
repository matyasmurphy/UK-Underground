using Unity.VisualScripting;
using UnityEngine;

public class MinableObject : MonoBehaviour
{
    private MineManager mineManager;
    private Energy energySystem;
    public enum MineralType
    {
        Rock,
        Copper,
        Iron
    }
    public enum DestroyedStage
    {
        Default,
        Stage1,
        Stage2,
        Stage3
    }

    public MineralType mineralType;
    public DestroyedStage currentDestroyedStage;

    private int currentHealth;
    private int maxHealth;

    private SpriteRenderer spriteRenderer;
    public Sprite mineralDefault;
    public Sprite destoryedStage1;
    public Sprite destoryedStage2;
    public Sprite destoryedStage3;

    public GameObject tooltip;

    private bool mouseOver;

    private void Start()
    {
        switch (mineralType)
        {
            case MineralType.Rock: maxHealth = 3; break;
            case MineralType.Copper: maxHealth = 6; break;
            case MineralType.Iron: maxHealth = 9; break;
        }

        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        energySystem = GameObject.FindAnyObjectByType<Energy>();
    }
    private void Update()
    {
        if (mouseOver)
        {
            tooltip.SetActive(true);
        }
        else
        {
            tooltip.SetActive(false);
        }
    }

    public void BreakMineral(int breakingPower)
    {
        energySystem.TryConsumeEnergy(10f);
        switch (currentDestroyedStage)
        {
            case DestroyedStage.Default:
                currentDestroyedStage = DestroyedStage.Stage1;
                spriteRenderer.sprite = destoryedStage1;
                break;
            case DestroyedStage.Stage1:
                currentDestroyedStage = DestroyedStage.Stage2;
                spriteRenderer.sprite = destoryedStage2;
                break;
            case DestroyedStage.Stage2:
                currentDestroyedStage = DestroyedStage.Stage3;
                spriteRenderer.sprite = destoryedStage3;
                break;
            case DestroyedStage.Stage3:
                if (mineralType == MineralType.Rock)
                    InventorySystem.Instance.AddItem(InventorySystem.Instance.rock, 1);
                else if (mineralType == MineralType.Copper)
                    InventorySystem.Instance.AddItem(InventorySystem.Instance.copper, 1);
                else if (mineralType == MineralType.Iron)
                    InventorySystem.Instance.AddItem(InventorySystem.Instance.iron, 1);

                GameObject.FindAnyObjectByType<MineManager>().OnMineralBroken();

                if (GameObject.FindAnyObjectByType<MineManager>().amountOfMinerals <= 0)
                    Instantiate(GameObject.FindAnyObjectByType<MineManager>().ladderPrefab, transform.position, Quaternion.identity);

                Destroy(gameObject);
                break;
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
