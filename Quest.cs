using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questName;
    public ItemData whatTheQuestTakes;
    public int requiredAmount = 1;
    public ItemData questReward;
    public int rewardAmount = 1;
    public bool isComplete;
}
