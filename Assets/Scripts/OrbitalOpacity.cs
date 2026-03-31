using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Attach to the XR controller. Assign rayInteractor, increaseOpacityButton, and
// decreaseOpacityButton in the Inspector. Point the ray at a molecule and press
// either button to step its orbital (.cub surface) opacity up or down.
public class OrbitalOpacity : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;

    [Header("Buttons")]
    public InputActionProperty increaseOpacityButton;
    public InputActionProperty decreaseOpacityButton;

    [Header("Settings")]
    [Range(0.05f, 0.5f)]
    public float opacityStep = 0.1f;

    // Per-molecule opacity tracking
    private Dictionary<GameObject, float> orbitalOpacities = new Dictionary<GameObject, float>();

    private void OnEnable()
    {
        increaseOpacityButton.action.Enable();
        decreaseOpacityButton.action.Enable();
    }

    private void OnDisable()
    {
        increaseOpacityButton.action.Disable();
        decreaseOpacityButton.action.Disable();
    }

    private void Update()
    {
        bool increase = increaseOpacityButton.action.WasPressedThisFrame();
        bool decrease = decreaseOpacityButton.action.WasPressedThisFrame();

        if (!increase && !decrease) return;
        if (rayInteractor == null) return;

        rayInteractor.TryGetCurrentRaycast(
            out RaycastHit? raycastHit,
            out _,
            out _,
            out _,
            out bool isUIHitClosest
        );

        if (isUIHitClosest || !raycastHit.HasValue) return;

        GameObject hit = raycastHit.Value.collider?.gameObject;
        if (hit == null) return;

        // Walk up hierarchy until we find the Molecule-tagged root.
        // FIX: old code always walked exactly 2 levels, causing it to overshoot
        // past the molecule root when atoms were direct children (depth mismatch).
        GameObject molecule = FindMoleculeRoot(hit);
        if (molecule == null) return;

        // Find the child whose name ends with ".cub" — the orbital surface.
        // Order of .cub/.pdb children varies per prefab so we can't use GetChild(0).
        Transform orbitalRoot = FindCubChild(molecule.transform);
        if (orbitalRoot == null) return;

        // Seed opacity from the actual material if we haven't tracked it yet.
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
