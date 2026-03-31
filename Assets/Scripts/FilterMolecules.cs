using UnityEngine;

public class FilterMolecules : MonoBehaviour
{
    public GameObject moleculeList;
    private int numMolecules;

    private void Start()
    {
        numMolecules = moleculeList.transform.childCount;
    }

    public void HandleInputData(int val)
    {
        // All
        if (val == 0)
        {
            for (int i = 0; i < numMolecules; i++)
            {
                moleculeList.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
        // BF3
        else if (val == 1)
        {
            for (int i = 0; i < numMolecules; i++)
            {
                string moleculeFileName = moleculeList.transform.GetChild(i).name;
                int index = moleculeFileName.IndexOf("_");
                string molecule = moleculeFileName.Substring(0, index);

                if (molecule == "BF3")
                {
                    moleculeList.transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    moleculeList.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        // NH3
        else if (val == 2)
        {
            for (int i = 0; i < numMolecules; i++)
            {
                string moleculeFileName = moleculeList.transform.GetChild(i).name;
                int index = moleculeFileName.IndexOf("_");
                string molecule = moleculeFileName.Substring(0, index);

                if (molecule == "NH3")
                {
                    moleculeList.transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    moleculeList.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        // BF3NH3
        else if (val == 3)
        {
            for (int i = 0; i < numMolecules; i++)
            {
                string moleculeFileName = moleculeList.transform.GetChild(i).name;
                int index = moleculeFileName.IndexOf("_");
                string molecule = moleculeFileName.Substring(0, index);

                if (molecule == "BF3NH3")
                {
                    moleculeList.transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    moleculeList.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
    }
}
