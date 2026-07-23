using UnityEngine;
using System.Collections;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;

public class SaveRoom : MonoBehaviour
{
    [SerializeField] TMP_Text buttonText;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameObject savePage;
    [SerializeField] GameObject menuPage;

    private float waitDuration = 2f;

    private void Start()
    {
        inputField.onSelect.AddListener(x => OpenKeyboard());

        NonNativeKeyboard.Instance.OnTextSubmitted += OnTextSubmitted;
    }

    private void OnDestroy()
    {
        NonNativeKeyboard.Instance.OnTextSubmitted -= OnTextSubmitted;
    }

    private void UpdateText()
    {
        StartCoroutine(SavedPopup());
    }

    private IEnumerator SavedPopup()
    {
        // Update text
        buttonText.text = "Saved!";
        buttonText.color = Color.green;

        // Duration that screenshot notifcation is fully visible
        yield return new WaitForSeconds(waitDuration);

        // Screenshot notification gradually disappears
        buttonText.text = "Save Room";
        buttonText.color = new Color32(50, 50, 50, 255);
    }

    private void OpenKeyboard()
    {
        NonNativeKeyboard.Instance.InputField = inputField;
        NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);
    }

    private void OnTextSubmitted(object sender, EventArgs e)
    {
        SubmitFileName(NonNativeKeyboard.Instance.InputField.text);
    }

    private void SubmitFileName(string fileName)
    {
        if (fileName == null || fileName == "")
        {
            return;
        }

        fileName = fileName + ".json";
        SaveSystem.SetSaveFileNameAndSave(fileName);

        savePage.SetActive(false);
        menuPage.SetActive(true);

        UpdateText();
    }
}
