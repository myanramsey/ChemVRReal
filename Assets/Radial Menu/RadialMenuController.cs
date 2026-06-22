using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RadialMenuController : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty openButton;
    public InputActionProperty selectButton;

    [Header("UI")]
    public GameObject radialPartPrefab;
    public Transform radialPartCanvas;
    public Transform handTransform;
    public float angleBetweenRadialParts = 5f;

    [Header("State")]
    public RadialMenuData currentMenu;
    public RadialMenuSelectionEvent onOptionConfirmed;

    private readonly List<GameObject> spawnedParts = new List<GameObject>();
    private int currentSelectedIndex = -1;
    private bool menuOpen = false;

    void Start()
    {
        if (openButton.action != null)
            openButton.action.Enable();

        if (selectButton.action != null)
            selectButton.action.Enable();

        if (radialPartCanvas != null)
            radialPartCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (openButton.action != null && openButton.action.WasPressedThisFrame())
        {
            if (!menuOpen)
                OpenMenu();
            else
                CloseMenu();
        }

        if (!menuOpen || currentMenu == null || currentMenu.options == null || currentMenu.options.Length == 0)
            return;

        UpdateSelection();

        if (selectButton.action != null && selectButton.action.WasPressedThisFrame())
            ConfirmSelection();
    }

    public void SetMenu(RadialMenuData menuData)
    {
        currentMenu = menuData;

        if (menuOpen)
            BuildMenu();
    }

    public void OpenMenu()
    {
        if (currentMenu == null)
        {
            Debug.LogWarning("[RadialMenu] No current menu assigned.");
            return;
        }

        menuOpen = true;
        BuildMenu();
        radialPartCanvas.gameObject.SetActive(true);
        radialPartCanvas.position = handTransform.position;
        radialPartCanvas.rotation = handTransform.rotation;
    }

    public void CloseMenu()
    {
        menuOpen = false;
        currentSelectedIndex = -1;

        if (radialPartCanvas != null)
            radialPartCanvas.gameObject.SetActive(false);
    }

    void BuildMenu()
    {
        ClearMenu();

        int partCount = currentMenu.options.Length;

        for (int i = 0; i < partCount; i++)
        {
            float angle = -i * 360f / partCount - angleBetweenRadialParts / 2f;
            Vector3 eulerAngles = new Vector3(0f, 0f, angle);

            GameObject part = Instantiate(radialPartPrefab, radialPartCanvas);
            part.transform.localPosition = Vector3.zero;
            part.transform.localEulerAngles = eulerAngles;
            part.transform.localScale = Vector3.one;

            Image image = part.GetComponent<Image>();
            if (image != null)
            {
                image.fillAmount = (1f / partCount) - (angleBetweenRadialParts / 360f);
                image.color = currentMenu.options[i].displayColor;
            }

            spawnedParts.Add(part);
        }
    }

    void ClearMenu()
    {
        foreach (GameObject part in spawnedParts)
        {
            if (part != null)
                Destroy(part);
        }

        spawnedParts.Clear();
    }

    void UpdateSelection()
    {
        Vector3 centerToHand = handTransform.position - radialPartCanvas.position;
        Vector3 projected = Vector3.ProjectOnPlane(centerToHand, radialPartCanvas.forward);

        if (projected.sqrMagnitude < 0.0001f)
            return;

        float angle = Vector3.SignedAngle(radialPartCanvas.up, projected, -radialPartCanvas.forward);
        if (angle < 0f)
            angle += 360f;

        currentSelectedIndex = Mathf.Clamp(
            (int)(angle * currentMenu.options.Length / 360f),
            0,
            currentMenu.options.Length - 1
        );

        for (int i = 0; i < spawnedParts.Count; i++)
        {
            Image image = spawnedParts[i].GetComponent<Image>();
            if (image == null)
                continue;

            Color baseColor = currentMenu.options[i].displayColor;

            if (i == currentSelectedIndex)
            {
                image.color = baseColor;
                spawnedParts[i].transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                image.color = new Color(baseColor.r * 0.45f, baseColor.g * 0.45f, baseColor.b * 0.45f, baseColor.a);
                spawnedParts[i].transform.localScale = Vector3.one;
            }
        }
    }

    void ConfirmSelection()
    {
        if (currentSelectedIndex < 0 || currentSelectedIndex >= currentMenu.options.Length)
            return;

        RadialMenuOption selectedOption = currentMenu.options[currentSelectedIndex];
        onOptionConfirmed?.Invoke(selectedOption);
    }
}