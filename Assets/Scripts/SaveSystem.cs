using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveSystem : MonoBehaviour
{
    [System.Serializable]
    private struct SaveData
    {
        public GameObject[] molecules;
    }

    private SaveData saveData = new SaveData();

    // ========== Saving ==========
    public void SaveRoom()
    {
        SaveMolecules();
        Save();
    }

    private void SaveMolecules()
    {
        saveData.molecules = GameObject.FindGameObjectsWithTag("Molecule");
    }

    private void Save()
    {
        string saveFile = Application.persistentDataPath + "/save" + ".save";
        File.WriteAllText(saveFile, JsonUtility.ToJson(saveData, true));
    }

    // ========== Loading ==========
    public void LoadRoom()
    {
        Load();
        LoadMolecules();
    }

    private void Load()
    {
        string saveFile = Application.persistentDataPath + "/save" + ".save";
        string saveContent = File.ReadAllText(saveFile);
        saveData = JsonUtility.FromJson<SaveData>(saveContent);

        LoadScene.LoadSceneByIndex(1);
    }

    private void LoadMolecules()
    {
        for (int i = 0; i < saveData.molecules.Length; i++)
        {
            Instantiate(saveData.molecules[i]);
        }
    }
}
