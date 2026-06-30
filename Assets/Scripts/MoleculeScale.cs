using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Attach to the right-hand XR controller.
// Assign rayInteractor and radialMenuController in the Inspector.
// Point the ray at a molecule, then open the radial menu and select
// Size -> Bigger / Smaller to scale it, or Delete Molecule to remove it.
public class MoleculeScale : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;
    public RadialMenuController radialMenuController;

    [Header("Settings")]
    [Range(0.05f, 1f)]
    public float scaleStep = 0.15f;
    public float minScale = 0.1f;
    public float maxScale = 10f;

    // Molecule locked when the Size submenu is entered.
    private GameObject _lockedMolecule;

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[MoleculeScale] radialMenuController not assigned!"); return; }
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
            case "mol_size":
                // Lock on the currently aimed molecule when entering the size submenu.
                _lockedMolecule = GetAimedMolecule();
                Debug.Log($"[MoleculeScale] Locked: {(_lockedMolecule != null ? _lockedMolecule.name : "none")}");
                break;

            case "size_up":
            case "size_down":
                ApplyScale(option.id == "size_up");
                break;

            case "delete_mol":
                DeleteAimed();
                break;
        }
    }

    void ApplyScale(bool increase)
    {
        GameObject target = _lockedMolecule != null ? _lockedMolecule : GetAimedMolecule();
        if (target == null) { Debug.Log("[MoleculeScale] No molecule targeted."); return; }

        float current = target.transform.localScale.x;
        float next = Mathf.Clamp(current + (increase ? scaleStep : -scaleStep), minScale, maxScale);
        target.transform.localScale = Vector3.one * next;
        Debug.Log($"[MoleculeScale] {target.name} scale -> {next:F2}");
    }

    void DeleteAimed()
    {
        GameObject target = GetAimedMolecule();
        if (target == null) { Debug.Log("[MoleculeScale] No molecule to delete."); return; }
        Debug.Log($"[MoleculeScale] Deleting {target.name}");
        if (_lockedMolecule == target) _lockedMolecule = null;
        Destroy(target);
    }

    GameObject GetAimedMolecule()
    {
        if (rayInteractor == null) return null;

        rayInteractor.TryGetCurrentRaycast(
            out RaycastHit? hit,
            out _,
            out _,
            out _,
            out bool isUI
        );

        if (isUI || !hit.HasValue) return null;
        GameObject obj = hit.Value.collider?.gameObject;
        return obj == null ? null : FindMoleculeRoot(obj);
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
}
