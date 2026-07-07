using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class SetRaycastLineColor : MonoBehaviour
{
    public RadialMenuController radialMenuController;
    public LineRenderer lineRenderer;
    public XRInteractorLineVisual lineVisual;

    [Header("Alpha Settings")]
    [Range(0.05f, 0.5f)]
    public float alphaStep = 0.1f;

    private Gradient cachedGradient = new Gradient();
    private Color currentColor = Color.white;

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
        if (option.id == null) return;

        if (option.id == "ray_alpha_increase")
        {
            currentColor.a = Mathf.Clamp01(currentColor.a + alphaStep);
            ApplyColor(currentColor);
            return;
        }

        if (option.id == "ray_alpha_decrease")
        {
            currentColor.a = Mathf.Clamp01(currentColor.a - alphaStep);
            ApplyColor(currentColor);
            return;
        }

        if (!option.id.StartsWith("ray_")) return;
        currentColor = option.displayColor;
        ApplyColor(currentColor);
        Debug.Log($"[LineColor] Color set to {currentColor} from option '{option.id}'");
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
