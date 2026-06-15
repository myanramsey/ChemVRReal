using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ToggleMenu : MonoBehaviour
{
    [Tooltip("Canvas GameObject that contains menu.")]
    [SerializeField] private GameObject menu;

    public InputActionProperty menuButton;

    private void OnEnable()
    {
        menuButton.action.Enable();
    }

    private void OnDisable()
    {
        menuButton.action.Disable();
    }

    private void Update()
    {
        if (!menuButton.action.WasPressedThisFrame()) return;

        if (menu.activeSelf)
        {
            menu.SetActive(false);
        }
        else
        {
            menu.SetActive(true);
        }
    }
}
