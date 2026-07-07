using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ToggleBackboneOrbital : MonoBehaviour
{
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;

    [Header("Buttons (A = toggle backbone, B = toggle orbitals)")]
    public InputActionProperty primaryButton;
    public InputActionProperty secondaryButton;

    [Header("Radial Menu")]
    public RadialMenuController radialMenuController;
    public ModeIndicator modeIndicator;

    private bool toggleMode = false;
    private readonly Dictionary<GameObject, bool> backboneVisible = new Dictionary<GameObject, bool>();
    private readonly Dictionary<GameObject, bool> orbitalVisible  = new Dictionary<GameObject, bool>();

    void Start()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.AddListener(HandleOption);

        primaryButton.action?.Enable();
        secondaryButton.action?.Enable();
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        if (option.id == "toggle_backbone_orbital")
        {
            toggleMode = true;
            modeIndicator?.SetMode("Toggle Mode");
            Debug.Log("[ToggleBackboneOrbital] Toggle mode ON — A=backbone, B=orbitals.");
        }
    }

    public void ExitToggleMode()
    {
        toggleMode = false;
        modeIndicator?.ResetToNormal();
    }

    void Update()
    {
        if (!toggleMode) return;

        bool pressedA = primaryButton.action != null   && primaryButton.action.WasPressedThisFrame();
        bool pressedB = secondaryButton.action != null && secondaryButton.action.WasPressedThisFrame();

        if (!pressedA && !pressedB) return;
        if (!TryGetRaycastHit(out RaycastHit raycastHit)) return;

        GameObject hit = raycastHit.collider?.gameObject;
        if (hit == null) return;

        GameObject molecule = FindMoleculeRoot(hit);
        if (molecule == null) return;

        // Seed defaults: both visible the first time we see this molecule.
        if (!backboneVisible.ContainsKey(molecule)) backboneVisible[molecule] = true;
        if (!orbitalVisible.ContainsKey(molecule))  orbitalVisible[molecule]  = true;

        if (pressedA)
        {
            backboneVisible[molecule] = !backboneVisible[molecule];
            // Mutual exclusion: if both would be hidden, force orbitals back on.
            if (!backboneVisible[molecule] && !orbitalVisible[molecule])
                orbitalVisible[molecule] = true;
        }
        else // pressedB
        {
            orbitalVisible[molecule] = !orbitalVisible[molecule];
            // Mutual exclusion: if both would be hidden, force backbone back on.
            if (!backboneVisible[molecule] && !orbitalVisible[molecule])
                backboneVisible[molecule] = true;
        }

        ApplyVisibility(molecule);
    }

    private void ApplyVisibility(GameObject molecule)
    {
        Transform backbone = FindChildByExtension(molecule.transform, ".pdb");
        Transform orbital  = FindChildByExtension(molecule.transform, ".cub");

        if (backbone != null) backbone.gameObject.SetActive(backboneVisible[molecule]);
        if (orbital  != null) orbital.gameObject.SetActive(orbitalVisible[molecule]);
    }

    private Transform FindChildByExtension(Transform root, string extension)
    {
        foreach (Transform child in root)
        {
            if (child.name.EndsWith(extension)) return child;
        }
        return null;
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
