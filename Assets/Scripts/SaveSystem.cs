using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    [System.Serializable]
    private struct SaveData
    {
        // ===== Molecule Info =====
        public int numMolecules;

        // Molecule names
        public List<string> names;

        // Transform info
        public List<Vector3> positions;
        public List<Vector3> rotations;
        public List<Vector3> scales;

        // Color info
        public List<Color> orbitalColors;
        public List<Color> atomColors;
        public List<Color> atomVertexColors;

        // ===== Settings Info =====
        public int wallColor;
        public int floorColor;
    }

    private static SaveData saveData = new SaveData();

    private static string saveFolder;
    private static string saveFileName;

    private void Awake()
    {
        saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
    }

    // ========== Saving ==========
    public static void SaveRoom()
    {
        SaveMolecules();
        Save();
    }

    private static void SaveMolecules()
    {
        // Initialize lists
        saveData.names = new List<string>();
        saveData.positions = new List<Vector3>();
        saveData.rotations = new List<Vector3>();
        saveData.scales = new List<Vector3>();
        saveData.orbitalColors = new List<Color>();
        saveData.atomColors = new List<Color>();
        saveData.atomVertexColors = new List<Color>();

        GameObject[] molecules = GameObject.FindGameObjectsWithTag("Molecule");
        saveData.numMolecules = molecules.Length;

        for (int i = 0; i < molecules.Length; i++)
        {
            saveData.names.Add(molecules[i].name);

            // Save transform
            saveData.positions.Add(molecules[i].transform.position);
            saveData.rotations.Add(new Vector3(molecules[i].transform.rotation.eulerAngles.x, molecules[i].transform.rotation.eulerAngles.y, molecules[i].transform.rotation.eulerAngles.z));
            saveData.scales.Add(molecules[i].transform.localScale);

            // Save orbital colors
            for (int j = 0; j < molecules[i].transform.GetChild(0).childCount; j++)
            {
                GameObject orbital = molecules[i].transform.GetChild(0).GetChild(j).gameObject;
                Color orbitalColor = orbital.GetComponent<MeshRenderer>().material.color;
                saveData.orbitalColors.Add(orbitalColor);
            }

            // Save atom/bond colors
            for (int j = 0; j < molecules[i].transform.GetChild(1).childCount; j++)
            {
                GameObject atom = molecules[i].transform.GetChild(1).GetChild(j).gameObject;
                Color atomColor = atom.GetComponent<MeshRenderer>().material.color;
                saveData.atomColors.Add(atomColor);
            }

            // Save atom/bond vertex colors
            for (int j = 0; j < molecules[i].transform.GetChild(1).childCount; j++)
            {
                // Save vertex colors
                GameObject atom = molecules[i].transform.GetChild(1).GetChild(j).gameObject;
                Mesh mesh = atom.GetComponent<MeshFilter>().mesh;
                Color[] meshColors = mesh.colors;
                Color meshColor = meshColors[0];
                saveData.atomVertexColors.Add(meshColor);
            }
        }
    }

    private static void Save()
    {
        string[] files = Directory.GetFiles(saveFolder, "*.json");
        int fileNum = files.Length + 1;
        string saveFile = Path.Combine(Application.persistentDataPath, "Saves", saveFileName);
        File.WriteAllText(saveFile, JsonUtility.ToJson(saveData, true));
    }

    // ========== Loading ==========
    public static void LoadRoom()
    {
        Load();
    }

    private static void Load()
    {
        string saveFile = Path.Combine(Application.persistentDataPath, "Saves", saveFileName);

        if (!File.Exists(saveFile))
        {
            return;
        }

        string saveContent = File.ReadAllText(saveFile);
        saveData = JsonUtility.FromJson<SaveData>(saveContent);

        SceneManager.sceneLoaded += OnSceneLoaded;
        LoadScene.LoadSceneByIndex(1);
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        LoadMolecules();
        Settings.RestoreSettings(saveData.wallColor, saveData.floorColor);
    }

    private static void LoadMolecules()
    {
        Debug.Log(saveData.orbitalColors.Count);

        for (int i = 0; i < saveData.numMolecules; i++)
        {
            // Load Transform 
            GameObject molecule = Resources.Load<GameObject>(saveData.names[i]);
            GameObject moleculeInstance = Instantiate(molecule, saveData.positions[i], Quaternion.Euler(saveData.rotations[i].x, saveData.rotations[i].y, saveData.rotations[i].z));
            moleculeInstance.transform.localScale = saveData.scales[i];

            // Load colors
            // Orbitals
            for (int j = 0; j < moleculeInstance.transform.GetChild(0).childCount; j++)
            {
                GameObject orbital = moleculeInstance.transform.GetChild(0).GetChild(j).gameObject;
                orbital.GetComponent<MeshRenderer>().material.color = saveData.orbitalColors[0];
                saveData.orbitalColors.RemoveAt(0);
            }

            // Atoms and Bonds
            for (int j = 0; j < moleculeInstance.transform.GetChild(1).childCount; j++)
            {
                // Reset material color
                GameObject atom = moleculeInstance.transform.GetChild(1).GetChild(j).gameObject;
                atom.GetComponent<MeshRenderer>().material.color = saveData.atomColors[0];
                saveData.atomColors.RemoveAt(0);

                // Reset vertex color
                Mesh mesh = atom.GetComponent<MeshFilter>().mesh;
                Color[] colors = new Color[mesh.colors.Length];
                for (int k = 0; k < colors.Length; k++)
                {
                    colors[k] = saveData.atomVertexColors[0];
                }
                mesh.colors = colors;
                saveData.atomVertexColors.RemoveAt(0);
            }

            // Edit molecule instance name
            int index = moleculeInstance.name.IndexOf("(");
            moleculeInstance.name = moleculeInstance.name.Substring(0, index);

            // Edit components
            Rigidbody rb = moleculeInstance.GetComponent<Rigidbody>();
            var grab = moleculeInstance.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

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
        }
    }

    // ========== Mutators ==========
    public static void SetSaveFileNameAndLoad(string name)
    {
        saveFileName = name;
        LoadRoom();
    }

    public static void SetSaveFileNameAndSave(string name)
    {
        saveFileName = name;
        SaveRoom();
    }

    public static void SetWallColor(int color)
    {
        saveData.wallColor = color;
    }

    public static void SetFloorColor(int color)
    {
        saveData.floorColor = color;
    }
}
