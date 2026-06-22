using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SVImageControl : MonoBehaviour, IDragHandler, IPointerClickHandler
{
    [SerializeField] private Image colorPickerImage;

    private RawImage svImage;

    private ColorPickerControl cpc;

    private RectTransform rectTransform, colorPickerTransform;

    private void Awake()
    {
        svImage = GetComponent<RawImage>();
        cpc = FindAnyObjectByType<ColorPickerControl>();
        rectTransform= GetComponent<RectTransform>();

        colorPickerTransform = colorPickerImage.GetComponent<RectTransform>();
        colorPickerTransform.anchoredPosition = Vector2.zero;
    }

    void UpdateColor(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);      
        Vector3 pos = localPoint;

        float halfWidth = rectTransform.rect.width * 0.5f;
        float halfHeight = rectTransform.rect.height * 0.5f;

        // Clamp color picker inside SV image
        pos.x = Mathf.Clamp(pos.x, -halfWidth, halfWidth);
        pos.y = Mathf.Clamp(pos.y, -halfHeight, halfHeight);

        float x = pos.x + halfWidth;
        float y = pos.y + halfHeight;

        // Normalize SV values between 0 and 1
        float xNorm = x / rectTransform.sizeDelta.x;
        float yNorm = y / rectTransform.sizeDelta.y;

        // Update color picker position and color
        colorPickerTransform.anchoredPosition = pos;
        colorPickerImage.color = Color.HSVToRGB(0, 0, 1 - yNorm);

        // Send SV values to ColorPickerControl to update GameObject's color
        cpc.SetSV(xNorm, yNorm);
    }
    public void OnDrag(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UpdateColor(eventData);
    }
}
