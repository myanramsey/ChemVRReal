using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Attach to the XR controller. Assign rayInteractor, increaseOpacityButton, and
// decreaseOpacityButton in the Inspector. Point the ray at a molecule and press
// either button to step its orbital (.cub surface) opacity up or down.
public class OrbitalOpacity : MonoBehaviour
{
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;

    [Header("Buttons")]
    public InputActionProperty increaseOpacityButton;
    public InputActionProperty decreaseOpacityButton;

    [Header("Settings")]
    [Range(0.05f, 0.5f)]
    public float opacityStep = 0.1f;

    [Header("Radial Menu")]
    public RadialMenuController radialMenuController;
    public ModeIndicator modeIndicator;

    private bool opacityMode = false;
    private Dictionary<GameObject, float> orbitalOpacities = new Dictionary<GameObject, float>();

    void Start()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.AddListener(HandleOption);

        increaseOpacityButton.action?.Enable();
        decreaseOpacityButton.action?.Enable();
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        if (option.id == "opacity_molecule")
        {
            opacityMode = true;
            modeIndicator?.SetMode("Opacity Mode");
            Debug.Log("[OrbitalOpacity] Opacity mode ON — point at a molecule and press A (increase) or B (decrease).");
        }
    }

    public void ExitOpacityMode()
    {
        opacityMode = false;
        modeIndicator?.ResetToNormal();
    }

    private void Update()
    {
        if (!opacityMode) return;

        bool increase = increaseOpacityButton.action != null && increaseOpacityButton.action.WasPressedThisFrame();
        bool decrease = decreaseOpacityButton.action != null && decreaseOpacityButton.action.WasPressedThisFrame();

        if (!increase && !decrease) return;

        AdjustOpacity(increase);
    }

    // Called by Update (button press) or RaycastAlphaMenuOption (radial menu).
    public void AdjustOpacity(bool increase)
    {
        if (!TryGetRaycastHit(out RaycastHit raycastHit)) return;

        GameObject hit = raycastHit.collider?.gameObject;
        if (hit == null) return;

        GameObject molecule = FindMoleculeRoot(hit);
        if (molecule == null) return;

        Transform orbitalRoot = FindCubChild(molecule.transform);
        if (orbitalRoot == null) return;

        if (!orbitalOpacities.ContainsKey(molecule))
            orbitalOpacities[molecule] = ReadOpacity(orbitalRoot);

        float next = Mathf.Clamp01(orbitalOpacities[molecule] + (increase ? opacityStep : -opacityStep));
        orbitalOpacities[molecule] = next;

        ApplyOpacity(orbitalRoot, next);
    }

    // Find the direct child of the molecule root whose name ends with ".cub".
    private Transform FindCubChild(Transform moleculeRoot)
    {
        foreach (Transform child in moleculeRoot)
        {
            if (child.name.EndsWith(".cub")) return child;
        }
        return null;
    }

    private bool TryGetRaycastHit(out RaycastHit hit)
    {
        foreach (XRRayInteractor ray in new[] { leftRayInteractor, rightRayInteractor })
        {
            if (ray == null) continue;
            ray.TryGetCurrentRaycast(out RaycastHit? raycastHit, out _, out _, out _, out bool isUIHit);
            if (!isUIHit && raycastHit.HasValue)
            {
                hit = raycastHit.Value;
                return true;
            }
        }
        hit = default;
        return false;
    }

    // Walk up the transform hierarchy looking for the first ancestor (or self)
    // that has an XRGrabInteractable — that's the molecule root.
    private GameObject FindMoleculeRoot(GameObject start)
    {
        Transform t = start.transform;
        for (int i = 0; i < 5; i++)
        {
            if (t.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() != null)
                return t.gameObject;
            if (t.parent == null) break;
            t = t.parent;
        }
        return null;
    }

    // Read current alpha from the first renderer found under the orbital root.
    private float ReadOpacity(Transform orbitalRoot)
    {
        Renderer[] renderers = orbitalRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
            return renderers[0].material.color.a;
        return 1f;
    }

    // Set alpha on every renderer under the orbital root.
    // Uses renderer.material (instanced) so shared materials on other molecules
    // are not affected.
    private void ApplyOpacity(Transform orbitalRoot, float alpha)
    {
        Renderer[] renderers = orbitalRoot.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            // renderer.material auto-creates an instance for this object only
            Material mat = r.material;
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;

            // Keep Standard shader render mode consistent with current alpha.
            // If fully opaque, restore Opaque mode; otherwise stay in Fade mode.
            if (alpha >= 1f)
            {
                mat.SetFloat("_Mode", 0);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
            }
            else
            {
                mat.SetFloat("_Mode", 2); // Fade
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }
    }
}
