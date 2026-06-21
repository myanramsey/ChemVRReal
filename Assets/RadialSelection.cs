using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;


public class RadialSelection : MonoBehaviour
{
    public InputActionProperty spawnButton;
    public InputActionProperty selectButton;

    [Range(2,10)]
    public int numberofRadialParts;
    public GameObject radialPartPrefab;
    public Transform radialPartCanvas;
    public float angleBetweenRadialParts;
    public UnityEvent<int> onRadialPartSelected;

    public Transform handTransform;
    public Color[] radialColors;

    private List<GameObject> spawnedRadialParts = new List<GameObject>();
    private int currentSelectedRadialPartIndex = -1;
    private bool menuOpen = false;

    public void HideTriggerSelected()
    {
        onRadialPartSelected.Invoke(currentSelectedRadialPartIndex);
        radialPartCanvas.gameObject.SetActive(false);
    }

    void Start()
    {
        if (spawnButton.action == null) { Debug.LogError("[RadialMenu] spawnButton is NOT assigned!"); return; }
        spawnButton.action.Enable();
        selectButton.action?.Enable();
        Debug.Log($"[RadialMenu] spawnButton assigned: {spawnButton.action.name}, enabled: {spawnButton.action.enabled}");
    }

    void Update()
    {
        if (spawnButton.action == null) return;

        if (spawnButton.action.WasPressedThisFrame())
        {
            if (!menuOpen)
            {
                Debug.Log("[RadialMenu] Opening menu");
                SpawnRadialPart();
                menuOpen = true;
            }
            else
            {
                Debug.Log("[RadialMenu] Closing menu (toggle)");
                radialPartCanvas.gameObject.SetActive(false);
                menuOpen = false;
            }
        }

        if (menuOpen)
        {
            GetSelectedRadialPart();
        }

        if (menuOpen && selectButton.action != null && selectButton.action.WasPressedThisFrame())
        {
            Debug.Log($"[RadialMenu] Selection confirmed: index {currentSelectedRadialPartIndex}");
            HideTriggerSelected();
            menuOpen = false;
        }
    }

    void SpawnRadialPart()
    {
        if (radialPartCanvas == null) { Debug.LogError("[RadialMenu] radialPartCanvas is NOT assigned!"); return; }
        if (radialPartPrefab == null) { Debug.LogError("[RadialMenu] radialPartPrefab is NOT assigned!"); return; }
        if (handTransform == null)    { Debug.LogError("[RadialMenu] handTransform is NOT assigned!"); return; }
        Debug.Log($"[RadialMenu] Spawning {numberofRadialParts} parts");

        radialPartCanvas.gameObject.SetActive(true);
        radialPartCanvas.position = handTransform.position;
        radialPartCanvas.rotation = handTransform.rotation;

        foreach (GameObject radialPart in spawnedRadialParts)
            Destroy(radialPart);

        spawnedRadialParts.Clear();

        for (int i = 0; i < numberofRadialParts; i++)
        {
            float angle = -i * 360f / numberofRadialParts - angleBetweenRadialParts / 2f;
            Vector3 radialPartEulerAngle = new Vector3(0, 0, angle);

            GameObject spawnedRadialPart = Instantiate(radialPartPrefab, radialPartCanvas);
            spawnedRadialPart.transform.position = radialPartCanvas.position;
            spawnedRadialPart.transform.localEulerAngles = radialPartEulerAngle;
            spawnedRadialPart.GetComponent<Image>().fillAmount = (1 / (float)numberofRadialParts - (angleBetweenRadialParts / 360f));

            if (radialColors != null && i < radialColors.Length)
                spawnedRadialPart.GetComponent<Image>().color = radialColors[i];

            spawnedRadialParts.Add(spawnedRadialPart);
        }
    }

    public void GetSelectedRadialPart()
    {
        Vector3 centerToHand = handTransform.position - radialPartCanvas.position;
        Vector3 centerToHandProjection = Vector3.ProjectOnPlane(centerToHand, radialPartCanvas.forward);

        float angle = Vector3.SignedAngle(radialPartCanvas.up, centerToHandProjection, -radialPartCanvas.forward);
        if (angle < 0)
            angle += 360f;
        currentSelectedRadialPartIndex = (int)(angle * numberofRadialParts / 360f);

        for (int i = 0; i < spawnedRadialParts.Count; i++)
        {
            if (i == currentSelectedRadialPartIndex)
            {
                spawnedRadialParts[i].GetComponent<Image>().color = Color.red;
                spawnedRadialParts[i].transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                if (radialColors != null && i < radialColors.Length)
                    spawnedRadialParts[i].GetComponent<Image>().color = radialColors[i];
                else
                    spawnedRadialParts[i].GetComponent<Image>().color = Color.white;
                spawnedRadialParts[i].transform.localScale = Vector3.one * 1f;
            }
        }
    }
}
