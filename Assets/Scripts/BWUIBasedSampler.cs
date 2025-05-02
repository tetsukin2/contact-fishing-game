using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class BWUIBasedSampler : MonoBehaviour
{
    [System.Serializable]
    public class BWImage
    {
        public Image uiImage;                 // The black-and-white image to detect over
        [HideInInspector] public Texture2D tex;           // Cached texture
        [HideInInspector] public Color32[] pixelData;     // Cached pixels
        [HideInInspector] public int width, height;       // Texture dimensions
        [HideInInspector] public RectTransform rectTransform; // Cached rectTransform
    }

    [Header("Input")]
    public Image[] pointImages;     // Small UI images that represent points around the cursor
    public BWImage[] bwImages;      // All possible BW images to check against

    [Header("Colors")]
    public Color blackColor = Color.red;     // Color if over black pixel
    public Color whiteColor = Color.blue;    // Color if over white or outside

    [Header("Output")]
    public UnityEvent<Image> onImageSelected;       // UnityEvent triggered when the most-hit image changes

    private BWImage mostHitImage = null;     // Internally tracked most-hit image
    private BWImage lastSelectedImage = null;

    void Start()
    {
        // Cache texture and pixel data for each BWImage
        foreach (var img in bwImages)
        {
            img.tex = img.uiImage.sprite.texture;

            if (!img.tex.isReadable)
            {
                Debug.LogError($"Texture on {img.uiImage.name} must be readable (enable 'Read/Write' in import settings).");
                continue;
            }

            img.pixelData = img.tex.GetPixels32();
            img.width = img.tex.width;
            img.height = img.tex.height;
            img.rectTransform = img.uiImage.rectTransform;
        }
    }

    void Update()
    {
        Dictionary<BWImage, int> hitCounts = new Dictionary<BWImage, int>();
        mostHitImage = null;

        // For each point, detect which image it intersects
        foreach (var point in pointImages)
        {
            Vector2 screenPoint = point.rectTransform.position; // Center position in screen space
            Color32 pixel = default;
            BWImage hitImage = null;

            // Try all BW images, find first one that contains the point
            foreach (var img in bwImages)
            {
                if (TryGetPixelFromImage(img, screenPoint, out pixel))
                {
                    hitImage = img;
                    break; // Only take the first one found
                }
            }

            // Set color based on detection
            if (hitImage != null)
            {
                point.color = pixel.r < 128 ? blackColor : whiteColor;

                // Track hit count
                if (!hitCounts.ContainsKey(hitImage))
                    hitCounts[hitImage] = 0;

                hitCounts[hitImage]++;
            }
            else
            {
                point.color = whiteColor;
            }
        }

        // Determine which image had the most hits
        int maxHits = 0;
        foreach (var kvp in hitCounts)
        {
            if (kvp.Value > maxHits)
            {
                maxHits = kvp.Value;
                mostHitImage = kvp.Key;
            }
        }

        // Trigger UnityEvent if selected image changed
        if (mostHitImage != lastSelectedImage)
        {
            lastSelectedImage = mostHitImage;
            onImageSelected.Invoke(mostHitImage != null ? mostHitImage.uiImage : null);
        }
    }

    /// <summary>
    /// Checks if a given screen point intersects a BW image, and returns the pixel at that point.
    /// </summary>
    bool TryGetPixelFromImage(BWImage img, Vector2 screenPos, out Color32 pixel)
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
