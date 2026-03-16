using UnityEngine;

public class UnlockFarms : MonoBehaviour
{
    public GameObject firstLadder;
    public GameObject farmUpgrade1;
    public GameObject farmUpgrade2;
    public GameObject droneUpgrade1;
    public GameObject droneUpgrade2;

    private bool isFarm1Unlocked;
    private bool isFarm2Unlocked;
    private bool isDrone1Unlocked;
    private bool isDrone2Unlocked;

    public void UnlockFarmUpgrade(int level)
    {
        if (level == 1 && !isFarm1Unlocked)
        {
            isFarm1Unlocked = true;
            firstLadder.SetActive(true);
            farmUpgrade1.SetActive(true);
        }
        else if (level == 2 && !isFarm2Unlocked)
        {
            isFarm2Unlocked = true;
            farmUpgrade2.SetActive(true);
        }
    }

    public void UnlockDroneUpgrade(int level)
    {
        if (level == 1 && !isDrone1Unlocked)
        {
            isDrone1Unlocked = true;
            droneUpgrade1.SetActive(true);
        }
        else if (level == 2 && !isDrone2Unlocked)
        {
            isDrone2Unlocked = true;
            droneUpgrade2.SetActive(true);
        }
    }
}