using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DeleteMolecule : MonoBehaviour
{
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;

    [Header("Delete Buttons (Left Hand A or B)")]
    public InputActionProperty primaryButton;
    public InputActionProperty secondaryButton;

    [Header("Radial Menu")]
    public RadialMenuController radialMenuController;
    public ModeIndicator modeIndicator;

    private bool deleteMode = false;

    void Start()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        if (option.id == "delete_molecule")
        {
            deleteMode = true;
            Debug.Log("[DeleteMolecule] Delete mode ON — point at a molecule and press A or B to delete.");
        }
    }

    private void Update()
    {
        // Exit delete mode when the menu closes (menu closed = not open and deleteMode still set).
        // We detect this by checking if either confirm button was pressed outside of delete mode entry.
        if (!deleteMode) return;

        bool pressed = (primaryButton.action != null && primaryButton.action.WasPressedThisFrame())
                    || (secondaryButton.action != null && secondaryButton.action.WasPressedThisFrame());

        if (!pressed) return;

        // Try whichever ray interactor has an active hit.
        if (!TryGetRaycastHit(out RaycastHit raycastHit)) return;

        GameObject hit = raycastHit.collider?.gameObject;
        if (hit == null) return;

        GameObject molecule = FindMoleculeRoot(hit);
        if (molecule == null) return;

        Destroy(molecule);
        Debug.Log($"[DeleteMolecule] Deleted: {molecule.name}");

        // Stay in delete mode so the player can delete more molecules.
        // They exit delete mode by opening/closing the radial menu again.
    }

    // Exit delete mode when the radial menu is opened again (re-selecting anything resets state).
    public void ExitDeleteMode()
    {
        deleteMode = false;
        modeIndicator?.ResetToNormal();
        Debug.Log("[DeleteMolecule] Delete mode OFF.");
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

    private GameObject FindMoleculeRoot(GameObject start)
    {
        Transform t = start.transform;
        for (int i = 0; i < 5; i++)
        {
            if (t.CompareTag("Molecule")) return t.gameObject;
            if (t.parent == null) break;
            t = t.parent;
        }
        return null;
    }
}
