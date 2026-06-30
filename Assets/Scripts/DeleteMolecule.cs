using UnityEngine;

// Listens to the radial menu for the "delete_mol" option and destroys
// whichever molecule MoleculeSelector is currently pointing at.
public class DeleteMolecule : MonoBehaviour
{
    public RadialMenuController radialMenuController;

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[DeleteMolecule] radialMenuController not assigned!"); return; }
        radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        if (option.id != "delete_mol") return;

        GameObject target = MoleculeSelector.CurrentMolecule;
        if (target == null)
        {
            Debug.Log("[DeleteMolecule] No molecule selected.");
            return;
        }

        Debug.Log($"[DeleteMolecule] Destroying {target.name}");
        Destroy(target);
    }
}
