using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class MOSlider : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    [SerializeField] private InputActionProperty leftTrigger;
    [SerializeField] private InputActionProperty rightTrigger;

    [SerializeField] private GameObject moSliderMenu;
    [SerializeField] private Slider slider;

    [SerializeField] private ShowMOSliderMenu showMOSliderMenu;

    private GameObject molecule;
    private GameObject[] molecules;
    private List<GameObject> moleculeList = new List<GameObject>();

    private float height;
    private float xRot;
    private float zRot;

    private bool isOpen = false;

    private float lastSliderValue = -1f;

    private Vector3 pos;
    private Quaternion rot;
    private Vector3 scale;

    private List<Color> orbitalColors = new List<Color>();
    private List<Color> atomColors = new List<Color>();
    private List<Color> atomVertexColors = new List<Color>();

    private void Start()
    {
        height = moSliderMenu.transform.position.y;
        xRot = moSliderMenu.transform.rotation.eulerAngles.x;
        zRot = moSliderMenu.transform.rotation.eulerAngles.z;
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
        SpawnMenu();
        ChangeMO();
    }

    private void SpawnMenu()
    {
        if (!showMOSliderMenu.moMode) return;
        if (isOpen) return;
        if (!leftTrigger.action.WasPressedThisFrame() && !rightTrigger.action.WasPressedThisFrame()) return;

        moleculeList.Clear();

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

        // Get molecule attribute info
        pos = molecule.transform.position;
        rot = molecule.transform.rotation;
        scale = molecule.transform.localScale;
        SaveColors();

        // Spawn menu in front and facing player
        Transform vrPlayer = xrOrigin.Camera.transform;

        Vector3 targetPos = vrPlayer.position + (vrPlayer.forward * 2f);
        targetPos.y = height;
        moSliderMenu.transform.position = targetPos;

        Quaternion targetRot = Quaternion.LookRotation(vrPlayer.forward);
        moSliderMenu.transform.rotation = Quaternion.Euler(xRot, targetRot.eulerAngles.y, zRot);

        // Get molecules of same name
        molecules = Resources.LoadAll<GameObject>("");

        string moleculeName = molecule.name.Split('_')[0];

        foreach (GameObject m in molecules)
        {
            string mName = m.name.Split('_')[0];

            if (moleculeName == mName)
            {
                moleculeList.Add(m);
            }
        }

        // Set slider max length
        slider.maxValue = moleculeList.Count - 1;

        moSliderMenu.SetActive(true);
        isOpen = true;
    }

    private void ChangeMO()
    {
        if (!showMOSliderMenu.moMode) return;
        if (!isOpen) return;
        if (lastSliderValue == slider.value) return;

        lastSliderValue = slider.value;

        GameObject oldMolecule = molecule;

        GameObject newMolecule = Instantiate(moleculeList[(int)slider.value], pos, rot);
        newMolecule.transform.localScale = scale;
        ColorMolecule(newMolecule);
        newMolecule.name = newMolecule.name.Replace("(Clone)", "").Trim();

        Rigidbody rb = newMolecule.GetComponent<Rigidbody>();
        var grab = newMolecule.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
        if (grab != null)
        {
            grab.useDynamicAttach = true;
            grab.throwOnDetach = false;

            grab.selectExited.AddListener((args) => {
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
            });
        }

        Destroy(oldMolecule);
        molecule = newMolecule;
    }

    private void SaveColors()
    {
        // Save the material colors of all parts of the molecule
        orbitalColors.Clear();
        atomColors.Clear();
        atomVertexColors.Clear();

        // Orbitals
        for (int i = 0; i < molecule.transform.GetChild(0).childCount; i++)
        {
            GameObject orbital = molecule.transform.GetChild(0).GetChild(i).gameObject;
            Color orbitalColor = orbital.GetComponent<MeshRenderer>().material.color;
            orbitalColors.Add(orbitalColor);
        }

        // Atoms and Bonds
        for (int i = 0; i < molecule.transform.GetChild(1).childCount; i++)
        {
            GameObject atom = molecule.transform.GetChild(1).GetChild(i).gameObject;
            MeshRenderer mr = atom.GetComponent<MeshRenderer>();
            Color atomColor = new Color();
            if (mr != null)
            {
                atomColor = mr.material.color;
            }
            atomColors.Add(atomColor);

            // Save vertex colors
            MeshFilter meshFilter = atom.GetComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            if (meshFilter != null)
            {
                mesh = meshFilter.mesh;
            }
            if (mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color))
            {
                Color[] colors = mesh.colors;
                Color color = colors[0];
                atomVertexColors.Add(color);
            }
            else
            {
                atomVertexColors.Add(Color.clear);
            }
        }
    }

    private void ColorMolecule(GameObject newMolecule)
    {
        // Orbitals
        for (int i = 0; i < newMolecule.transform.GetChild(0).childCount; i++)
        {
            GameObject orbital = newMolecule.transform.GetChild(0).GetChild(i).gameObject;
            orbital.GetComponent<MeshRenderer>().material.color = orbitalColors[i];
        }

        // Atoms and Bonds
        for (int i = 0; i < newMolecule.transform.GetChild(1).childCount; i++)
        {
            // Reset material color
            GameObject atom = molecule.transform.GetChild(1).GetChild(i).gameObject;
            MeshRenderer mr = atom.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.material.color = atomColors[i];
            }

            // Reset vertex color
            MeshFilter meshFilter = atom.GetComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            if (meshFilter != null)
            {
                mesh = meshFilter.mesh;
            }
            if (mesh.HasVertexAttribute(UnityEngine.Rendering.VertexAttribute.Color))
            {
                Color[] colors = new Color[mesh.colors.Length];
                for (int j = 0; j < colors.Length; j++)
                {
                    colors[j] = atomVertexColors[i];
                }
                mesh.colors = colors;
            }
        }
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

    public void Confirm()
    {
        isOpen = false;
        moSliderMenu.SetActive(false);
    }
}
