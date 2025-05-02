using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class ImagePixelSampler : MonoBehaviour
{
    [Header("Input")]
    public List<JoystickCursorSelectable> SelectablesToSample; // All possible BW images to check against

    [Header("Colors")]
    public float AlphaThreshold = 50f; // Transparent color for no hit

    public UnityEvent<Image> onImageSelected = new(); // UnityEvent triggered when the most-hit image changes

    public JoystickCursorSelectable CurrentSelectable { get; private set; } = null; // Internally tracked most-hit image

    private List<CursorBraillePin> _braillePins; // Small UI images that represent points around the cursor

    private void Start()
    {
        // Get our own copy of the braille pins
        _braillePins = new List<CursorBraillePin>(UIManager.Instance.JoystickCursor.GetBraillePins());
        InputDeviceManager.Instance.JoystickPressed.AddListener(OnJoystickPressed);
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.Instance.EncyclopediaState) return;

        Dictionary<JoystickCursorSelectable, int> hitCounts = new();

        // For each point, detect which image it intersects
        foreach (var point in _braillePins)
        {
            Vector2 screenPoint = point.rectTransform.position; // Center position in screen space
            Color32 pixel = default;
            JoystickCursorSelectable hitSelectable = null;

            // Try all BW images, find first one that contains the point
            foreach (var selectable in SelectablesToSample)
            {
                if (TryGetPixelFromImage(selectable, screenPoint, out pixel))
                {
                    hitSelectable = selectable;
                    break; // Only take the first one found
                }
            }

            // Set color based on detection
            if (hitSelectable != null)
            {
                point.SetActuated(pixel.r > 128
                    && pixel.g > 128
                    && pixel.b > 128
                    && pixel.a >= AlphaThreshold);

                // Track hit count
                if (!hitCounts.ContainsKey(hitSelectable))
                    hitCounts[hitSelectable] = 0;

                hitCounts[hitSelectable]++;
            }
            else
            {
                point.SetActuated(false);
            }
        }

        // Determine which image had the most hits
        JoystickCursorSelectable newSelectable = null;
        int maxHits = 0;
        foreach (var kvp in hitCounts)
        {
            if (kvp.Value > maxHits)
            {
                maxHits = kvp.Value;
                newSelectable = kvp.Key;
            }
        }

        // Update CurrentSelectable only if it changes
        if (newSelectable != CurrentSelectable)
        {
            CurrentSelectable = newSelectable;
            onImageSelected.Invoke(CurrentSelectable != null ? CurrentSelectable.BWImage : null);

            // Update hover states for all selectables
            foreach (var selectable in SelectablesToSample)
            {
                selectable.SetHover(selectable == CurrentSelectable);
            }
        }
    }

    private void OnJoystickPressed()
    {
        if (GameManager.Instance.CurrentState != GameManager.Instance.EncyclopediaState) return;

        CurrentSelectable?.OnSelect(); // Call OnSelect on the currently selected image
    }

    /// <summary>
    /// Checks if a given screen point intersects a BW image, and returns the pixel at that point.
    /// </summary>
    bool TryGetPixelFromImage(JoystickCursorSelectable img, Vector2 screenPos, out Color32 pixel)
    {
        Vector2 localPoint;

        // Convert screen point to local UI position
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(img.rectTransform, screenPos, null, out localPoint))
        {
            pixel = default;
            return false;
        }

        // Convert to UV coordinates [0,1]
        Vector2 pivot = img.rectTransform.pivot;
        float uvX = (localPoint.x + img.rectTransform.rect.width * pivot.x) / img.rectTransform.rect.width;
        float uvY = (localPoint.y + img.rectTransform.rect.height * pivot.y) / img.rectTransform.rect.height;

        // Out of bounds check
        if (uvX < 0f || uvX > 1f || uvY < 0f || uvY > 1f)
        {
            pixel = default;
            return false;
        }

        // Convert to texture pixel index
        int px = Mathf.FloorToInt(uvX * img.width);
        int py = Mathf.FloorToInt(uvY * img.height);
        int index = py * img.width + px;

        // Guard against bad index
        if (index < 0 || index >= img.pixelData.Length)
        {
            pixel = default;
            return false;
        }

        pixel = img.pixelData[index];
        return true;
    }
}
