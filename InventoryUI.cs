using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public Transform slotParent;

    private SlotUI[] slotUIs;

    void Awake() => Instance = this;

    void Start()
    {
        // Build UI slots to match InventorySystem slot count
        int count = InventorySystem.Instance.slotCount;
        slotUIs = new SlotUI[count];

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotParent);
            slotUIs[i] = go.GetComponent<SlotUI>();
        }

        Refresh();
    }

    void Update()
    {
        // Toggle inventory with Tab or I
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.I))
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void Refresh()
    {
        var slots = InventorySystem.Instance.slots;
        for (int i = 0; i < slotUIs.Length; i++)
            slotUIs[i].UpdateSlot(slots[i]);

        HotbarUI.Instance?.Refresh();
    }
}