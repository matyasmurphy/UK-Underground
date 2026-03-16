using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaxCollector : MonoBehaviour
{
    private TimeManager timeManager;
    private PlayerMovement playerMovement;

    public FadePanel fadePanel;
    public GameObject taxEnforcerPrefab;
    public int potatoQuote;
    public bool isTaxDialogPlaying = false;

    [Header("Dialogs")]
    public List<IndividualDialog> gotPotatoesDialogs;
    public List<IndividualDialog> noGotPotatoesDialogs;

    [Header("Dialog UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueHeaderText;
    public TextMeshProUGUI dialogueContentText;
    public GameObject choicePanel;
    public TextMeshProUGUI questionHeaderText;
    public TextMeshProUGUI questionText;
    public Button yesButton;
    public Button noButton;

    [Header("Cutscene")]
    public float walkSpeed = 2f;
    public float stopDistance = 1.5f;
    public Transform homePosition;

    private static readonly int AnimMoving = Animator.StringToHash("isMoving");
    private static readonly int AnimFaceX = Animator.StringToHash("moveX");
    private static readonly int AnimFaceY = Animator.StringToHash("moveY");

    private void Start()
    {
        timeManager = FindAnyObjectByType<TimeManager>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
    }

    public void OnPlayerSleptOnSunday()
    {
        bool hasPotatoes = InventorySystem.Instance.HasItem(InventorySystem.Instance.potato);
        StartCoroutine(TaxCutscene(hasPotatoes));
    }

    private IEnumerator TaxCutscene(bool hasPotatoes)
    {
        isTaxDialogPlaying = true;

        // 1. Wait for screen to go fully black
        yield return new WaitUntil(() => fadePanel.IsFullyBlack);

        // 2. Instantiate enforcer at home position while still black
        GameObject npcInstance = Instantiate(taxEnforcerPrefab, homePosition.position, Quaternion.identity);
        Dialog dialog = npcInstance.GetComponent<Dialog>();

        // 3. Inject scene UI references into the spawned prefab
        dialog.dialoguePanel = dialoguePanel;
        dialog.dialogueHeaderText = dialogueHeaderText;
        dialog.dialogueContentText = dialogueContentText;
        dialog.choicePanel = choicePanel;
        dialog.questionHeaderText = questionHeaderText;
        dialog.questionText = questionText;
        dialog.yesButton = yesButton;
        dialog.noButton = noButton;

        // 4. Assign the correct dialog lines
        dialog.dialogs = hasPotatoes ? gotPotatoesDialogs : noGotPotatoesDialogs;

        // 5. Freeze player, fade back in
        Transform player = playerMovement.transform;
        playerMovement.enabled = false;
        fadePanel.HoldAtBlack = false;

        // 6. Walk toward the player from home position
        yield return StartCoroutine(WalkTo(npcInstance, player.position, stopDistance));
        FaceTarget(npcInstance, player.position);
        SetMovingAnim(npcInstance, false);

        // 7. Take potatoes before dialog so it feels like he collects them
        if (hasPotatoes)
            InventorySystem.Instance.RemoveItem(InventorySystem.Instance.potato, potatoQuote);

        // 8. Play dialog, wait for player to finish it
        yield return StartCoroutine(PlayCutsceneDialog(dialog));

        // 9. Walk back to home position
        yield return StartCoroutine(WalkTo(npcInstance, homePosition.position, 0.05f));
        SetMovingAnim(npcInstance, false);

        // 10. Destroy the instance
        Destroy(npcInstance);

        // 11. Restore player control
        playerMovement.enabled = true;
        isTaxDialogPlaying = false;
    }

    // ── Walk using Transform.MoveTowards ─────────────────────────────────────

    private IEnumerator WalkTo(GameObject npc, Vector3 destination, float arrivalThreshold)
    {
        Animator npcAnimCached = npc.GetComponent<Animator>();
        if (npcAnimCached != null) npcAnimCached.enabled = true;

        SetMovingAnim(npc, true);

        while (true)
        {
            if (npc == null) yield break;

            Vector3 flatSelf = new Vector3(npc.transform.position.x, npc.transform.position.y, 0f);
            Vector3 flatDest = new Vector3(destination.x, destination.y, 0f);

            if (Vector3.Distance(flatSelf, flatDest) <= arrivalThreshold)
                break;

            Vector3 dir = (flatDest - flatSelf).normalized;

            FaceTarget(npc, destination);

            if (npcAnimCached != null)
            {
                npcAnimCached.SetFloat(AnimFaceX, dir.x);
                npcAnimCached.SetFloat(AnimFaceY, dir.y);
            }

            npc.transform.position = Vector3.MoveTowards(
                npc.transform.position,
                new Vector3(destination.x, destination.y, npc.transform.position.z),
                walkSpeed * Time.deltaTime
            );

            yield return null;
        }

        SetMovingAnim(npc, false);
    }

    // ── Play a Dialog as a cutscene and wait for it to finish ────────────────

    private IEnumerator PlayCutsceneDialog(Dialog dialog)
    {
        if (Dialog.activeDialog != null)
        {
            Dialog.activeDialog.dialoguePanel.SetActive(false);
            Dialog.activeDialog.isPlayerTalkingToNPC = false;
            Dialog.activeDialog = null;
        }

        dialog.currentDialogIndex = 0;
        dialog.currentLineIndex = 0;
        dialog.isPlayerTalkingToNPC = true;
        Dialog.activeDialog = dialog;

        dialog.dialoguePanel.SetActive(true);
        dialog.dialogueHeaderText.text = dialog.dialogs[0].headerContent;
        StartCoroutine(dialog.PlayDialogAnimation());

        yield return new WaitUntil(() => !dialog.isPlayerTalkingToNPC);
        Dialog.activeDialog = null;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetMovingAnim(GameObject npc, bool moving)
    {
        Animator npcAnim = npc.GetComponent<Animator>();
        if (npcAnim == null) return;

        if (moving)
        {
            npcAnim.enabled = true;
            npcAnim.SetBool(AnimMoving, true);
        }
        else
        {
            npcAnim.SetBool(AnimMoving, false);
            npcAnim.SetFloat(AnimFaceX, 0f);
            npcAnim.SetFloat(AnimFaceY, 0f);
            npcAnim.enabled = false; // Freeze on current frame
        }
    }

    private void FaceTarget(GameObject npc, Vector3 targetPos)
    {
        Vector3 s = npc.transform.localScale;
        s.x = (targetPos.x < npc.transform.position.x) ? -Mathf.Abs(s.x) : Mathf.Abs(s.x);
        npc.transform.localScale = s;
    }
}