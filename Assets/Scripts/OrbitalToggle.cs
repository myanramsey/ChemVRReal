using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitalToggle : MonoBehaviour
{
    public InputActionProperty primaryButton;

    private bool orbitalsVisible = true;

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
        if (primaryButton.action.WasPressedThisFrame())
        {
            orbitalsVisible = !orbitalsVisible;

            foreach (GameObject mol in GameObject.FindGameObjectsWithTag("Molecule"))
            {
                if (mol.transform.childCount > 0)
                    mol.transform.GetChild(0).gameObject.SetActive(orbitalsVisible);
            }
        }
    }
}