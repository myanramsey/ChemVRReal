using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using Unity.XR.CoreUtils;
using UnityEditor;
using TMPro;

public class RadialMenuController : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty openButton;
    public InputActionProperty selectButton;

    [Header("UI")]
    public GameObject radialPartPrefab;
    public Transform radialPartCanvas;
    public Transform handTransform;
    [Tooltip("Where the wheel spawns when opened. Assign an empty GameObject anchored to the player (e.g. a child of the XR Origin) rather than the hand, so the menu opens on the player instead of wherever the controller happens to be pointing.")]
    public Transform spawnAnchor;
    public XROrigin xrOrigin;
    public float angleBetweenRadialParts = 5f;

    [Header("Label Settings")]
    public float labelRadius = 80f;
    public int labelFontSize = 12;
    public Color labelColor = Color.white;

    [Header("State")]
    public RadialMenuData currentMenu;
    public RadialMenuSelectionEvent onOptionConfirmed;

    [Header("Mode Scripts")]
    public DeleteMolecule deleteMolecule;
    public MoleculeScale moleculeScale;
    public OrbitalOpacity orbitalOpacity;
    public ToggleBackboneOrbital toggleBackboneOrbital;
    public ShowColorPickerMenu colorMenu;
    public ShowMOSliderMenu sliderMenu;

    private readonly List<GameObject> spawnedParts = new List<GameObject>();
    private readonly List<GameObject> spawnedImageParts = new List<GameObject>();
    private int currentSelectedIndex = -1;
    private bool menuOpen = false;

    private RadialMenuOption lastSelectedOption;
    private Color lastSelectedColor;

    private Vector3 menuLocalOffset;
    private bool hasMenuOffset = false;

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
        ExitAllModes();
        BuildMenu();
        radialPartCanvas.gameObject.SetActive(true);

        // Spawn radial menu in front and facing player
        Transform vrPlayer = xrOrigin.transform;

        Vector3 targetPos = handTransform.position + (handTransform.forward * 0.25f);
        targetPos.y = handTransform.position.y + 0.1f;
        radialPartCanvas.transform.position = targetPos;

        Quaternion targetRot = Quaternion.LookRotation(vrPlayer.forward);
        radialPartCanvas.transform.rotation = Quaternion.Euler(0, targetRot.eulerAngles.y, 0);

        // Keep track of offset to add to menu if player moves
        menuLocalOffset = vrPlayer.InverseTransformPoint(radialPartCanvas.position);
        hasMenuOffset = true;
    }

    void LateUpdate()
    {
        if (!menuOpen || xrOrigin == null || !hasMenuOffset) return;

        float menuFollowSpeed = 1f;
        Transform vrPlayer = xrOrigin.transform;

        Vector3 targetPos = vrPlayer.TransformPoint(menuLocalOffset);
        radialPartCanvas.transform.position = targetPos;

        Quaternion targetRot = Quaternion.LookRotation(vrPlayer.forward);
        radialPartCanvas.transform.rotation = Quaternion.Euler(0, targetRot.eulerAngles.y, 0);

        radialPartCanvas.position = Vector3.Lerp(radialPartCanvas.position, targetPos, Time.deltaTime * menuFollowSpeed);
        radialPartCanvas.rotation = Quaternion.Slerp(radialPartCanvas.rotation, targetRot, Time.deltaTime * menuFollowSpeed);
    }

    public void CloseMenu()
    {
        menuOpen = false;
        currentSelectedIndex = -1;

        if (radialPartCanvas != null)
            radialPartCanvas.gameObject.SetActive(false);

        if (lastSelectedOption != null)
        {
            lastSelectedOption.displayColor = lastSelectedColor;
        }
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

                if (lastSelectedOption != null)
                {
                    lastSelectedOption.displayColor = lastSelectedColor;
                }
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
        rt.sizeDelta = new Vector2(60f, 80f);

        Text uiText = labelObj.AddComponent<Text>();
        uiText.text = text;
        uiText.fontSize = labelFontSize;
        uiText.color = labelColor;
        uiText.alignment = TextAnchor.MiddleCenter;
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
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
        Ray ray = new Ray(handTransform.position, handTransform.forward);

        Plane menuPlane = new Plane(radialPartCanvas.forward, radialPartCanvas.position);

        if (!menuPlane.Raycast(ray, out float enter) || enter < 0f)
        {
            SetSelectedIndex(-1);
            return;
        }

        Vector3 hitPoint = ray.GetPoint(enter);

        Vector3 localHit = radialPartCanvas.InverseTransformPoint(hitPoint);

        // Assumes the wheel is centered at local (0, 0).
        float radius = Mathf.Sqrt(localHit.x * localHit.x + localHit.y * localHit.y);

        RectTransform firstPart = spawnedImageParts[0].GetComponent<RectTransform>();

        if (firstPart == null) return;

        // Assumes the radial image is square and centered.
        float wheelRadius = firstPart.rect.width * 0.5f;

        if (radius > wheelRadius || radius < 0.001f)
        {
            SetSelectedIndex(-1);
            return;
        }
        
        float angle = Mathf.Atan2(localHit.x, localHit.y) * Mathf.Rad2Deg;

        if (angle < 0f)
        { 
            angle += 360f;
        }

        int partCount = currentMenu.options.Length;
        float sliceAngle = 360f / partCount;

        int index = Mathf.FloorToInt(angle / sliceAngle);

        float angleWithinSlice = angle % sliceAngle;

        float gap = angleBetweenRadialParts;

        if (angleWithinSlice < gap / 2f || angleWithinSlice > sliceAngle - gap / 2f)
        {
            SetSelectedIndex(-1);
            return;
        }

        SetSelectedIndex(index);
    }

    void SetSelectedIndex(int index)
    {
        currentSelectedIndex = index;

        for (int i = 0; i < spawnedImageParts.Count; i++)
        {
            Image image = spawnedImageParts[i].GetComponent<Image>();
            if (image == null) continue;

            Color baseColor = currentMenu.options[i].displayColor;

            if (i == index)
            {
                image.color = baseColor;
                spawnedImageParts[i].transform.localScale =
                    Vector3.one * 1.1f;
            }
            else
            {
                image.color = new Color(
                    baseColor.r * 0.45f,
                    baseColor.g * 0.45f,
                    baseColor.b * 0.45f,
                    baseColor.a
                );

                spawnedImageParts[i].transform.localScale =
                    Vector3.one;
            }
        }
    }

    void ConfirmSelection()
    {
        if (currentSelectedIndex < 0 || currentSelectedIndex >= currentMenu.options.Length) return;

        if (lastSelectedOption != null)
        {
            lastSelectedOption.displayColor = lastSelectedColor;
        }

        RadialMenuOption selectedOption = currentMenu.options[currentSelectedIndex];
        lastSelectedOption = selectedOption;
        lastSelectedColor = selectedOption.displayColor;
        if (!(selectedOption.id == "ray_alpha_increase") && !(selectedOption.id == "ray_alpha_decrease") && !selectedOption.id.StartsWith("ray_"))
        {
            selectedOption.displayColor = Color.black;
        }
        ExitAllModes();
        onOptionConfirmed?.Invoke(selectedOption);
    }

    void ExitAllModes()
    {
        deleteMolecule?.ExitDeleteMode();
        moleculeScale?.ExitScaleMode();
        orbitalOpacity?.ExitOpacityMode();
        toggleBackboneOrbital?.ExitToggleMode();
        colorMenu?.ExitColorMode();
        sliderMenu?.ExitMOMode();
    }
}