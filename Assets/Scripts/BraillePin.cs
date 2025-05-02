using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))] // Ensure UI representation
public class BraillePin : MonoBehaviour
{
    [SerializeField] private Color _pinUpColor = Color.red;     // Color if over black 
    [SerializeField] private Color _pinDownColor = Color.blue;    // Color if over white or outside
    
    private Image _pinImage; // The UI image representing the pin

    public RectTransform rectTransform => _pinImage.rectTransform; // Expose the rectTransform for external use
    public int Value { get; private set; } = 0; // Track if the pin is active as an int

    private void Awake()
    {
        _pinImage = GetComponent<Image>();
    }

    public void SetActuated(bool isActive)
    {
        _pinImage.color = isActive ? _pinUpColor : _pinDownColor;
        // TODO: SWAP LATER I THINK ITS INVERTED IDK
        Value = isActive ? 0 : 1; // Set the value based on the pin state
        
    }
}
