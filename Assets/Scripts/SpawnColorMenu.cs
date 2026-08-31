using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SpawnColorMenu : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    [SerializeField] private InputActionProperty leftTrigger;
    [SerializeField] private InputActionProperty rightTrigger;

    [SerializeField] private GameObject colorPickerMenu;

    [SerializeField] private ShowColorPickerMenu showColorPickerMenu;

    private ColorPickerControl cpc;

    private GameObject molecule;

    private float height;
    private float xRot;
    private float zRot;

    private bool isOpen = false;

    private void Start()
    {
        height = colorPickerMenu.transform.position.y;
        xRot = colorPickerMenu.transform.rotation.eulerAngles.x;
        zRot = colorPickerMenu.transform.rotation.eulerAngles.z;
    }

    private void OnEnable()
    {
        leftTrigger.action.Enable();
        rightTrigger.action.Enable();
    }

    private void OnDisable()
    {
        leftTrigger.action.Disable();
        rightTrigger.action.Disable();
    }

    private void Update()
    {
        if (isOpen) return;
        if (!showColorPickerMenu.colorMode) return;
        if (!leftTrigger.action.WasPressedThisFrame() && !rightTrigger.action.WasPressedThisFrame()) return;

        XRRayInteractor rayInteractor = null;

        if (leftTrigger.action.WasPressedThisFrame())
            rayInteractor = leftRayInteractor;
        else if (rightTrigger.action.WasPressedThisFrame())
            rayInteractor = rightRayInteractor;

        if (rayInteractor == null) return;

        if (!TryGetRaycastHit(rayInteractor, out RaycastHit raycastHit)) return;

        // Walk up hierarchy to find molecule root (mirrors RayPointerLogger logic)
        GameObject hit = raycastHit.collider?.gameObject;
        if (hit == null) return;

        molecule = hit;
        if (hit.transform.parent != null)
        {
            molecule = hit.transform.parent.gameObject;
            if (molecule.transform.parent != null)
                molecule = molecule.transform.parent.gameObject;
        }

        if (molecule.tag != "Molecule") return;
        if (molecule.transform.childCount == 0) return;

        // Highlight selected molecule
        //molecule.GetComponent<Outline>().enabled = true;

        // Spawn color picker menu in front and facing player
        Transform vrPlayer = xrOrigin.Camera.transform;

        Vector3 targetPos = vrPlayer.position + (vrPlayer.forward * 2f);
        targetPos.y = height;
        colorPickerMenu.transform.position = targetPos;

        Quaternion targetRot = Quaternion.LookRotation(vrPlayer.forward);
        colorPickerMenu.transform.rotation = Quaternion.Euler(xRot, targetRot.eulerAngles.y, zRot);

        colorPickerMenu.SetActive(true);

        // Send selected molecule to ColorPickerControl script
        cpc = colorPickerMenu.GetComponent<ColorPickerControl>();
        cpc.SetGameObject(molecule);

        // Update isOpen to true to allow selection of individual parts of the molecule
        isOpen = true;
    }

    public void SetIsOpen(bool open)
    {
        isOpen = open;
    }

    public bool GetIsOpen()
    {
        return isOpen;
    }

    public GameObject GetGameObject()
    {
        return molecule;
    }

    private bool TryGetRaycastHit(XRRayInteractor ray, out RaycastHit hit)
    {
        hit = default;

        if (ray == null)
            return false;

        ray.TryGetCurrentRaycast(
            out RaycastHit? raycastHit,
            out _,
            out _,
            out _,
            out bool isUIHit
        );

        if (isUIHit || !raycastHit.HasValue)
            return false;

        hit = raycastHit.Value;
        return true;
    }
}
