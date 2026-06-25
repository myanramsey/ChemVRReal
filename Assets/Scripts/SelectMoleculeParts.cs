using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SelectMoleculeParts : MonoBehaviour
{
    [SerializeField] private Transform vrPlayer;
    [SerializeField] private XRRayInteractor rayInteractor;
    [SerializeField] private InputActionProperty primaryButton;

    [SerializeField] private GameObject colorPickerMenu;

    private SpawnColorMenu scm;
    private ColorPickerControl cpc;

    private MeshRenderer mr = null;
    private MeshRenderer mr2 = null;

    private GameObject lastSelectedPart, lastSelectedPart2;

    private void Start()
    {
        scm = FindAnyObjectByType<SpawnColorMenu>();
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
        if (!scm.GetIsOpen()) return;
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

        // Find part of molecule that was hit by raycast
        GameObject hit = raycastHit.Value.collider?.gameObject;
        if (hit == null) return;

        GameObject moleculePart = hit;
        GameObject moleculePart2 = null;
        string moleculePartName = moleculePart.name;
        int index = moleculePartName.IndexOf(" ");
        moleculePartName = moleculePartName.Substring(0, index);
        if (index == -1) return;
        
        if (moleculePartName == "Orbital")
        {
            mr = moleculePart.GetComponent<MeshRenderer>();
        }
        else
        {
            // If hit atom or bond, get the meshrenderers for both to change their colors/opacity together
            if (hit.transform.parent == null) return;
           
            GameObject moleculePartParent = hit.transform.parent.gameObject;
            
            for (int i = 0; i < moleculePartParent.transform.childCount; i++)
            {
                GameObject child = moleculePartParent.transform.GetChild(i).gameObject;
                string childName = child.name;
                index = childName.IndexOf(" ");
                childName = childName.Substring(0, index);

                if ((childName == moleculePartName) && (mr == null))
                {
                    mr = child.GetComponent<MeshRenderer>();
                    moleculePart = child;
                }
                else if (childName == moleculePartName)
                {
                    mr2 = child.GetComponent<MeshRenderer>();
                    moleculePart2 = child;
                }
            }
        }

        // Remove highlight around the entire molecule
        scm.GetGameObject().GetComponent<Outline>().enabled = false;

        // Remove highlight from previously selected part
        if (lastSelectedPart != moleculePart && lastSelectedPart != null)
        {
            lastSelectedPart.GetComponent<Outline>().enabled = false;
        }
        if (lastSelectedPart2 != moleculePart2 && lastSelectedPart2 != null)
        {
            lastSelectedPart2.GetComponent<Outline>().enabled = false;
        }

        // Highlight selected molecule part
        mr.gameObject.GetComponent<Outline>().enabled = true;
        if (mr2 != null)
        {
            mr2.gameObject.GetComponent<Outline>().enabled = true;
        }

        lastSelectedPart = moleculePart;

        // Send the MeshRenderer of the part of molecule that will be changed to ColorPickerControl script
        cpc = colorPickerMenu.GetComponent<ColorPickerControl>();
        cpc.SetMeshRenderer(mr, mr2);

        // Reset MeshRenderers
        mr = null;
        mr2 = null;
    }
}
