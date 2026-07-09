using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

public class SetRaycastLineColor : MonoBehaviour
{
    public RadialMenuController radialMenuController;

    [Header("Right Hand")]
    public LineRenderer lineRenderer;
    public XRInteractorLineVisual lineVisual;

    [Header("Left Hand")]
    public LineRenderer lineRendererLeft;
    public XRInteractorLineVisual lineVisualLeft;

    [Header("Alpha Settings")]
    [Range(0.05f, 0.5f)]
    public float alphaStep = 0.1f;

    private Gradient cachedGradient = new Gradient();
    private Gradient cachedGradientLeft = new Gradient();
    private Color currentColor = Color.white;

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[LineColor] radialMenuController not assigned!"); return; }
        if (lineRenderer == null)         { Debug.LogError("[LineColor] lineRenderer not assigned!"); return; }
        if (lineRendererLeft == null)     { Debug.LogWarning("[LineColor] lineRendererLeft not assigned - left hand ray color will not update"); }
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
        ApplyColorTo(lineRenderer, lineVisual, cachedGradient, c);
        ApplyColorTo(lineRendererLeft, lineVisualLeft, cachedGradientLeft, c);
    }

    void ApplyColorTo(LineRenderer renderer, XRInteractorLineVisual visual, Gradient gradient, Color c)
    {
        if (renderer == null) return;

        renderer.startColor = c;
        renderer.endColor = c;

        if (visual != null)
        {
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(c.a, 0f), new GradientAlphaKey(c.a, 1f) }
            );
            visual.validColorGradient = gradient;
            visual.invalidColorGradient = gradient;
        }
    }
}
