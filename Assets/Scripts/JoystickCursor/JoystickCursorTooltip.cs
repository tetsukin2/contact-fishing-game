using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoystickCursorTooltip : MonoBehaviour
{
    public RectTransform Content;        // The root panel of the tooltip
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public Vector2 defaultOffset = new(20, -20);

    private RectTransform _canvasRectTransform;
    private RectTransform _cursorRectTransform;

    private bool isVisible = false;
    private Vector2 _offset;

    private void Start()
    {
        Hide();
    }

    public void Show(string title, string message)
    {
        Title.text = title;
        Description.text = message;
        Content.gameObject.SetActive(true);
        isVisible = true;
        UpdatePosition();
    }

    public void Hide()
    {
        Content.gameObject.SetActive(false);
        isVisible = false;
    }

    void Update()
    {
        if (isVisible)
            UpdatePosition();
    }

    public void Initialize(RectTransform cursorTransform, RectTransform canvasTransform)
    {
        _cursorRectTransform = cursorTransform;
        _canvasRectTransform = canvasTransform;
        _offset = transform.position - cursorTransform.position;
    }

    void UpdatePosition()
    {
        Vector2 cursorScreenPos = _cursorRectTransform.position;
        Vector2 offset = _offset;

        Content.pivot = new Vector2(0f, 1f); // default: top-left

        // Calculate preferred size
        Vector2 size = Content.sizeDelta;

        //float canvasWidth = _canvasRectTransform.sizeDelta.x * 0.5f;
        //float canvasHeight = _canvasRectTransform.sizeDelta.y * 0.5f;

        float canvasWidth = _canvasRectTransform.sizeDelta.x;
        float canvasHeight = _canvasRectTransform.sizeDelta.y;

        float tooltipRightPos = cursorScreenPos.x + _offset.x + size.x;
        float tooltipTop = cursorScreenPos.y + _offset.y;
        float tooltipBottom = tooltipTop - size.y;

        // If tooltip goes off right edge, switch to left
        if (tooltipRightPos > canvasWidth)
        {
            Debug.Log("leftswapping");
            offset.x = -offset.x - size.x;
            Content.pivot = new Vector2(1f, Content.pivot.y); // flip horizontally
        }

        //// If tooltip goes off top, flip down
        //if (tooltipTop > canvasHeight)
        //{
        //    Debug.Log("downswapping");
        //    offset.y = -offset.y;
        //    Content.pivot = new Vector2(Content.pivot.x, 0f); // flip vertically
        //}

        //// If tooltip goes off bottom, flip up again
        //if (tooltipBottom < 0)
        //{
        //    Debug.Log("upswapping");
        //    offset.y = Mathf.Abs(offset.y);
        //    Content.pivot = new Vector2(Content.pivot.x, 1f);
        //}

        // Update with adjusted values
        //Content.position = cursorScreenPos + offset;
        transform.position = cursorScreenPos + offset;
    }
}