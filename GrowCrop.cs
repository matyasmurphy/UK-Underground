using UnityEngine;

public class GrowCrop : MonoBehaviour
{
    private Energy energySystem;
    public enum CropType { NotPlanted, Potato }
    public enum CropStage { NotPlanted, Stage1, Stage2, Stage3 }

    public CropType cropType;   
    public CropStage cropStage;

    private bool mouseOver;

    public SpriteRenderer spriteRenderer;
    public Sprite notPlantedSprite;
    public Sprite stageSprite1;
    public Sprite stageSprite2;
    public Sprite stageSprite3;

    public GameObject tooltip;
    public GameObject outline;

    private TimeManager timeManager;
    private TimeManager.Day lastGrownDay = (TimeManager.Day)(-1);

    private void Start()
    {
        timeManager = FindAnyObjectByType<TimeManager>();
        energySystem = FindAnyObjectByType<Energy>();
    }
    private void Update()
    {
        //Tooltip
        if (mouseOver == true)
        {
            if (cropStage == CropStage.NotPlanted || cropStage == CropStage.Stage3)
            {
                tooltip.SetActive(true);
                outline.SetActive(true);
            }
        }
        else
        {
            outline.SetActive(false);
            tooltip.SetActive(false);
        }

        //Plant crop
        if (mouseOver && Input.GetKeyDown(KeyCode.E) && cropStage == CropStage.NotPlanted && HotbarUI.Instance.selectedItem.itemName == "PotatoSeed")
        {
            energySystem.TryConsumeEnergy(10f);
            if (InventorySystem.Instance.HasItem(InventorySystem.Instance.potatoSeed, 1))
            {
                InventorySystem.Instance.RemoveItem(InventorySystem.Instance.potatoSeed, 1);
                cropType = CropType.Potato;
                cropStage = CropStage.Stage1;
                spriteRenderer.sprite = stageSprite1;
                lastGrownDay = timeManager.currentDay;
            }
            else
                Debug.Log("Not enough potato seeds!");

        }

        if (cropStage != CropStage.NotPlanted && cropType != CropType.NotPlanted)
        {
            if (timeManager.currentDay != lastGrownDay)
            {
                lastGrownDay = timeManager.currentDay;

                switch (cropStage)
                {
                    case CropStage.Stage1:
                        spriteRenderer.sprite = stageSprite2;
                        cropStage = CropStage.Stage2;
                        break;
                    case CropStage.Stage2:
                        spriteRenderer.sprite = stageSprite3;
                        cropStage = CropStage.Stage3;
                        break;
                }
            }
        }

        if (mouseOver && Input.GetKeyDown(KeyCode.E) && cropStage == CropStage.Stage3)
        {
            energySystem.TryConsumeEnergy(10f);
            Harvest();
        }
    }
    public bool IsReadyToHarvest()
    {
        return cropStage == CropStage.Stage3;
    }

    public void Harvest()
    {
        InventorySystem.Instance.AddItem(InventorySystem.Instance.potato, 1);
        cropType = CropType.NotPlanted;
        cropStage = CropStage.NotPlanted;
        spriteRenderer.sprite = notPlantedSprite;
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
