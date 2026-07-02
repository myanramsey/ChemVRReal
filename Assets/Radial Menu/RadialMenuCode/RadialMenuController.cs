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

    [Header("Label Settings")]
    public float labelRadius = 80f;
    public int labelFontSize = 18;
    public Color labelColor = Color.white;

    [Header("State")]
    public RadialMenuData currentMenu;
    public RadialMenuSelectionEvent onOptionConfirmed;

    [Header("Mode Scripts")]
    public DeleteMolecule deleteMolecule;
    public MoleculeScale moleculeScale;

    private readonly List<GameObject> spawnedParts = new List<GameObject>();
    private readonly List<GameObject> spawnedImageParts = new List<GameObject>();
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
        deleteMolecule?.ExitDeleteMode();
        moleculeScale?.ExitScaleMode();
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

        // First pass: create all image slices so they are low in the hierarchy.
        for (int i = 0; i < partCount; i++)
        {
            float angle = -i * 360f / partCount - angleBetweenRadialParts / 2f;

            GameObject part = Instantiate(radialPartPrefab, radialPartCanvas);
            part.transform.localPosition = Vector3.zero;
            part.transform.localEulerAngles = new Vector3(0f, 0f, angle);
            part.transform.localScale = Vector3.one;

            Image image = part.GetComponent<Image>();
            if (image != null)
            {
                image.fillAmount = (1f / partCount) - (angleBetweenRadialParts / 360f);
                image.color = currentMenu.options[i].displayColor;
            }

            spawnedParts.Add(part);
            spawnedImageParts.Add(part);
        }

        // Second pass: create all labels after all images so they render on top.
        for (int i = 0; i < partCount; i++)
        {
            AddLabel(currentMenu.options[i].displayText, i, partCount);
        }
    }

    void AddLabel(string text, int index, int totalCount)
    {
        // Slices rotate clockwise (negative Z). To match, use a positive clockwise
        // angle so sin/cos place the label inside the correct slice.
        float sliceAngleDeg = 360f / totalCount;
        float midAngleDeg = index * sliceAngleDeg + sliceAngleDeg / 2f;
        float midAngleRad = midAngleDeg * Mathf.Deg2Rad;

        float labelX = Mathf.Sin(midAngleRad) * labelRadius;
        float labelY = Mathf.Cos(midAngleRad) * labelRadius;

        GameObject labelObj = new GameObject("Label_" + index);
        labelObj.transform.SetParent(radialPartCanvas, false);

        RectTransform rt = labelObj.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(labelX, labelY);
        rt.sizeDelta = new Vector2(100f, 40f);

        Text uiText = labelObj.AddComponent<Text>();
        uiText.text = text;
        uiText.fontSize = labelFontSize;
        uiText.color = labelColor;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.horizontalOverflow = HorizontalWrapMode.Overflow;
        uiText.verticalOverflow = VerticalWrapMode.Overflow;

        spawnedParts.Add(labelObj);
    }

    void ClearMenu()
    {
        foreach (GameObject part in spawnedParts)
        {
            if (part != null)
                Destroy(part);
        }

        spawnedParts.Clear();
        spawnedImageParts.Clear();
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

        for (int i = 0; i < spawnedImageParts.Count; i++)
        {
            Image image = spawnedImageParts[i].GetComponent<Image>();
            if (image == null)
                continue;

            Color baseColor = currentMenu.options[i].displayColor;

            if (i == currentSelectedIndex)
            {
                image.color = baseColor;
                spawnedImageParts[i].transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                image.color = new Color(baseColor.r * 0.45f, baseColor.g * 0.45f, baseColor.b * 0.45f, baseColor.a);
                spawnedImageParts[i].transform.localScale = Vector3.one;
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