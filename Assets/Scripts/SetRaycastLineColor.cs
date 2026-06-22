using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class SetRaycastLineColor : MonoBehaviour
{
    public RadialMenuController radialMenuController;
    public LineRenderer lineRenderer;
    public XRInteractorLineVisual lineVisual;

    private Gradient cachedGradient = new Gradient();

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[LineColor] radialMenuController not assigned!"); return; }
        if (lineRenderer == null)         { Debug.LogError("[LineColor] lineRenderer not assigned!"); return; }
        radialMenuController.onOptionConfirmed.AddListener(HandleOption);
        Debug.Log("[LineColor] Listener registered");
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        if (option.id == null || !option.id.StartsWith("ray_")) return;
        ApplyColor(option.displayColor);
        Debug.Log($"[LineColor] Color set to {option.displayColor} from option '{option.id}'");
    }

    void ApplyColor(Color c)
    {
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;

        if (lineVisual != null)
        {
            cachedGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(c.a, 0f), new GradientAlphaKey(c.a, 1f) }
            );
            lineVisual.validColorGradient = cachedGradient;
            lineVisual.invalidColorGradient = cachedGradient;
        }
    }
}
