using UnityEngine;

/// <summary>
/// Handles rendering of the fishing line between the fishing rod tip and the bobber
/// </summary>
public class FishingLineRenderer : MonoBehaviour
{
    [SerializeField] private Transform rodTip;  // Starting point
    [SerializeField] private Transform hook;  // End point

    [SerializeField] private int segments = 20;  // More segments = smoother line
    [SerializeField] private float slack = 0.3f;  // Controls how much the line hangs

    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments;
        //lineRenderer.startWidth = 0.08f;
        //lineRenderer.endWidth = 0.02f;
        lineRenderer.startWidth = 0.015f;
        lineRenderer.endWidth = 0.015f;
    }

    void Update()
    {
        if (rodTip != null && hook != null)
        {
            UpdateFishingLine();
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
}
