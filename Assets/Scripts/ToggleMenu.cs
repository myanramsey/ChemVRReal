using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class ToggleMenu : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [Tooltip("Canvas GameObject that contains menu.")]
    [SerializeField] private GameObject menu;
    [SerializeField] InputActionProperty menuButton;

    private ContinuousMoveProvider movement;
    private ContinuousTurnProvider turning;

    private float height;
    private float xRot;
    private float zRot;

    private void Start()
    {
        movement = FindAnyObjectByType<ContinuousMoveProvider>();
        turning = FindAnyObjectByType<ContinuousTurnProvider>();

        height = menu.transform.position.y;
        xRot = menu.transform.rotation.eulerAngles.x;
        zRot = menu.transform.rotation.eulerAngles.z;
    }

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
            // Set the menu inactive
            menu.SetActive(false);

            // Enable player movement
            movement.enabled = true;
            turning.enabled = true;
        }
        else
        {
            // Spawn color picker menu in front and facing player
            Transform vrPlayer = xrOrigin.Camera.transform;

            Vector3 targetPos = vrPlayer.position + (vrPlayer.forward * 2f);
            targetPos.y = height;
            menu.transform.position = targetPos;

            Quaternion targetRot = Quaternion.LookRotation(vrPlayer.forward);
            menu.transform.rotation = Quaternion.Euler(xRot, targetRot.eulerAngles.y, zRot);

            // Disable player movement
            movement.enabled = false;
            turning.enabled = false;

            // Set the menu active
            menu.SetActive(true);
        }
    }
}
