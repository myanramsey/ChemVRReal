using UnityEngine;
using UnityEngine.InputSystem;

// Toggles a menu panel.
// - Via button: the assigned menuButton input action
// - Via radial menu: the "mol_select" option id
public class ToggleMenu : MonoBehaviour
{
    [Tooltip("Canvas GameObject that contains the menu.")]
    [SerializeField] private GameObject menu;

    public InputActionProperty menuButton;
    public RadialMenuController radialMenuController;

    private void OnEnable()
    {
        menuButton.action?.Enable();
    }

    private void OnDisable()
    {
        menuButton.action?.Disable();
    }

    private void Start()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.AddListener(HandleRadialOption);
    }

    private void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleRadialOption);
    }

    private void Update()
    {
        if (menuButton.action != null && menuButton.action.WasPressedThisFrame())
            Toggle();
    }

    private void HandleRadialOption(RadialMenuOption option)
    {
        if (option.id == "mol_select")
            Toggle();
    }

    private void Toggle()
    {
        if (menu == null) return;
        menu.SetActive(!menu.activeSelf);
    }
}
