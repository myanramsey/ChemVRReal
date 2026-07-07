using UnityEngine;
using TMPro;

// Attach this to a world-space Canvas parented to the left hand.
// Displays the current interaction mode above the hand at all times.
public class ModeIndicator : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI modeText;
    public RadialMenuController radialMenuController;

    [Header("Font")]
    public int fontSize = 24;
    public Color textColor = Color.white;

    private string currentMode = "Normal";

    void Start()
    {
        if (modeText == null)
            modeText = GetComponentInChildren<TextMeshProUGUI>();

        if (modeText != null)
        {
            modeText.fontSize = fontSize;
            modeText.color = textColor;
            modeText.alignment = TextAlignmentOptions.Center;
            modeText.overflowMode = TextOverflowModes.Overflow;
        }

        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.AddListener(HandleOption);

        UpdateDisplay();
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void LateUpdate()
    {
        // Billboard — always face the user's headset.
        Camera cam = Camera.main;
        if (cam != null)
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }

    void HandleOption(RadialMenuOption option)
    {
        if (option.id == "delete_molecule")
            SetMode("Delete Mode");
    }

    public void SetMode(string mode)
    {
        currentMode = mode;
        UpdateDisplay();
    }

    public void ResetToNormal()
    {
        currentMode = "Normal";
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (modeText != null)
            modeText.text = currentMode;
    }
}
