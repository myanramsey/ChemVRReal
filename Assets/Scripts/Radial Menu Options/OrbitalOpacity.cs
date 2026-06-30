using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Attach to the XR controller. Assign rayInteractor and radialMenuController.
// Point the ray at a molecule, enter the Orbital Opacity submenu from the
// radial menu, then select More Opaque / More Transparent to adjust the
// .cub orbital surface alpha.
public class OrbitalOpacity : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;
    public RadialMenuController radialMenuController;

    [Header("Settings")]
    [Range(0.05f, 0.5f)]
    public float opacityStep = 0.15f;

    private GameObject _lockedMolecule;
    private readonly Dictionary<GameObject, float> _opacities = new Dictionary<GameObject, float>();

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[OrbitalOpacity] radialMenuController not assigned!"); return; }
        radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        switch (option.id)
        {
            case "orb_opacity":
                _lockedMolecule = GetAimedMolecule();
                Debug.Log($"[OrbitalOpacity] Locked: {(_lockedMolecule != null ? _lockedMolecule.name : "none")}");
                break;

            case "orb_up":
            case "orb_down":
                ApplyOpacityStep(option.id == "orb_up");
                break;
        }
    }

    void ApplyOpacityStep(bool increase)
    {
        GameObject target = _lockedMolecule != null ? _lockedMolecule : GetAimedMolecule();
        if (target == null) { Debug.Log("[OrbitalOpacity] No molecule targeted."); return; }

        Transform cub = FindCubChild(target.transform);
        if (cub == null) { Debug.Log("[OrbitalOpacity] No .cub child found."); return; }

        if (!_opacities.ContainsKey(target))
            _opacities[target] = ReadOpacity(cub);

        float next = Mathf.Clamp01(_opacities[target] + (increase ? opacityStep : -opacityStep));
        _opacities[target] = next;
        ApplyOpacity(cub, next);
        Debug.Log($"[OrbitalOpacity] {target.name} orbital opacity -> {next:F2}");
    }

    GameObject GetAimedMolecule()
    {
        if (rayInteractor == null) return null;
        rayInteractor.TryGetCurrentRaycast(out RaycastHit? hit, out _, out _, out _, out bool isUI);
        if (isUI || !hit.HasValue) return null;
        GameObject obj = hit.Value.collider?.gameObject;
        return obj == null ? null : FindMoleculeRoot(obj);
    }

    Transform FindCubChild(Transform moleculeRoot)
    {
        foreach (Transform child in moleculeRoot)
            if (child.name.EndsWith(".cub")) return child;
        return null;
    }

    GameObject FindMoleculeRoot(GameObject start)
    {
        Transform t = start.transform;
        for (int i = 0; i < 6; i++)
        {
            if (t.GetComponent<XRGrabInteractable>() != null) return t.gameObject;
            if (t.parent == null) break;
            t = t.parent;
        }
        return null;
    }

    float ReadOpacity(Transform orbitalRoot)
    {
        Renderer[] renderers = orbitalRoot.GetComponentsInChildren<Renderer>();
        return renderers.Length > 0 ? renderers[0].material.color.a : 1f;
    }

    void ApplyOpacity(Transform orbitalRoot, float alpha)
    {
        foreach (Renderer r in orbitalRoot.GetComponentsInChildren<Renderer>())
        {
            Material mat = r.material;
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;

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
                mat.SetFloat("_Mode", 2);
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
