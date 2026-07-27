using UnityEngine;
using Unity.XR.CoreUtils;

// Attach to any always-loaded GameObject and assign moleculeSpawnMenu, radialMenuController,
// and xrOrigin in the Inspector. Selecting "Add Molecule" in the radial menu teleports the
// existing Molecule Spawn Menu panel in front of the player, mirroring the snap-to-player
// logic already used by SpawnColorMenu for the color picker panel.
public class ShowMoleculeSpawnMenu : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private GameObject moleculeSpawnMenu;
    public RadialMenuController radialMenuController;

    public float distanceFromPlayer = 1.5f;

    private float height;
    private float xRot, zRot;

    void Start()
    {
        height = moleculeSpawnMenu.transform.position.y;
        xRot = moleculeSpawnMenu.transform.rotation.eulerAngles.x;
        zRot = moleculeSpawnMenu.transform.rotation.eulerAngles.z;

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
        if (option.id != "add_molecule") return;

        Transform vrPlayer = xrOrigin.Camera.transform;
        Vector3 targetPos = vrPlayer.position + (vrPlayer.forward * distanceFromPlayer);
        targetPos.y = height;
        moleculeSpawnMenu.transform.position = targetPos;

        Quaternion targetRot = Quaternion.LookRotation(vrPlayer.forward);
        moleculeSpawnMenu.transform.rotation = Quaternion.Euler(xRot, targetRot.eulerAngles.y, zRot);

        moleculeSpawnMenu.SetActive(true);
    }
}
