using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

// Handles raycast transparency options from the radial menu.
// Option ids: "ray_alpha_25", "ray_alpha_50", "ray_alpha_75", "ray_alpha_100"
public class RaycastAlpha : MonoBehaviour
{
    public RadialMenuController radialMenuController;
    public LineRenderer lineRenderer;
    public XRInteractorLineVisual lineVisual;

    private Gradient _cachedGradient = new Gradient();

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[RaycastAlpha] radialMenuController not assigned!"); return; }
        radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        float alpha = option.id switch
        {
            "ray_alpha_25"  => 0.25f,
            "ray_alpha_50"  => 0.50f,
            "ray_alpha_75"  => 0.75f,
            "ray_alpha_100" => 1.00f,
            _ => -1f
        };

        if (alpha < 0f) return;
        ApplyAlpha(alpha);
        Debug.Log($"[RaycastAlpha] Alpha set to {alpha}");
    }

    void ApplyAlpha(float alpha)
    {
        if (lineRenderer != null)
        {
            Color sc = lineRenderer.startColor; sc.a = alpha; lineRenderer.startColor = sc;
            Color ec = lineRenderer.endColor;   ec.a = alpha; lineRenderer.endColor = ec;
        }

        if (lineVisual != null)
        {
            UpdateGradientAlpha(lineVisual.validColorGradient,   alpha);
            UpdateGradientAlpha(lineVisual.invalidColorGradient, alpha);
        }
    }

    void UpdateGradientAlpha(Gradient g, float alpha)
    {
        GradientColorKey[]  ck = g.colorKeys;
        GradientAlphaKey[]  ak = g.alphaKeys;
        for (int i = 0; i < ak.Length; i++) ak[i].alpha = alpha;

        _cachedGradient.SetKeys(ck, ak);

        // Write back to whichever gradient we're updating.
        if (lineVisual != null)
        {
            lineVisual.validColorGradient   = _cachedGradient;
            lineVisual.invalidColorGradient = _cachedGradient;
        }
    }
}
