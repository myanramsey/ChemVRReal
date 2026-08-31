using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SelectMoleculeParts : MonoBehaviour
{
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    [SerializeField] private InputActionProperty leftTrigger;
    [SerializeField] private InputActionProperty rightTrigger;

    [SerializeField] private GameObject colorPickerMenu;

    private SpawnColorMenu scm;
    private ColorPickerControl cpc;

    private MeshRenderer mr = null;
    private MeshRenderer mr2 = null;

    private GameObject lastSelectedPart, lastSelectedPart2, currentMolecule, lastMolecule;

    private void Start()
    {
        scm = FindAnyObjectByType<SpawnColorMenu>();
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
        if (!scm.GetIsOpen()) return;
        if (!leftTrigger.action.WasPressedThisFrame() && !rightTrigger.action.WasPressedThisFrame()) return;

        XRRayInteractor rayInteractor = null;

        if (leftTrigger.action.WasPressedThisFrame())
            rayInteractor = leftRayInteractor;
        else if (rightTrigger.action.WasPressedThisFrame())
            rayInteractor = rightRayInteractor;

        if (rayInteractor == null) return;

        if (!TryGetRaycastHit(rayInteractor, out RaycastHit raycastHit)) return;

        // Find part of molecule that was hit by raycast
        GameObject hit = raycastHit.collider?.gameObject;
        if (hit == null) return;

        if (hit.transform.parent != null)
        {
            currentMolecule = hit.transform.parent.gameObject;
            if (currentMolecule.transform.parent != null)
            {
                currentMolecule = currentMolecule.transform.parent.gameObject;
            } 
        }
        if (currentMolecule != lastMolecule && lastMolecule != null) return;

        GameObject moleculePart = hit;
        GameObject moleculePart2 = null;
        string moleculePartName = moleculePart.name;
        int index = moleculePartName.IndexOf(" ");
        if (index == -1) return;
        moleculePartName = moleculePartName.Substring(0, index);
        
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
        //scm.GetGameObject().GetComponent<Outline>().enabled = false;

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
        lastSelectedPart2 = moleculePart2;
        lastMolecule = scm.GetGameObject();

        // Send the MeshRenderer of the part of molecule that will be changed to ColorPickerControl script
        cpc = colorPickerMenu.GetComponent<ColorPickerControl>();
        cpc.SetMeshRenderer(mr, mr2);

        // Reset MeshRenderers
        mr = null;
        mr2 = null;
    }

    public void ResetSelectedMolecule()
    {
        currentMolecule = null;
        lastMolecule = null;
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
