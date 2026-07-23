using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class LoadRoom : MonoBehaviour
{
    [SerializeField] private RectTransform roomListTransform;
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private RectTransform deleteButtonsTransform;
    [SerializeField] private Button deleteButtonPrefab;
    [SerializeField] private RectTransform contentTransform;

    private string[] files;
    int numFiles;

    private void OnEnable()
    {
        CreateButtons();
        RefreshCanvas();
    }

    private void OnDisable()
    {
        foreach (Transform child in roomListTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in deleteButtonsTransform)
        {
            Destroy(child.gameObject);
        }
    }

    // Creates buttons for each save file
    private void CreateButtons()
    {
        // Access save files
        string saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        files = Directory.GetFiles(saveFolder, "*.json");
        numFiles = files.Length;

        foreach (string file in files)
        { 
            // Create a load button for each room
            Button button = Instantiate(buttonPrefab, roomListTransform);
            string fileName = Path.GetFileName(file);
            string text = fileName.Substring(0, fileName.Length - 5);
            button.GetComponentInChildren<TextMeshProUGUI>().text = text;

            button.onClick.AddListener(() =>
            {
                SetRoomToLoad(fileName);
            });

            // Create a delete button for each save
            Button deleteButton = Instantiate(deleteButtonPrefab, deleteButtonsTransform);

            deleteButton.onClick.AddListener(() =>
            {
                DeleteSaveFile(fileName, button, deleteButton);
                RefreshCanvas();
            });
        }
    }

    private void RefreshCanvas()
    {
        // Update height of content so that all save files can be seen in scroll area
        contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, numFiles * 35);

        // Force canvas to update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(roomListTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(deleteButtonsTransform);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
    }

    // Set the save file to be loaded by the SaveSystem script
    private void SetRoomToLoad(string saveFileName)
    {
        SaveSystem.SetSaveFileNameAndLoad(saveFileName);
    }

    // Deletes save file
    private void DeleteSaveFile(string saveFileName, Button button, Button deleteButton)
    {
        string saveFile = Path.Combine(Application.persistentDataPath, "Saves", saveFileName);
        File.Delete(saveFile);

        numFiles -= 1;

        Destroy(button.gameObject);
        Destroy(deleteButton.gameObject);
    }
}
