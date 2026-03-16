using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestsManager : MonoBehaviour
{
    public List<Quest> quests = new List<Quest>();
    public List<Quest> activeQuests = new List<Quest>();

    private void Awake()
    {
        // Reset ScriptableObject state that leaks between Play sessions
        foreach (Quest q in quests)
            q.isComplete = false;
    }
}