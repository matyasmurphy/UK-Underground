using UnityEngine;
using System.Collections.Generic;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance;

    [Header("Money")]
    public int money;
    [Header("Available Items")]
    public ItemData potatoSeed;
    public ItemData potato;
    public ItemData rock;
    public ItemData copper;
    public ItemData iron;
    public ItemData biscuits;
    [Header("Settings")]
    public int slotCount = 12; // Change this to however many slots you want

    public List<InventorySlot> slots = new List<InventorySlot>();

    void Awake()
    {
        Instance = this;
        // Initialize empty slots
        for (int i = 0; i < slotCount; i++)
            slots.Add(new InventorySlot());
    }

    // Returns true if item was added successfully
    public bool AddItem(ItemData item, int amount = 1)
    {
        // First try to stack into existing slots
        foreach (var slot in slots)
        {
            if (slot.item == item && slot.quantity < item.maxStack)
            {
                int canFit = item.maxStack - slot.quantity;
                int toAdd = Mathf.Min(canFit, amount);
                slot.quantity += toAdd;
                amount -= toAdd;
                InventoryUI.Instance?.Refresh();
                if (amount <= 0) return true;
            }
        }

        // Then fill empty slots
        foreach (var slot in slots)
        {
            if (slot.item == null)
            {
                slot.item = item;
                slot.quantity = Mathf.Min(amount, item.maxStack);
                amount -= slot.quantity;
                InventoryUI.Instance?.Refresh();
                if (amount <= 0) return true;
            }
        }

        Debug.Log("Inventory full!");
        return false;
    }

    public bool RemoveItem(ItemData item, int amount = 1)
    {
        // Guard: make sure player actually has enough before touching any slot
        if (!HasItem(item, amount)) return false;

        int remaining = amount;

        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i].item != item) continue;

            if (slots[i].quantity >= remaining)
            {
                slots[i].quantity -= remaining;
                remaining = 0;
            }
            else
            {
                remaining -= slots[i].quantity;
                slots[i].quantity = 0;
            }

            if (slots[i].quantity == 0)
                slots[i].item = null;

            if (remaining <= 0)
                break;
        }

        InventoryUI.Instance?.Refresh();
        return true;
    }

    public bool HasItem(ItemData item, int amount = 1)
    {
        int total = 0;
        foreach (var slot in slots)
            if (slot.item == item) total += slot.quantity;
        return total >= amount;
    }
}

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;
}