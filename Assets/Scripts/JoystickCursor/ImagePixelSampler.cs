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
    //private bool _tooltipShowing = false;

    private void Start()
    {
        // Get our own copy of the braille pins
        _braillePins = new List<CursorBraillePin>(UIManager.Instance.JoystickCursor.GetBraillePins());
        InputDeviceManager.Instance.JoystickPressed.AddListener(OnJoystickPressed);
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameManager.Instance.EncyclopediaState) return;
        //Debug.Log(CurrentSelectable);

        Dictionary<JoystickCursorSelectable, int> hitCounts = new();
        Dictionary<JoystickCursorSelectable, int> actuationCounts = new();

        int pinsThatWouldActivate = 0;
        // For each point, detect which image it intersects
        foreach (var point in _braillePins)
        {
            Vector2 screenPoint = point.rectTransform.position; // Center position in screen space

            // Get the best selectable
            JoystickCursorSelectable bestSelectable = null;
            Color32 bestPixel = default;

            foreach (var selectable in SelectablesToSample)
            {
                if (!selectable.IsSelectable) continue;

                if (TryGetPixelFromImage(selectable, screenPoint, out Color32 candidatePixel))
                {
                    bool pixelWouldActuate = candidatePixel.r > 128 &&
                                             candidatePixel.g > 128 &&
                                             candidatePixel.b > 128 &&
                                             candidatePixel.a >= AlphaThreshold;

                    // Use the first selectable that would actuate
                    if (pixelWouldActuate)
                    {
                        bestSelectable = selectable;
                        bestPixel = candidatePixel;
                        break; // Stop early if it's good
                    }

                    // Otherwise, keep the pixel in case no actuating one is found
                    if (bestSelectable == null)
                    {
                        bestSelectable = selectable;
                        bestPixel = candidatePixel;
                    }
                }
            }

            if (bestSelectable != null)
            {
                bool pixelWouldActuate = bestPixel.r > 128 &&
                                          bestPixel.g > 128 &&
                                          bestPixel.b > 128 &&
                                          bestPixel.a >= AlphaThreshold;

                //Debug.Log($"Triggers actuation: {bestSelectable.TriggersActuation}");
                bool finalActuation = bestSelectable.TriggersActuation && pixelWouldActuate;

                point.SetActuated(finalActuation);

                // Count hits for hover logic
                if (!hitCounts.ContainsKey(bestSelectable))
                    hitCounts[bestSelectable] = 0;
                hitCounts[bestSelectable]++;

                // Count actuation separately
                if (pixelWouldActuate)
                {
                    if (!actuationCounts.ContainsKey(bestSelectable))
                        actuationCounts[bestSelectable] = 0;
                    actuationCounts[bestSelectable]++;

                    pinsThatWouldActivate += point.Value;
                }
            }
            else
            {
                point.SetActuated(false);
            }
        }

        // Determine which image had the most hits
        JoystickCursorSelectable newSelectable = null;
        int maxActuations = 0;
        foreach (var kvp in actuationCounts)
        {
            if (kvp.Value > maxActuations)
            {
                maxActuations = kvp.Value;
                newSelectable = kvp.Key;
            }
        }

        // Update CurrentSelectable only if it changes
        if (newSelectable != CurrentSelectable)
        {
            CurrentSelectable = newSelectable;
            onImageSelected.Invoke(CurrentSelectable != null ? CurrentSelectable.BWImage : null);

            if (CurrentSelectable != null)
            {
                UIManager.Instance.JoystickCursor.Tooltip.Show(CurrentSelectable.TooltipText);
            }
            else
            {
                UIManager.Instance.JoystickCursor.Tooltip.Hide();
            }

            // Update hover states for all selectables
            foreach (var selectable in SelectablesToSample)
            {
                selectable.SetHover(selectable == CurrentSelectable);
            }
        }

        // Handle tooltip visibility
        //if (!_tooltipShowing && CurrentSelectable != null && pinsThatWouldActivate > 0)
        //{
        //    _tooltipShowing = true;
        //    UIManager.Instance.JoystickCursor.Tooltip.Show(
        //        CurrentSelectable.TooltipTitle,
        //        CurrentSelectable.TooltipDescription);
        //}
        //else if (_tooltipShowing && pinsThatWouldActivate == 0)
        //{
        //    _tooltipShowing = false;
        //    UIManager.Instance.JoystickCursor.Tooltip.Hide();
        //}
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
        pixel = default;

        // Null check for img
        if (img == null)
        {
            Debug.LogError("JoystickCursorSelectable is null.");
            return false;
        }

        // Null check for rectTransform
        if (img.rectTransform == null)
        {
            Debug.LogError($"RectTransform is null for JoystickCursorSelectable: {img.name}");
            return false;
        }

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
