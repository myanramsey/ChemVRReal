using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.EventSystems;
using TMPro; // Remove this if you're not using TextMeshPro

public class RayPointerLogger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XRRayInteractor rayInteractor;

    [Header("Optional UI")]
    [Tooltip("Drag a TextMeshPro UI Text here to display the target on screen")]
    [SerializeField] private TextMeshProUGUI targetLabel;

    private string _lastHitName = "";

    void Update()
    {
        if (rayInteractor == null) return;

        bool hitSomething = rayInteractor.TryGetCurrentRaycast(
            out RaycastHit? raycastHit,
            out _,
            out RaycastResult? uiRaycastResult,
            out _,
            out bool isUIHitClosest
        );

        if (hitSomething)
        {
            string hitName = ResolveHitName(raycastHit, uiRaycastResult, isUIHitClosest);

            // Only log when the target changes (avoids console spam)
            if (hitName != _lastHitName)
            {
                _lastHitName = hitName;
                Debug.Log($"[Ray Pointer] Now pointing at: <b>{hitName}</b>");
            }

            // Update UI label every frame
            if (targetLabel != null)
            {
                // Update with molecule being pointed at
                string moleculeName = "";
                GameObject molecule = null;
                GameObject child = raycastHit?.collider?.gameObject;
                molecule = child;
                if (child != null)
                {
                    if (child.transform.parent != null)
                    {
                        GameObject parent1 = child.transform.parent.gameObject;
                        molecule = parent1;
                        if (parent1.transform.parent != null)
                        {
                            GameObject parent2 = parent1.transform.parent.gameObject;
                            molecule = parent2;
                            moleculeName = parent2.name;
                        }
                        else
                        {
                            moleculeName = parent1.name;
                        }
                    }
                    else
                    {
                        moleculeName = child.name;
                    }

                    // Only displays name of object if object is a molecule
                    if (molecule.tag == "Molecule")
                    {
                        targetLabel.text = $"{moleculeName}";
                    }
                    else
                    {
                        targetLabel.text = $"";
                    }
                }
            }
        }
        else
        {
            if (_lastHitName != "")
            {
                _lastHitName = "";
                Debug.Log("[Ray Pointer] Not pointing at anything.");
            }

            if (targetLabel != null)
                targetLabel.text = "";
        }
    }

    private string ResolveHitName(
        RaycastHit? hit3D,
        RaycastResult? hitUI,
        bool isUIClosest)
    {
        if (isUIClosest && hitUI.HasValue && hitUI.Value.gameObject != null)
            return $"{hitUI.Value.gameObject.name} (UI)";

        if (hit3D.HasValue && hit3D.Value.collider != null)
            return hit3D.Value.collider.gameObject.name;

        return "Unknown";
    }
}