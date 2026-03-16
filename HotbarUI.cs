using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarUI : MonoBehaviour
{
    public static HotbarUI Instance;

    public SlotUI[] hotbarSlotUIs;   // Drag your 5 HotbarPanel slot UIs here
    public int hotbarSize = 5;       // Must match your Grid column count
    public int selectedIndex = 0;

    public ItemData selectedItem;

    [Header("Selection Highlight")]
    public Color selectedColor = Color.yellow;
    public Color normalColor = Color.white;

    void Awake() => Instance = this;

    void Update()
    {
        selectedItem = GetSelectedItem();

        // Select slots with keys 1–5
        for (int i = 0; i < hotbarSize; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectSlot(i);
        }

        // Scroll wheel through hotbar
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll > 0f) SelectSlot((selectedIndex - 1 + hotbarSize) % hotbarSize);
        if (scroll < 0f) SelectSlot((selectedIndex + 1) % hotbarSize);
    }

    public void SelectSlot(int index)
    {
        selectedIndex = index;
        Refresh();
    }

    public void Refresh()
    {
        // Hotbar mirrors inventory slots 0..(hotbarSize-1)
        var slots = InventorySystem.Instance.slots;
        for (int i = 0; i < hotbarSlotUIs.Length; i++)
        {
            hotbarSlotUIs[i].UpdateSlot(slots[i]);

            // Highlight selected
            var img = hotbarSlotUIs[i].GetComponent<Image>();
            if (img != null)
                img.color = (i == selectedIndex) ? selectedColor : normalColor;
        }
    }

    // Returns the currently selected item (null if empty)
    public ItemData GetSelectedItem()
    {
        var slot = InventorySystem.Instance.slots[selectedIndex];
        return slot.item;
    }
}