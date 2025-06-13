using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Exposes Button input value.
/// </summary>
public class ButtonInput : MonoBehaviour
{
    [Header("Debugging")]
    [SerializeField] private bool showButtonData = false;

    public bool Button0Held { get; private set; } = false;
    public bool Button1Held { get; private set; } = false;

    [SerializeField] private UnityEvent _button0Pressed = new();
    [SerializeField] private UnityEvent _button1Pressed = new();

    /// <summary>
    /// Invoked on the frame Button 0 (top) is pressed.
    /// </summary>
    public UnityEvent Button0Pressed => _button0Pressed;
    /// <summary>
    /// Invoked on the frame Button 1 (button) is pressed.
    /// </summary>
    public UnityEvent Button1Pressed => _button0Pressed;

    /// <summary>
    /// Begins the process of reading Button data.
    /// </summary>=
    public void StartReadingButtonData(string characteristicUUID)
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() => StartCoroutine(ReadButtonData(characteristicUUID)));
    }

    private IEnumerator ReadButtonData(string characteristicUuid)
    {
        while (true)
        {
            bool wasButton0PreviouslyPressed = Button0Held;
            Button0Held = Input.GetKey(KeyCode.A);
            if (!wasButton0PreviouslyPressed && Button0Held)
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() => _button0Pressed.Invoke());
                if (showButtonData) Debug.Log("Button 0 Pressed!");
            }

            bool wasButton1PreviouslyPressed = Button1Held;
            Button1Held = Input.GetKey(KeyCode.B);
            if (!wasButton0PreviouslyPressed && Button1Held)
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() => _button1Pressed.Invoke());
                if (showButtonData) Debug.Log("Button 1 Pressed!");
            }

            yield return new WaitForSecondsRealtime(0.01f);
        }
    }
}
