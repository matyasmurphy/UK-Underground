using TMPro;
using UnityEngine;

public class InventoryText : MonoBehaviour
{
    [Header("Money")]
    public TextMeshProUGUI moneyText;
    private void Update()
    {
        moneyText.text = $"{InventorySystem.Instance.money.ToString()}";
    }
}
