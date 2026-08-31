using UnityEngine;
using UnityEngine.UI;

public class Toggles : MonoBehaviour
{
    [SerializeField] private Toggle mirrorPlaneToggle;
    [SerializeField] private Toggle axisToggle;
    [SerializeField] private Toggle labelToggle;

    public void ToggleMirrorPlanes() 
    {
        if (mirrorPlaneToggle.isOn)
        {
            GameObject[] molecules = GameObject.FindGameObjectsWithTag("Molecule");

            foreach (GameObject molecule in molecules)
            {
                molecule.transform.GetChild(2).gameObject.SetActive(true);
            }
        }
        else if (!mirrorPlaneToggle.isOn)
        {
            GameObject[] molecules = GameObject.FindGameObjectsWithTag("Molecule");

            foreach (GameObject molecule in molecules)
            {
                molecule.transform.GetChild(2).gameObject.SetActive(false);
            }
        }
    }

    public void ToggleAxes()
    {
        if (axisToggle.isOn)
        {
            GameObject[] molecules = GameObject.FindGameObjectsWithTag("Molecule");

            foreach (GameObject molecule in molecules)
            {
                molecule.transform.GetChild(3).gameObject.SetActive(true);
            }
        }
        else if (!axisToggle.isOn)
        {
            GameObject[] molecules = GameObject.FindGameObjectsWithTag("Molecule");

            foreach (GameObject molecule in molecules)
            {
                molecule.transform.GetChild(3).gameObject.SetActive(false);
            }
        }
    }

    public void ToggleLabels()
    {
        if (labelToggle.isOn)
        {
            GameObject[] molecules = GameObject.FindGameObjectsWithTag("Molecule");

            foreach (GameObject molecule in molecules)
            {
                molecule.transform.GetChild(4).gameObject.SetActive(true);
            }
        }
        else if (!labelToggle.isOn)
        {
            GameObject[] molecules = GameObject.FindGameObjectsWithTag("Molecule");

            foreach (GameObject molecule in molecules)
            {
                molecule.transform.GetChild(4).gameObject.SetActive(false);
            }
        }
    }
}
