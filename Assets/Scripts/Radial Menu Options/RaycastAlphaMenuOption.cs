using UnityEngine;

// Attach to the same GameObject as RadialMenuController (or anywhere in the scene).
// Bridges the radial menu's onOptionConfirmed event to OrbitalOpacity's AdjustOpacity().
public class RaycastAlphaMenuOption : MonoBehaviour
{
    public RadialMenuController radialMenuController;
    public OrbitalOpacity orbitalOpacity;

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[AlphaMenu] radialMenuController not assigned."); return; }
        if (orbitalOpacity == null)       { Debug.LogError("[AlphaMenu] orbitalOpacity not assigned."); return; }

        radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        if (option.id == "alpha_increase")
            orbitalOpacity.AdjustOpacity(true);
        else if (option.id == "alpha_decrease")
            orbitalOpacity.AdjustOpacity(false);
    }
}
