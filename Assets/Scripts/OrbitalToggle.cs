using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class OrbitalToggle : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;
    public InputActionProperty primaryButton;

    private Dictionary<GameObject, bool> orbitalStates = new Dictionary<GameObject, bool>();

    private void OnEnable()
    {
        primaryButton.action.Enable();
    }

    private void OnDisable()
    {
        primaryButton.action.Disable();
    }

    private void Update()
    {
        if (!primaryButton.action.WasPressedThisFrame()) return;
        if (rayInteractor == null) return;

        rayInteractor.TryGetCurrentRaycast(
            out RaycastHit? raycastHit,
            out _,
            out _,
            out _,
            out bool isUIHitClosest
        );

        if (isUIHitClosest || !raycastHit.HasValue) return;

        // Walk up hierarchy to find molecule root (mirrors RayPointerLogger logic)
        GameObject hit = raycastHit.Value.collider?.gameObject;
        if (hit == null) return;

        GameObject molecule = hit;
        if (hit.transform.parent != null)
        {
            molecule = hit.transform.parent.gameObject;
            if (molecule.transform.parent != null)
                molecule = molecule.transform.parent.gameObject;
        }

        if (molecule.tag != "Molecule") return;
        if (molecule.transform.childCount == 0) return;

        // Toggle orbital (child 0 = .cub orbital surface)
        if (!orbitalStates.ContainsKey(molecule))
            orbitalStates[molecule] = true;

        orbitalStates[molecule] = !orbitalStates[molecule];
        molecule.transform.GetChild(0).gameObject.SetActive(orbitalStates[molecule]);
    }
}
