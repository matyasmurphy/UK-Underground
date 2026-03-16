using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the on-screen energy bar UI.
/// Attach to the UI panel that holds the energy bar.
/// Wire EnergySystem.OnEnergyChanged → EnergyUI.OnEnergyChanged in Inspector.
/// </summary>
public class EnergyUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Energy energySystem;
    [SerializeField] private Slider energySlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI energyLabel;   // Optional: "185 / 270"

    [Header("Colors")]
    [SerializeField] private Color fullColor = new Color(0.29f, 0.85f, 0.44f); // Green
    [SerializeField] private Color mediumColor = new Color(1.00f, 0.84f, 0.00f); // Yellow
    [SerializeField] private Color lowColor = new Color(1.00f, 0.27f, 0.00f); // Orange-red
    [SerializeField] private float mediumThreshold = 0.5f;
    [SerializeField] private float lowThreshold = 0.25f;

    [Header("Animation")]
    [SerializeField] private float smoothSpeed = 5f;        // 0 = instant

    private float targetValue;

    private void Start()
    {
        if (energySystem == null)
            energySystem = GameObject.FindAnyObjectByType<Energy>();

        // Subscribe to event
        energySystem.OnEnergyChanged.AddListener(OnEnergyChanged);

        // Initialize immediately
        float pct = energySystem.EnergyPercent;
        targetValue = pct;
        energySlider.value = pct;
        UpdateColor(pct);
        UpdateLabel(energySystem.CurrentEnergy, energySystem.MaxEnergy);
    }

    private void OnDestroy()
    {
        if (energySystem != null)
            energySystem.OnEnergyChanged.RemoveListener(OnEnergyChanged);
    }

    private void Update()
    {
        // Smooth bar animation
        if (smoothSpeed > 0f)
        {
            float current = energySlider.value;
            energySlider.value = Mathf.MoveTowards(current, targetValue, smoothSpeed * Time.deltaTime);
            UpdateColor(energySlider.value);
        }
    }

    // Called by EnergySystem.OnEnergyChanged event
    public void OnEnergyChanged(float current, float max)
    {
        targetValue = current / max;

        if (smoothSpeed <= 0f)
        {
            energySlider.value = targetValue;
            UpdateColor(targetValue);
        }

        UpdateLabel(current, max);
    }

    private void UpdateColor(float percent)
    {
        if (fillImage == null) return;

        if (percent > mediumThreshold)
            fillImage.color = Color.Lerp(mediumColor, fullColor,
                (percent - mediumThreshold) / (1f - mediumThreshold));
        else if (percent > lowThreshold)
            fillImage.color = Color.Lerp(lowColor, mediumColor,
                (percent - lowThreshold) / (mediumThreshold - lowThreshold));
        else
            fillImage.color = lowColor;
    }

    private void UpdateLabel(float current, float max)
    {
        if (energyLabel != null)
            energyLabel.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }
}