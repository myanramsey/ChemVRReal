using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SpawnColorMenu : MonoBehaviour
{
    [SerializeField] private Transform vrPlayer;
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private InputActionProperty primaryButton;

    [SerializeField] private GameObject colorPickerMenu;

    private ColorPickerControl cpc;

    private float height;

    private bool isOpen = false;

    private void Start()
    {
        height = colorPickerMenu.transform.position.y;
    }

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
        if (isOpen) return;
        if (!primaryButton.action.WasPressedThisFrame()) return;
        if (rayInteractor == null) return;

        rayInteractor.TryGetCurrentRaycast(
            out RaycastHit? raycastHit,
            out _,
            out _,
            out _,
            out bool isUIHitClosest
        );

        if (isUIHitClosest || !raycastHit.HasValue) return;

        // Walk up hierarchy to find molecule root (mirrors RayPointerLogger logic)
        GameObject hit = raycastHit.Value.collider?.gameObject;
        if (hit == null) return;

        GameObject molecule = hit;
        if (hit.transform.parent != null)
        {
            molecule = hit.transform.parent.gameObject;
            if (molecule.transform.parent != null)
                molecule = molecule.transform.parent.gameObject;
        }

        if (molecule.tag != "Molecule") return;
        if (molecule.transform.childCount == 0) return;

        // Highlight selected molecule
        molecule.GetComponent<Outline>().enabled = true;

        // Spawn color picker menu in front and facing player
        Vector3 targetPos = vrPlayer.position + (vrPlayer.forward * 2f);
        targetPos.y = height;
        colorPickerMenu.transform.position = targetPos;
        colorPickerMenu.transform.rotation = Quaternion.LookRotation(vrPlayer.forward);
        colorPickerMenu.SetActive(true);

        // Send which GameObject's color/opacity will be changed to ColorPickerControl script
        cpc = colorPickerMenu.GetComponent<ColorPickerControl>();
        cpc.SetGameObject(molecule);
        isOpen = true;
    }

    public void SetIsOpen(bool open)
    {
        isOpen = open;
    }
}
