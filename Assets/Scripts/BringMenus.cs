using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEditor;
using Unity.XR.CoreUtils;

public class BringMenus : MonoBehaviour
{
    [SerializeField] private GameObject moleculeSpawnMenu;
    [SerializeField] private GameObject colorPickerMenu;
    [SerializeField] private GameObject screenshotMenu;

    [SerializeField] private XROrigin xrOrigin;
    public InputActionProperty button;
    [SerializeField] private ModeIndicator modeIndicator;

    private List<GameObject> menuList = new List<GameObject>();

    private float height;
    private float xRot;
    private float zRot;

    private void OnEnable()
    {
        button.action.Enable();
    }

    private void OnDisable()
    {
        button.action.Disable();
    }

    private void Start()
    {
        menuList.Add(moleculeSpawnMenu);
        menuList.Add(colorPickerMenu);
        menuList.Add(screenshotMenu);

        height = moleculeSpawnMenu.transform.position.y;
        xRot = moleculeSpawnMenu.transform.rotation.eulerAngles.x;
        zRot = moleculeSpawnMenu.transform.rotation.eulerAngles.z;
    }

    private void Update()
    {
        if (!button.action.WasPressedThisFrame()) return;
        if (modeIndicator.currentMode != "") return;

        Transform vrPlayer = xrOrigin.Camera.transform;
        Vector3 targetPos = vrPlayer.position + (vrPlayer.forward * 2f);
        targetPos.y = height;
        Quaternion targetRot = Quaternion.LookRotation(vrPlayer.forward);

        // Brings every active menu to player
        foreach (GameObject menu in menuList)
        {
            if (menu.activeSelf)
            {
                menu.transform.position = targetPos;
                menu.transform.rotation = Quaternion.Euler(xRot, targetRot.eulerAngles.y, zRot);
            }
        }
    }
}
