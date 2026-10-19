using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class FilterMolecules : MonoBehaviour
{
    public GameObject moleculeList;
    public TMP_Dropdown dropdown;
    private List<GameObject> activeMoleculeList = new List<GameObject>();
    private int numMolecules = 0;

    private void Start()
    {
        for (int i = 0; i < moleculeList.transform.childCount; i++)
        {
            if (moleculeList.transform.GetChild(i).gameObject.activeSelf)
            {
                activeMoleculeList.Add(moleculeList.transform.GetChild(i).gameObject);
                numMolecules++;
            }
        }
    }

    public void HandleInputData(int val)
    {
        // Shows all molecules
        if (val == 0)
        {
            for (int i = 0; i < numMolecules; i++)
            {
                activeMoleculeList[i].SetActive(true);
            }
        }
        // Individal molecule filtering
        else
        {
            string molecule = dropdown.options[val].text;

            for (int i = 0; i < numMolecules; i++)
            {
                string moleculeFileName = activeMoleculeList[i].name;
                int index = moleculeFileName.IndexOf("_");
                string moleculeName = moleculeFileName.Substring(0, index);

                if (moleculeName == molecule)
                {
                    activeMoleculeList[i].SetActive(true);
                }
                else
                {
                    activeMoleculeList[i].SetActive(false);
                }
            }
        }
    }
}
