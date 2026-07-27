using UnityEngine;
using Unity.XR.CoreUtils;

// Attach to any always-loaded GameObject and assign colorPickerMenu, radialMenuController,
// and xrOrigin in the Inspector. Selecting "Orbital Color" in the radial menu teleports the
// existing Color Picker Menu panel in front of the player. Molecule targeting is unchanged —
// still handled by SpawnColorMenu's existing point-and-click raycast flow.
public class ShowColorPickerMenu : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private GameObject colorPickerMenu;
    public RadialMenuController radialMenuController;

    public float distanceFromPlayer = 1.5f;

    private float height;
    private float xRot, zRot;

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
        if (option.id != "orbital_color") return;

        Transform vrPlayer = xrOrigin.Camera.transform;
        Vector3 targetPos = vrPlayer.position + (vrPlayer.forward * distanceFromPlayer);
        targetPos.y = height;
        colorPickerMenu.transform.position = targetPos;

        Quaternion targetRot = Quaternion.LookRotation(vrPlayer.forward);
        colorPickerMenu.transform.rotation = Quaternion.Euler(xRot, targetRot.eulerAngles.y, zRot);

        colorPickerMenu.SetActive(true);
    }
}
