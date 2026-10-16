using Unity.XR.CoreUtils;
using UnityEngine;

public class ShowMOSliderMenu : MonoBehaviour
{
    [Header("Radial Menu")]
    public RadialMenuController radialMenuController;
    public ModeIndicator modeIndicator;

    [HideInInspector] public bool moMode = false;

    void Start()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        if (option.id != "mo_slider") return;

        if (option.id == "mo_slider")
        {
            moMode = true;
            modeIndicator?.SetMode("MO Mode");
        }
    }

    public void ExitMOMode()
    {
        moMode = false;
        modeIndicator?.ResetToNormal();
    }
}
