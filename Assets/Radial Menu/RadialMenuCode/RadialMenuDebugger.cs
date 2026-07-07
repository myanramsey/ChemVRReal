using UnityEngine;
using UnityEngine.UI;

public class RadialMenuDebugger : MonoBehaviour
{
    public RadialMenuController controller;
    public Transform radialPartCanvas;
    public Transform handTransform;
    public GameObject radialPartPrefab;

    [Header("Debug Settings")]
    public bool logOnStart = true;
    public bool logEveryFewSeconds = true;
    public float logInterval = 2f;

    private float timer;

    void Start()
    {
        if (logOnStart)
        {
            Debug.Log("=== RADIAL MENU DEBUGGER: START ===");
            ValidateSetup();
        }
    }

    void Update()
    {
        if (!logEveryFewSeconds)
            return;

        timer += Time.deltaTime;
        if (timer >= logInterval)
        {
            timer = 0f;
            LogRuntimeState();
        }
    }

    [ContextMenu("Validate Setup")]
    public void ValidateSetup()
    {
        if (controller == null)
            Debug.LogError("[RadialDebug] controller is NULL. Assign the RadialMenuController.");
        else
            Debug.Log("[RadialDebug] controller found: " + controller.name);

        if (radialPartCanvas == null)
            Debug.LogError("[RadialDebug] radialPartCanvas is NULL.");
        else
            Debug.Log("[RadialDebug] radialPartCanvas found: " + radialPartCanvas.name);

        if (handTransform == null)
            Debug.LogError("[RadialDebug] handTransform is NULL.");
        else
            Debug.Log("[RadialDebug] handTransform found: " + handTransform.name);

        if (radialPartPrefab == null)
        {
            Debug.LogError("[RadialDebug] radialPartPrefab is NULL.");
        }
        else
        {
            Debug.Log("[RadialDebug] radialPartPrefab found: " + radialPartPrefab.name);

            Image image = radialPartPrefab.GetComponent<Image>();
            if (image == null)
                Debug.LogError("[RadialDebug] radialPartPrefab does NOT have an Image component on the root.");
            else
                Debug.Log("[RadialDebug] radialPartPrefab Image found.");
        }

        if (controller != null)
        {
            if (controller.currentMenu == null)
            {
                Debug.LogError("[RadialDebug] currentMenu is NULL. The menu cannot build without menu data.");
            }
            else
            {
                Debug.Log("[RadialDebug] currentMenu found: " + controller.currentMenu.menuName);

                if (controller.currentMenu.options == null || controller.currentMenu.options.Length == 0)
                    Debug.LogError("[RadialDebug] currentMenu has no options.");
                else
                    Debug.Log("[RadialDebug] currentMenu option count: " + controller.currentMenu.options.Length);
            }
        }
    }

    [ContextMenu("Log Runtime State")]
    public void LogRuntimeState()
    {
        Debug.Log("=== RADIAL MENU DEBUGGER: RUNTIME ===");

        if (controller == null)
        {
            Debug.LogError("[RadialDebug] Cannot inspect runtime because controller is NULL.");
            return;
        }

        if (radialPartCanvas != null)
        {
            Debug.Log("[RadialDebug] Canvas activeInHierarchy: " + radialPartCanvas.gameObject.activeInHierarchy);
            Debug.Log("[RadialDebug] Canvas position: " + radialPartCanvas.position);
            Debug.Log("[RadialDebug] Canvas localScale: " + radialPartCanvas.localScale);
            Debug.Log("[RadialDebug] Canvas child count: " + radialPartCanvas.childCount);
        }

        if (handTransform != null && radialPartCanvas != null)
        {
            float distance = Vector3.Distance(handTransform.position, radialPartCanvas.position);
            Debug.Log("[RadialDebug] Distance from hand to canvas: " + distance);
        }

        if (controller.currentMenu != null && controller.currentMenu.options != null)
        {
            for (int i = 0; i < controller.currentMenu.options.Length; i++)
            {
                var option = controller.currentMenu.options[i];
                Debug.Log("[RadialDebug] Option " + i + ": id=" + option.id + ", text=" + option.displayText);
            }
        }

        if (radialPartCanvas != null)
        {
            for (int i = 0; i < radialPartCanvas.childCount; i++)
            {
                Transform child = radialPartCanvas.GetChild(i);
                Image img = child.GetComponent<Image>();

                Debug.Log(
                    "[RadialDebug] Child " + i +
                    " | name=" + child.name +
                    " | active=" + child.gameObject.activeInHierarchy +
                    " | localPos=" + child.localPosition +
                    " | localScale=" + child.localScale +
                    " | imageFound=" + (img != null)
                );

                if (img != null)
                {
                    Debug.Log(
                        "[RadialDebug] Child " + i +
                        " | color=" + img.color +
                        " | fillAmount=" + img.fillAmount
                    );
                }
            }
        }
    }
}