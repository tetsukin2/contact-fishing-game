using UnityEngine;

public class FishingLineController : MonoBehaviour
{
    public Transform rodTip;  // Starting point
    public Transform hook;  // End point
    private LineRenderer lineRenderer;
    public int segments = 20;  // More segments = smoother line
    public float slack = 0.3f;  // Controls how much the line hangs

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
