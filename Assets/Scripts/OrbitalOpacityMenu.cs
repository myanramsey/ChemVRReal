using System.Collections.Generic;
using UnityEngine;

// Listens to the radial menu for orbital transparency submenu options and
// adjusts the .cub orbital surface opacity on the locked molecule.
public class OrbitalOpacityMenu : MonoBehaviour
{
    public RadialMenuController radialMenuController;

    [Range(0.05f, 0.5f)]
    public float opacityStep = 0.15f;

    private GameObject _lockedMolecule;
    private readonly Dictionary<GameObject, float> _opacities = new Dictionary<GameObject, float>();

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[OrbitalOpacityMenu] radialMenuController not assigned!"); return; }
        radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        if (option.id == "orb_opacity")
        {
            _lockedMolecule = MoleculeSelector.CurrentMolecule;
            Debug.Log($"[OrbitalOpacityMenu] Locked onto {(_lockedMolecule != null ? _lockedMolecule.name : "nothing")}");
            return;
        }

        if (option.id != "orb_up" && option.id != "orb_down") return;

        GameObject target = _lockedMolecule != null ? _lockedMolecule : MoleculeSelector.CurrentMolecule;
        if (target == null) { Debug.Log("[OrbitalOpacityMenu] No molecule targeted."); return; }

        Transform cub = FindCubChild(target.transform);
        if (cub == null) { Debug.Log("[OrbitalOpacityMenu] No .cub child found."); return; }

        if (!_opacities.ContainsKey(target))
            _opacities[target] = ReadOpacity(cub);

        float next = Mathf.Clamp01(_opacities[target] + (option.id == "orb_up" ? opacityStep : -opacityStep));
        _opacities[target] = next;
        ApplyOpacity(cub, next);
        Debug.Log($"[OrbitalOpacityMenu] {target.name} orbital opacity -> {next:F2}");
    }

    private Transform FindCubChild(Transform root)
    {
        foreach (Transform child in root)
            if (child.name.EndsWith(".cub")) return child;
        return null;
    }

    private float ReadOpacity(Transform orbitalRoot)
    {
        Renderer[] renderers = orbitalRoot.GetComponentsInChildren<Renderer>();
        return renderers.Length > 0 ? renderers[0].material.color.a : 1f;
    }

    private void ApplyOpacity(Transform orbitalRoot, float alpha)
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
