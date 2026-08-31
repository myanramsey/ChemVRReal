using UnityEngine;
using Unity.XR.CoreUtils;
using Unity.VisualScripting;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

// Attach to any always-loaded GameObject and assign colorPickerMenu, radialMenuController,
// and xrOrigin in the Inspector. Selecting "Orbital Color" in the radial menu teleports the
// existing Color Picker Menu panel in front of the player. Molecule targeting is unchanged —
// still handled by SpawnColorMenu's existing point-and-click raycast flow.
public class ShowColorPickerMenu : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private GameObject colorPickerMenu;

    [SerializeField] private ColorPickerControl cpc;
    [SerializeField] private SpawnColorMenu scm;
    [SerializeField] private SelectMoleculeParts smp;

    [Header("Radial Menu")]
    public RadialMenuController radialMenuController;
    public ModeIndicator modeIndicator;

    public float distanceFromPlayer = 1.5f;

    private float height;
    private float xRot, zRot;

    [HideInInspector] public bool colorMode = false;

    void Start()
    {
        height = colorPickerMenu.transform.position.y;
        xRot = colorPickerMenu.transform.rotation.eulerAngles.x;
        zRot = colorPickerMenu.transform.rotation.eulerAngles.z;

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
        if (option.id != "molecule_color") return;

        if (option.id == "molecule_color")
        {
            colorMode = true;
            modeIndicator?.SetMode("Color Mode");
        }
    }

    public void ExitColorMode()
    {
        if (colorMode) 
        {
            cpc.Back();
            scm.SetIsOpen(false);
            smp.ResetSelectedMolecule();
        }

        colorMode = false;
        modeIndicator?.ResetToNormal();
    }
}
