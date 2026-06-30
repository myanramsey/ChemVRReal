using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

// Handles all Raycast Settings submenu options:
//   ray_red / ray_blue / ray_green / ray_white  -> change line color
//   ray_alpha_50 / ray_alpha_100                -> change line transparency
public class SetRaycastLineColor : MonoBehaviour
{
    public RadialMenuController radialMenuController;
    public LineRenderer lineRenderer;
    public XRInteractorLineVisual lineVisual;

    private Gradient _cachedGradient = new Gradient();

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[RaycastLine] radialMenuController not assigned!"); return; }
        radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        // Color options
        if (option.id.StartsWith("ray_") && !option.id.StartsWith("ray_alpha_"))
        {
            ApplyColor(option.displayColor);
            Debug.Log($"[RaycastLine] Color -> {option.displayColor} ('{option.id}')");
            return;
        }

        // Alpha options
        float alpha = option.id switch
        {
            "ray_alpha_50"  => 0.50f,
            "ray_alpha_100" => 1.00f,
            _ => -1f
        };

        if (alpha >= 0f)
        {
            ApplyAlpha(alpha);
            Debug.Log($"[RaycastLine] Alpha -> {alpha} ('{option.id}')");
        }
    }

    void ApplyColor(Color c)
    {
        if (lineRenderer != null)
        {
            lineRenderer.startColor = c;
            lineRenderer.endColor   = c;
        }

        if (lineVisual != null)
        {
            _cachedGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(c.a, 0f), new GradientAlphaKey(c.a, 1f) }
            );
            lineVisual.validColorGradient   = _cachedGradient;
            lineVisual.invalidColorGradient = _cachedGradient;
        }
    }

    void ApplyAlpha(float alpha)
    {
        if (lineRenderer != null)
        {
            Color sc = lineRenderer.startColor; sc.a = alpha; lineRenderer.startColor = sc;
            Color ec = lineRenderer.endColor;   ec.a = alpha; lineRenderer.endColor   = ec;
        }

        if (lineVisual != null)
        {
            GradientColorKey[] ck = lineVisual.validColorGradient.colorKeys;
            GradientAlphaKey[] ak = lineVisual.validColorGradient.alphaKeys;
            for (int i = 0; i < ak.Length; i++) ak[i].alpha = alpha;
            _cachedGradient.SetKeys(ck, ak);
            lineVisual.validColorGradient   = _cachedGradient;
            lineVisual.invalidColorGradient = _cachedGradient;
        }
    }
}
