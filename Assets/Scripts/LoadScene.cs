using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [Header("Scene Loading Settings")]
    [Tooltip("The name of the scene that will be loaded.")]
    public string sceneName = "";
    [Tooltip("The index of the scene that will be loaded.")]
    public int sceneIndex = 0;

    // Loads scene by name
    public void LoadSceneByName()
    {
        SceneManager.LoadScene(sceneName);
    }

    // Loads scene by index
    public void LoadSceneByIndex()
    {
        SceneManager.LoadScene(sceneIndex);
    }
}