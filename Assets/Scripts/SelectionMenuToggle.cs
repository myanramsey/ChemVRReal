using UnityEngine;

// Listens to the radial menu for the "mol_select" option and toggles
// the molecule spawn/selection panel on or off.
public class SelectionMenuToggle : MonoBehaviour
{
    public RadialMenuController radialMenuController;

    [Tooltip("The molecule spawn panel Canvas GameObject.")]
    public GameObject selectionMenuPanel;

    void Start()
    {
        if (radialMenuController == null) { Debug.LogError("[SelectionMenuToggle] radialMenuController not assigned!"); return; }
        radialMenuController.onOptionConfirmed.AddListener(HandleOption);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOption);
    }

    void HandleOption(RadialMenuOption option)
    {
        if (option.id != "mol_select") return;
        if (selectionMenuPanel == null) { Debug.LogError("[SelectionMenuToggle] selectionMenuPanel not assigned!"); return; }

        bool next = !selectionMenuPanel.activeSelf;
        selectionMenuPanel.SetActive(next);
        Debug.Log($"[SelectionMenuToggle] Molecule selection panel -> {(next ? "open" : "closed")}");
    }
}
