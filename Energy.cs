using UnityEngine;
using UnityEngine.Events;

public class Energy : MonoBehaviour
{
    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 270f;
    [SerializeField] private float currentEnergy;

    [Header("Passive Drain")]
    [SerializeField] private bool drainOverTime = true;
    [SerializeField] private float energyDrainPerSecond = 1f;

    [Header("Exhaustion")]
    [SerializeField] private float exhaustionThreshold = 0f;
    [SerializeField] private float exhaustionPenaltyEnergy = 50f;

    [Header("Events")]
    public UnityEvent<float, float> OnEnergyChanged;
    public UnityEvent OnExhausted;
    public UnityEvent OnEnergyFull;

    // Public read-only access
    public float CurrentEnergy => currentEnergy;
    public float MaxEnergy => maxEnergy;
    public float EnergyPercent => currentEnergy / maxEnergy;
    public bool IsExhausted => currentEnergy <= exhaustionThreshold;

    private bool hasPassedOut = false;

    private void Awake()
    {
        currentEnergy = maxEnergy;
    }

    private void Update()
    {
        if (drainOverTime && !hasPassedOut)
        {
            ConsumeEnergy(energyDrainPerSecond * Time.deltaTime);
        }
    }
    public bool TryConsumeEnergy(float amount)
    {
        if (IsExhausted) return false;

        ConsumeEnergy(amount);
        return true;
    }

    public void RestoreEnergy(float amount)
    {
        float previous = currentEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0f, maxEnergy);

        if (currentEnergy != previous)
        {
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        if (Mathf.Approximately(currentEnergy, maxEnergy))
        {
            OnEnergyFull?.Invoke();
        }
    }
    public void FullRestore()
    {
        currentEnergy = maxEnergy;
        hasPassedOut = false;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        OnEnergyFull?.Invoke();
    }

    public void ApplyExhaustionPenalty()
    {
        currentEnergy = Mathf.Max(maxEnergy - exhaustionPenaltyEnergy, 0f);
        hasPassedOut = false;
        OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
    }

    private void ConsumeEnergy(float amount)
    {
        if (hasPassedOut) return;

        float previous = currentEnergy;
        currentEnergy = Mathf.Clamp(currentEnergy - amount, 0f, maxEnergy);

        if (currentEnergy != previous)
        {
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        if (!hasPassedOut && IsExhausted)
        {
            hasPassedOut = true;
            OnExhausted?.Invoke();
        }
    }
}
