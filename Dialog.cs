using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TimeManager;

[System.Serializable]
public class IndividualDialog
{
    public string headerContent;
    public List<string> dialogueContents = new List<string>();

    [Header("Reward")]
    [Tooltip("Enable this for NPCs where you have a recieve a reward")]
    public bool givesItem;
    public ItemData recievedItem;
    public int amountofRecievedItem;

    [Header("Quest")]
    public bool doesDialogGiveQuest;
    public Quest quest;

    [Header("Choice")]
    [Tooltip("Enable this for NPCs where you have a choice")]
    public bool hasChoice;
    public string choiceQuestion;
    public int yesDialogIndex;
    public int noDialogIndex;

    [Header("Response")]
    public bool isResponseDialog;

    [Header("TeleportNPC")]
    public bool isTeleportDialog;
    public GameObject teleportTo;

    [Header("Quest TurnIn")]
    public bool isQuestTurnIn;
    public Quest questToComplete;

    [Header("Quest Required")]
    [Tooltip("This dialog only plays if this quest is currently active")]
    public bool requiresActiveQuest;
    public Quest requiredQuest;
}

public class Dialog : MonoBehaviour
{
    // Global lock — only one NPC dialog can be active at a time
    public static Dialog activeDialog = null;

    private QuestsManager questsManager;
    private TimeManager timeManager;
    public Day lastTalkedDay = (Day)(-1);
    public bool talkedToday = false;

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueHeaderText;
    public TextMeshProUGUI dialogueContentText;
    public float typingSpeed = 0.05f;

    public List<IndividualDialog> dialogs = new List<IndividualDialog>();

    // progressionIndex tracks which dialog is next in the normal daily sequence.
    // currentDialogIndex is what is actively being displayed (may temporarily
    // jump to a quest dialog without affecting progression).
    private int progressionIndex = 0;
    public int currentDialogIndex = 0;
    public int currentLineIndex = 0;

    public bool isPlayerTalkingToNPC = false;
    private bool isTyping = false;
    private bool mouseOver;
    private Coroutine typingCoroutine = null;

    private static readonly List<Dialog> allDialogs = new List<Dialog>();

    private bool allDialogsDone = false;
    private bool isQuestDialog = false;

    private GameObject pendingTeleport = null;

    private static void StopAllInstances()
    {
        foreach (Dialog d in allDialogs)
            if (!d.isCutsceneDialog)
                d.StopTyping();
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
        isTyping = false;
    }

    private void StartTyping()
    {
        StopTyping();
        typingCoroutine = StartCoroutine(TextAnimation());
    }

    [Header("Choice")]
    public GameObject choicePanel;
    public TextMeshProUGUI questionHeaderText;
    public TextMeshProUGUI questionText;
    public Button yesButton;
    public Button noButton;
    private bool awaitingChoice = false;

    [Header("Shop")]
    [Tooltip("Enable this for NPCs that should open the shop")]
    public bool opensShop;
    public GameObject shopPanel;

    [Header("Cutscene")]
    [Tooltip("Enable this for NPCs triggered automatically (e.g. TaxCollector). Skips mouse/outline/tooltip logic.")]
    public bool isCutsceneDialog = false;

    private IndividualDialog CurrentDialog => dialogs[currentDialogIndex];

    private bool HasQuestTurnIn
    {
        get
        {
            foreach (IndividualDialog d in dialogs)
            {
                if (d.isQuestTurnIn && d.questToComplete != null
                    && questsManager.activeQuests.Contains(d.questToComplete)
                    && InventorySystem.Instance.HasItem(d.questToComplete.whatTheQuestTakes, d.questToComplete.requiredAmount))
                    return true;
            }
            return false;
        }
    }

    private bool HasQuestDialog
    {
        get
        {
            foreach (IndividualDialog d in dialogs)
            {
                if (d.requiresActiveQuest && d.requiredQuest != null
                    && questsManager.activeQuests.Contains(d.requiredQuest)
                    && !d.requiredQuest.isComplete)
                    return true;
            }
            return false;
        }
    }

    private bool HasNormalDialogLeft
    {
        get
        {
            if (allDialogsDone || talkedToday) return false;
            for (int i = progressionIndex; i < dialogs.Count; i++)
            {
                IndividualDialog d = dialogs[i];
                if (d.isResponseDialog || d.isQuestTurnIn) continue;
                if (d.requiresActiveQuest) continue;
                if (d.doesDialogGiveQuest && d.quest != null
                    && (questsManager.activeQuests.Contains(d.quest) || d.quest.isComplete))
                    continue;
                return true;
            }
            return false;
        }
    }

    public GameObject outline;
    public GameObject tooltip;

    private void Start()
    {
        allDialogs.Add(this);
        questsManager = FindAnyObjectByType<QuestsManager>();
        timeManager = FindAnyObjectByType<TimeManager>();
        if (yesButton != null)
            yesButton.onClick.AddListener(() => OnChoice(true));
        if (noButton != null)
            noButton.onClick.AddListener(() => OnChoice(false));
    }

    private void OnDestroy()
    {
        allDialogs.Remove(this);
    }

    private void Update()
    {
        if (shopPanel != null && shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            shopPanel.SetActive(false);

        if (isCutsceneDialog)
        {
            HandleDialogAdvance();
            return;
        }

        // On a new day: advance normal progression and reset daily flag
        if (timeManager.currentDay != lastTalkedDay)
        {
            if (talkedToday && !allDialogsDone)
            {
                progressionIndex++;
                while (progressionIndex < dialogs.Count)
                {
                    IndividualDialog d = dialogs[progressionIndex];
                    if (!d.isResponseDialog && !d.isQuestTurnIn && !d.requiresActiveQuest)
                        break;
                    progressionIndex++;
                }
                if (progressionIndex >= dialogs.Count)
                    allDialogsDone = true;
            }
            talkedToday = false;

            if (pendingTeleport != null)
            {
                gameObject.transform.position = pendingTeleport.transform.position;
                pendingTeleport = null;
            }
        }

        bool wantsShop = opensShop && (dialogs.Count == 0 || allDialogsDone || talkedToday);

        if (mouseOver)
        {
            bool showInteract = !isPlayerTalkingToNPC && activeDialog == null
                && (wantsShop || HasQuestTurnIn || HasQuestDialog || HasNormalDialogLeft);
            outline.SetActive(showInteract);
            tooltip.SetActive(showInteract);
        }
        else
        {
            if (outline != null)
                outline.SetActive(false);
            if (tooltip != null)
                tooltip.SetActive(false);
        }

        if (mouseOver && Input.GetKeyDown(KeyCode.E) && !isPlayerTalkingToNPC && activeDialog == null)
        {
            if (wantsShop) { shopPanel.SetActive(true); return; }

            int targetIndex = -1;
            isQuestDialog = false;

            // 1. Quest turn-in takes highest priority
            for (int i = 0; i < dialogs.Count; i++)
            {
                IndividualDialog d = dialogs[i];
                if (d.isQuestTurnIn && d.questToComplete != null
                    && questsManager.activeQuests.Contains(d.questToComplete)
                    && InventorySystem.Instance.HasItem(d.questToComplete.whatTheQuestTakes, d.questToComplete.requiredAmount))
                {
                    targetIndex = i;
                    isQuestDialog = true;
                    break;
                }
            }

            // 2. Active quest response dialog
            if (targetIndex == -1)
            {
                for (int i = 0; i < dialogs.Count; i++)
                {
                    IndividualDialog d = dialogs[i];
                    if (d.requiresActiveQuest && d.requiredQuest != null
                        && questsManager.activeQuests.Contains(d.requiredQuest)
                        && !d.requiredQuest.isComplete)
                    {
                        targetIndex = i;
                        isQuestDialog = true;
                        break;
                    }
                }
            }

            // 3. Normal daily progression
            if (targetIndex == -1 && HasNormalDialogLeft)
            {
                IndividualDialog d = dialogs[progressionIndex];
                if (d.doesDialogGiveQuest && d.quest != null)
                {
                    if (!questsManager.activeQuests.Contains(d.quest) && !d.quest.isComplete)
                        targetIndex = progressionIndex;
                }
                else
                {
                    targetIndex = progressionIndex;
                }
            }

            if (targetIndex == -1) return;

            currentDialogIndex = targetIndex;
            activeDialog = this;
            isPlayerTalkingToNPC = true;
            dialoguePanel.SetActive(true);
            dialogueHeaderText.text = CurrentDialog.headerContent;
            StartTyping();
        }

        HandleDialogAdvance();
    }

    private void HandleDialogAdvance()
    {
        if (!isCutsceneDialog && activeDialog != null && activeDialog != this) return;

        if (isPlayerTalkingToNPC && Input.GetKeyDown(KeyCode.Return))
        {
            if (awaitingChoice) return;

            if (isTyping)
            {
                StopTyping();
                dialogueContentText.text = CurrentDialog.dialogueContents[currentLineIndex];
            }
            else
            {
                currentLineIndex++;

                if (currentLineIndex >= CurrentDialog.dialogueContents.Count)
                {
                    if (CurrentDialog.hasChoice)
                    {
                        awaitingChoice = true;
                        questionHeaderText.text = CurrentDialog.headerContent;
                        questionText.text = CurrentDialog.choiceQuestion;
                        choicePanel.SetActive(true);
                        return;
                    }

                    // --- Quest turn-in ---
                    if (CurrentDialog.isQuestTurnIn && CurrentDialog.questToComplete != null)
                    {
                        Quest q = CurrentDialog.questToComplete;

                        if (!InventorySystem.Instance.HasItem(q.whatTheQuestTakes, q.requiredAmount))
                        {
                            Debug.Log("FAILED - player does not have the required item.");
                            EndDialog();
                            return;
                        }

                        InventorySystem.Instance.RemoveItem(q.whatTheQuestTakes, q.requiredAmount);

                        if (q.questReward != null)
                            InventorySystem.Instance.AddItem(q.questReward, q.rewardAmount);

                        q.isComplete = true;
                        questsManager.activeQuests.Remove(q);

                        // Quest turn-ins don't consume the daily talk slot
                        EndDialog();
                        return;
                    }

                    // --- Give quest ---
                    if (CurrentDialog.doesDialogGiveQuest && CurrentDialog.quest != null)
                        questsManager.activeQuests.Add(CurrentDialog.quest);

                    // --- Give item ---
                    if (CurrentDialog.givesItem)
                        InventorySystem.Instance.AddItem(CurrentDialog.recievedItem, CurrentDialog.amountofRecievedItem);

                    // --- Teleport ---
                    if (CurrentDialog.isTeleportDialog)
                        pendingTeleport = CurrentDialog.teleportTo;

                    // Only mark talkedToday for normal (non-quest) dialogs
                    if (!isQuestDialog)
                    {
                        talkedToday = true;
                        lastTalkedDay = timeManager.currentDay;

                        if (progressionIndex >= dialogs.Count - 1)
                            allDialogsDone = true;
                    }

                    EndDialog();
                }
                else
                {
                    StartTyping();
                }
            }
        }
    }

    private void EndDialog()
    {
        isPlayerTalkingToNPC = false;
        dialoguePanel.SetActive(false);
        currentLineIndex = 0;
        isQuestDialog = false;

        // Restore currentDialogIndex to normal progression position
        currentDialogIndex = progressionIndex;

        if (activeDialog == this)
            activeDialog = null;
    }

    /// <summary>
    /// Called by TaxCollector to start a cutscene dialog on a freshly spawned NPC instance.
    /// </summary>
    public void StartAsCutscene()
    {
        progressionIndex = 0;
        currentDialogIndex = 0;
        currentLineIndex = 0;
        isPlayerTalkingToNPC = true;
        Dialog.activeDialog = this;
        dialoguePanel.SetActive(true);
        dialogueHeaderText.text = dialogs[0].headerContent;
        StartTyping();
    }

    public IEnumerator PlayDialogAnimation()
    {
        StartTyping();
        yield return typingCoroutine;
    }

    IEnumerator TextAnimation()
    {
        isTyping = true;
        dialogueContentText.text = "";

        foreach (char letter in CurrentDialog.dialogueContents[currentLineIndex])
        {
            dialogueContentText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void OnChoice(bool choseYes)
    {
        if (activeDialog != this) return;

        choicePanel.SetActive(false);
        awaitingChoice = false;
        StopTyping();

        currentDialogIndex = choseYes
            ? CurrentDialog.yesDialogIndex
            : CurrentDialog.noDialogIndex;

        currentLineIndex = 0;
        dialogueHeaderText.text = CurrentDialog.headerContent;
        StartTyping();
    }

    private void OnMouseEnter() { mouseOver = true; }
    private void OnMouseExit() { mouseOver = false; }
}