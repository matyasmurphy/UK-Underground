using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ShopItem
{
    public string itemName;
    public int price;
    public int sellPrice;
}
public class ShopScript : MonoBehaviour
{
    public List<ShopItem> items = new List<ShopItem>();

    [System.Serializable]
    public class ShopButtonUI
    {
        public Button button;
        public bool isSellButton;
    }

    public List<ShopButtonUI> shopButtons = new List<ShopButtonUI>();

    private void Start()
    {
        for (int i = 0; i < shopButtons.Count; i++)
        {
            if (i >= items.Count) break;

            ShopItem item = items[i];
            ShopButtonUI ui = shopButtons[i];

            int index = i;
            ui.button.onClick.AddListener(() => OnButtonClick(index));
        }
    }

    private void OnButtonClick(int index)
    {
        ShopButtonUI ui = shopButtons[index];

        if (ui.isSellButton)
            Sell(index);
        else
            Buy(index);
    }

    private void Buy(int index)
    {
        ShopItem item = items[index];

        if (InventorySystem.Instance.money >= item.price)
        {
            InventorySystem.Instance.money -= item.price;

            if (item.itemName == "Potato Seed") InventorySystem.Instance.AddItem(InventorySystem.Instance.potatoSeed, 1);
        }
        else
        {
            Debug.Log("Not enough money!");
        }
    }

    private void Sell(int index)
    {
        ShopItem item = items[index];

        if (item.itemName == "Potato")
        {
            if (!InventorySystem.Instance.HasItem(InventorySystem.Instance.potato)) { Debug.Log("Not enough to sell!"); return; }
            InventorySystem.Instance.RemoveItem(InventorySystem.Instance.potato, 1);
        }
        else if (item.itemName == "Rock")
        {
            if (!InventorySystem.Instance.HasItem(InventorySystem.Instance.rock)) { Debug.Log("Not enough to sell!"); return; }
            InventorySystem.Instance.RemoveItem(InventorySystem.Instance.rock, 1);
        }
        else if (item.itemName == "Copper")
        {
            if (!InventorySystem.Instance.HasItem(InventorySystem.Instance.copper)) { Debug.Log("Not enough to sell!"); return; }
            InventorySystem.Instance.RemoveItem(InventorySystem.Instance.copper, 1);
        }
        else if (item.itemName == "Iron")
        {
            if (!InventorySystem.Instance.HasItem(InventorySystem.Instance.iron)) { Debug.Log("Not enough to sell!"); return; }
            InventorySystem.Instance.RemoveItem(InventorySystem.Instance.iron, 1);
        }

        InventorySystem.Instance.money += item.sellPrice;
    }
}
