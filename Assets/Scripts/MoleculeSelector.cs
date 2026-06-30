using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Attach to the XR controller. Continuously tracks the molecule the ray
// is pointing at so other menu-driven scripts can act on it without
// each one needing its own raycast loop.
public class MoleculeSelector : MonoBehaviour
{
    [SerializeField] private XRRayInteractor rayInteractor;

    public static GameObject CurrentMolecule { get; private set; }

    void Update()
    {
        if (rayInteractor == null) return;

        rayInteractor.TryGetCurrentRaycast(
            out RaycastHit? hit,
            out _,
            out _,
            out _,
            out bool isUI
        );

        if (isUI || !hit.HasValue)
        {
            CurrentMolecule = null;
            return;
        }

        GameObject obj = hit.Value.collider?.gameObject;
        CurrentMolecule = obj == null ? null : FindMoleculeRoot(obj);
    }

    public static GameObject FindMoleculeRoot(GameObject start)
    {
        Transform t = start.transform;
        for (int i = 0; i < 6; i++)
        {
            if (t.GetComponent<XRGrabInteractable>() != null)
                return t.gameObject;
            if (t.parent == null) break;
            t = t.parent;
        }
        return null;
    }
}
