using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [Header("Scene Loading Settings")]
    [Tooltip("The name of the scene that will be loaded.")]
    [SerializeField] private string sceneName;

    // Loads scene by index
    public void LoadSceneByName()
    {
        SceneManager.LoadScene(sceneName);
    }
}