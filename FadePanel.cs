using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class FadePanel : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private Animator playerAnimator;

    public bool IsFading { get; private set; }
    public bool IsFullyBlack { get; private set; }
    public bool HoldAtBlack { get; set; }

    public float duration = 0.4f;
    private CanvasGroup canvasGroup;
    private CinemachineCamera cinemachineCamera;
    private CinemachineConfiner2D confiner2D;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        confiner2D = FindAnyObjectByType<CinemachineConfiner2D>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerAnimator = playerMovement.GetComponent<Animator>();
    }

    public void Fade(Transform player, Transform teleportTo, GameObject objectToDestroy, bool spawnMinerals = false, Collider2D newConfiner = null)
    {
        StartCoroutine(DoFade(player, teleportTo, objectToDestroy, spawnMinerals, newConfiner));
    }

    public void SkipDayFade()
    {
        StartCoroutine(DoSkipDayFade());
    }

    private IEnumerator DoFade(Transform player, Transform teleportTo, GameObject objectToDestroy, bool spawnMinerals, Collider2D newConfiner)
    {
        IsFading = true;

        playerAnimator.enabled = false;
        cinemachineCamera.enabled = false;

        // Fade to black
        float counter = 0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, counter / duration);
            yield return null;
        }

        // Disable damping
        var transposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();
        float originalDampingX = 0f, originalDampingY = 0f;
        if (transposer != null)
        {
            originalDampingX = transposer.Damping.x;
            originalDampingY = transposer.Damping.y;
            transposer.Damping = Vector3.zero;
            transposer.CenterOnActivate = false;
        }

        float originalConfinerDamping = confiner2D.Damping;
        confiner2D.Damping = 0f;

        // Teleport player
        player.position = new Vector3(teleportTo.position.x, teleportTo.position.y + 0.5f, teleportTo.position.z);

        if (objectToDestroy != null) Destroy(objectToDestroy);
        if (spawnMinerals) FindAnyObjectByType<MineManager>().SpawnMinerals();

        // Switch confiner
        if (newConfiner != null)
        {
            confiner2D.enabled = false;
            confiner2D.BoundingShape2D = newConfiner;
            confiner2D.InvalidateBoundingShapeCache();
            confiner2D.enabled = true;
        }

        // Get the brain and use it to snap
        var brain = FindAnyObjectByType<CinemachineBrain>();
        brain.enabled = false;

        Camera.main.transform.position = new Vector3(
            teleportTo.position.x,
            teleportTo.position.y + 0.5f,
            Camera.main.transform.position.z
        );

        cinemachineCamera.enabled = true;
        yield return null;

        brain.enabled = true;

        yield return null;
        yield return null;

        // Restore damping
        if (transposer != null)
        {
            transposer.Damping = new Vector3(originalDampingX, originalDampingY, 1);
            transposer.CenterOnActivate = true;
        }
        confiner2D.Damping = originalConfinerDamping;

        playerMovement.enabled = true;
        playerAnimator.enabled = true;

        // Fade back in
        counter = 0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, counter / duration);
            yield return null;
        }

        IsFading = false;
    }

    private IEnumerator DoSkipDayFade()
    {
        IsFullyBlack = false;

        // Fade to black
        float counter = 0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, counter / duration);
            yield return null;
        }

        IsFullyBlack = true;

        GameObject.FindAnyObjectByType<TimeManager>().WakeUp();
        yield return null;
        yield return null;

        yield return new WaitUntil(() => !HoldAtBlack);

        // Fade back in
        counter = 0f;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, counter / duration);
            yield return null;
        }
    }
}
