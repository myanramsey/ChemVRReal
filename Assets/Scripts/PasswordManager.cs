using Microsoft.MixedReality.Toolkit.Experimental.UI;
using System;
using TMPro;
using UnityEngine;

public class PasswordManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI textMesh;
    [SerializeField] private string password;

    private void Start()
    {
        inputField.onSelect.AddListener(x => OpenKeyboard());

        NonNativeKeyboard.Instance.OnTextSubmitted += OnTextSubmitted;
    }

    private void OnDestroy()
    {
        NonNativeKeyboard.Instance.OnTextSubmitted -= OnTextSubmitted;
    }

    private void OpenKeyboard()
    {
        NonNativeKeyboard.Instance.InputField = inputField;
        NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);
    }

    private void OnTextSubmitted(object sender, EventArgs e)
    {
        CheckPassword(NonNativeKeyboard.Instance.InputField.text);
    }

    private void CheckPassword(string input)
    {
        if (password == input)
        {
            LoadScene.LoadSceneByIndex(2);
        }
        else
        {
            textMesh.text = "Incorrect Password";
            textMesh.color = Color.red;
        }
    }
}
