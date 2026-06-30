using System.Collections.Generic;
using UnityEngine;

// Listens to the radial menu for size submenu options and scales the
// molecule that MoleculeSelector was pointing at when the menu was opened.
// Locks onto the target when the size submenu is entered so the user can
// apply multiple steps without re-aiming.
public class MoleculeScaleMenu : MonoBehaviour
{
    public RadialMenuController radialMenuController;

    [Range(0.05f, 1f)]
    public float scaleStep = 0.15f;
    public float minScale = 0.1f;
    public float maxScale = 10f;

    // Locked-in molecule while the size submenu is open.
    private GameObject _lockedMolecule;

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[MoleculeScaleMenu] radialMenuController not assigned!"); return; }
        radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        // When entering the size submenu, lock the current molecule.
        if (option.id == "mol_size")
        {
            _lockedMolecule = MoleculeSelector.CurrentMolecule;
            Debug.Log($"[MoleculeScaleMenu] Locked onto {(_lockedMolecule != null ? _lockedMolecule.name : "nothing")}");
            return;
        }

        if (option.id != "size_up" && option.id != "size_down") return;

        GameObject target = _lockedMolecule != null ? _lockedMolecule : MoleculeSelector.CurrentMolecule;
        if (target == null) { Debug.Log("[MoleculeScaleMenu] No molecule targeted."); return; }

        float current = target.transform.localScale.x;
        float next = Mathf.Clamp(current + (option.id == "size_up" ? scaleStep : -scaleStep), minScale, maxScale);
        target.transform.localScale = Vector3.one * next;
        Debug.Log($"[MoleculeScaleMenu] {target.name} scale -> {next:F2}");
    }
}
