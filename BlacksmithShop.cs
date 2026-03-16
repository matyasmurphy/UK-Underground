using UnityEngine;

public class BlacksmithShop : MonoBehaviour
{
    private UnlockFarms unlockFarms;
    private int currentUpgradeLevel = 1;
    private int currentDroneLevel = 1;
    private void Start()
    {
        unlockFarms = FindAnyObjectByType<UnlockFarms>();
    }
    public void UpgradeFarm()
    {
        if (currentUpgradeLevel <= 2)
        {
            if (InventorySystem.Instance.HasItem(InventorySystem.Instance.rock, 5) && InventorySystem.Instance.HasItem(InventorySystem.Instance.copper, 3) && InventorySystem.Instance.HasItem(InventorySystem.Instance.iron, 2))
            {
                InventorySystem.Instance.RemoveItem(InventorySystem.Instance.rock, 5);
                InventorySystem.Instance.RemoveItem(InventorySystem.Instance.copper, 3);
                InventorySystem.Instance.RemoveItem(InventorySystem.Instance.iron, 2);
                unlockFarms.UnlockFarmUpgrade(currentUpgradeLevel);
                currentUpgradeLevel++;
            }
        }
    }

    public void BuyDrone()
    {
        if (currentDroneLevel <= 2)
        {
            if (InventorySystem.Instance.HasItem(InventorySystem.Instance.rock, 10) && InventorySystem.Instance.HasItem(InventorySystem.Instance.copper, 8) && InventorySystem.Instance.HasItem(InventorySystem.Instance.iron, 5))
            {
                InventorySystem.Instance.RemoveItem(InventorySystem.Instance.rock, 10);
                InventorySystem.Instance.RemoveItem(InventorySystem.Instance.copper, 8);
                InventorySystem.Instance.RemoveItem(InventorySystem.Instance.iron, 5);
                unlockFarms.UnlockDroneUpgrade(currentDroneLevel);
                currentDroneLevel++;
            }
        }
    }
}
