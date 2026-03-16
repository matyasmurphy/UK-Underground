using UnityEngine;

public class Sleep : MonoBehaviour
{
    private TimeManager timeManager;
    private TaxCollector taxCollector;
    private Energy energySystem;

    private bool mouseOver;

    public GameObject outline;
    public GameObject tooltip;

    private void Start()
    {
        timeManager = GameObject.FindAnyObjectByType<TimeManager>();
        taxCollector = GameObject.FindAnyObjectByType<TaxCollector>();
        energySystem = GameObject.FindAnyObjectByType<Energy>();
    }

    private void Update()
    {
        if (mouseOver == true && Input.GetKeyDown(KeyCode.E))
        {
            energySystem.FullRestore();
            if (timeManager.currentDay == TimeManager.Day.Sunday)
                taxCollector.OnPlayerSleptOnSunday();

            GameObject.FindAnyObjectByType<FadePanel>().SkipDayFade();
        }

        outline.SetActive(mouseOver);
        tooltip.SetActive(mouseOver);
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
