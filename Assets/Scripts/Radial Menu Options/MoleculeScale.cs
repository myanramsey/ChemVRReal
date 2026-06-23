using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Attach to the right-hand XR controller. Assign rayInteractor, scaleUpButton,
// and scaleDownButton in the Inspector. Point the ray at a molecule and press
// either button to scale it up or down.
public class MoleculeScale : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;

    [Header("Buttons")]
    public InputActionProperty scaleUpButton;
    public InputActionProperty scaleDownButton;

    [Header("Settings")]
    [Range(0.05f, 0.5f)]
    public float scaleStep = 0.1f;
    public float minScale = 0.1f;
    public float maxScale = 5f;

    private void OnEnable()
    {
        scaleUpButton.action.Enable();
        scaleDownButton.action.Enable();
    }

    private void OnDisable()
    {
        scaleUpButton.action.Disable();
        scaleDownButton.action.Disable();
    }

    private void Update()
    {
        bool up   = scaleUpButton.action.WasPressedThisFrame();
        bool down = scaleDownButton.action.WasPressedThisFrame();

        if (!up && !down) return;
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

        GameObject molecule = FindMoleculeRoot(hit);
        if (molecule == null) return;

        float current = molecule.transform.localScale.x;
        float next = Mathf.Clamp(current + (up ? scaleStep : -scaleStep), minScale, maxScale);
        molecule.transform.localScale = Vector3.one * next;
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
}
