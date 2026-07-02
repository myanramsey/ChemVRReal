using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class MoleculeScale : MonoBehaviour
{
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;

    [Header("Left Hand Buttons (A = scale up, B = scale down)")]
    public InputActionProperty primaryButton;
    public InputActionProperty secondaryButton;

    [Header("Settings")]
    [Range(0.05f, 0.5f)]
    public float scaleStep = 0.1f;
    public float minScale = 0.1f;
    public float maxScale = 5f;

    [Header("Radial Menu")]
    public RadialMenuController radialMenuController;
    public ModeIndicator modeIndicator;

    private bool scaleMode = false;

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
        if (option.id == "scale_molecule")
        {
            scaleMode = true;
            modeIndicator?.SetMode("Scale Mode");
            Debug.Log("[MoleculeScale] Scale mode ON — point at a molecule and press A (up) or B (down).");
        }
    }

    public void ExitScaleMode()
    {
        scaleMode = false;
        modeIndicator?.ResetToNormal();
        Debug.Log("[MoleculeScale] Scale mode OFF.");
    }

    private void Update()
    {
        if (!scaleMode) return;

        bool up   = primaryButton.action != null && primaryButton.action.WasPressedThisFrame();
        bool down = secondaryButton.action != null && secondaryButton.action.WasPressedThisFrame();

        if (!up && !down) return;
        if (!TryGetRaycastHit(out RaycastHit raycastHit)) return;

        GameObject hit = raycastHit.collider?.gameObject;
        if (hit == null) return;

        GameObject molecule = FindMoleculeRoot(hit);
        if (molecule == null) return;

        float current = molecule.transform.localScale.x;
        float next = Mathf.Clamp(current + (up ? scaleStep : -scaleStep), minScale, maxScale);
        molecule.transform.localScale = Vector3.one * next;
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
            if (t.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() != null)
                return t.gameObject;
            if (t.parent == null) break;
            t = t.parent;
        }
        return null;
    }
}
