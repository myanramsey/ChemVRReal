using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DeleteMolecule : MonoBehaviour
{
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;

    [Header("Delete Buttons")]
    public InputActionProperty button1;
    public InputActionProperty button2;

    [Header("Radial Menu")]
    public RadialMenuController radialMenuController;
    public ModeIndicator modeIndicator;

    private bool deleteMode = false;

    void Start()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.AddListener(HandleOption);

        button1.action?.Enable();
        button2.action?.Enable();
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

        bool pressedA = (button1.action != null && button1.action.WasPressedThisFrame());
        bool pressedB = (button2.action != null && button2.action.WasPressedThisFrame());

        if (!pressedA && !pressedB) return;

        Debug.Log("[DeleteMolecule] Button pressed in delete mode — checking raycast...");

        if (!TryGetRaycastHit(out RaycastHit raycastHit)) return;

        GameObject hit = raycastHit.collider?.gameObject;
        Debug.Log($"[DeleteMolecule] Ray hit: {hit?.name} | tag: {hit?.tag}");
        if (hit == null) return;

        GameObject molecule = FindMoleculeRoot(hit);
        Debug.Log($"[DeleteMolecule] Molecule root found: {molecule?.name ?? "NULL"}");
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
        hit = default;

        // Check both rays.
        XRRayInteractor[] rays = { rightRayInteractor, leftRayInteractor };

        foreach (XRRayInteractor ray in rays)
        {
            if (ray == null) continue;

            bool hasHit = ray.TryGetCurrentRaycast(
                out RaycastHit? raycastHit,
                out _, out _, out _, out bool isUIHit
            );

            if (!hasHit || isUIHit || !raycastHit.HasValue)
                continue;

            RaycastHit candidateHit = raycastHit.Value;

            GameObject hitObject = candidateHit.collider?.gameObject;
            if (hitObject == null)
                continue;

            // Only accept a raycast that hits a molecule.
            GameObject molecule = FindMoleculeRoot(hitObject);

            if (molecule != null)
            {
                hit = candidateHit;
                return true;
            }
        }

        return false;
    }

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
}
