using UnityEngine;

public class FishingLineController : MonoBehaviour
{
    [SerializeField] private Transform rodTip;  // Starting point
    [SerializeField] private Transform hook;  // End point
    [SerializeField] private Rigidbody hookRigidbody;  // Rigidbody of the hook

    [SerializeField] private int segments = 20;  // More segments = smoother line
    [SerializeField] private float slack = 0.3f;  // Controls how much the line hangs
    [SerializeField] private float maxLineLength = 5f;  // Maximum allowed distance between rod tip and hook
    [SerializeField] private float easingSpeed = 2f;  // Speed of easing when transitioning to limited mode

    private bool isLineLimited = false;  // Whether the line length is limited
    private bool isEasing = false;  // Whether the hook is currently easing toward the limit
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.005f;
    }

    void Update()
    {
        if (rodTip != null && hook != null)
        {
            if (isLineLimited)
            {
                if (isEasing)
                {
                    EaseHookToLimit();
                }
                else
                {
                    EnforceLineLength();
                }
            }

            UpdateFishingLine();
        }
    }

    // Method to toggle the line length limitation
    public void SetLimitedLength(bool limit)
    {
        if (limit && !isLineLimited)
        {
            // Start easing when switching to limited mode
            isEasing = true;
        }
        isLineLimited = limit;
    }

    // Smoothly move the hook toward the maximum line length
    void EaseHookToLimit()
    {
        Vector3 direction = hook.position - rodTip.position;
        float distance = direction.magnitude;

        if (distance > maxLineLength)
        {
            // Calculate the target position at the maximum line length
            Vector3 targetPosition = rodTip.position + direction.normalized * maxLineLength;

            // Move the hook toward the target position using Lerp for easing
            hook.position = Vector3.Lerp(hook.position, targetPosition, Time.deltaTime * easingSpeed);

            // Stop easing if the hook is close enough to the target position
            if (Vector3.Distance(hook.position, targetPosition) < 0.01f)
            {
                isEasing = false;
            }
        }
        else
        {
            // Stop easing if the hook is already within the limit
            isEasing = false;
        }
    }

    // Enforce the maximum line length by clamping the hook's position
    void EnforceLineLength()
    {
        Vector3 direction = hook.position - rodTip.position;
        float distance = direction.magnitude;

        if (distance > maxLineLength)
        {
            // Clamp the hook's position to the maximum line length
            hook.position = rodTip.position + direction.normalized * maxLineLength;
        }
    }

    void UpdateFishingLine()
    {
        Vector3 start = rodTip.position;
        Vector3 end = hook.position;
        float distance = Vector3.Distance(start, end);

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            Vector3 middle = Vector3.Lerp(start, end, t);  // Linear interpolation

            // Add slack to simulate natural rope physics
            float sag = Mathf.Sin(t * Mathf.PI) * Mathf.Clamp(distance * slack * 0.5f, 0.05f, 0.7f);
            middle.y -= sag;

            lineRenderer.SetPosition(i, middle);
        }
    }

    public void Cast(float castForce, float drag)
    {
        SetLimitedLength(false);
        hookRigidbody.velocity = transform.forward * castForce;
        hookRigidbody.drag = drag;  // Lower drag to allow a longer cast
    }
}
