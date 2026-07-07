using System.Collections.Generic;
using UnityEngine;

public class RadialMenuManager : MonoBehaviour
{
    public RadialMenuController radialMenuController;
    public RadialMenuData rootMenu;

    private Stack<RadialMenuData> menuStack = new Stack<RadialMenuData>();

    void Start()
    {
        if (radialMenuController == null)
        {
            Debug.LogError("[RadialMenuManager] RadialMenuController is not assigned.");
            return;
        }

        radialMenuController.onOptionConfirmed.AddListener(HandleOptionConfirmed);

        if (rootMenu != null)
            radialMenuController.SetMenu(rootMenu);
    }

    void OnDestroy()
    {
        if (radialMenuController != null)
            radialMenuController.onOptionConfirmed.RemoveListener(HandleOptionConfirmed);
    }

    void HandleOptionConfirmed(RadialMenuOption option)
    {
        if (option.id == "back")
        {
            GoBack();
            return;
        }

        if (option.subMenu != null)
        {
            menuStack.Push(radialMenuController.currentMenu);
            radialMenuController.SetMenu(option.subMenu);
            radialMenuController.OpenMenu();
            return;
        }

        Debug.Log("[RadialMenuManager] Final option selected: " + option.id);
    }

    public void GoBack()
    {
        if (menuStack.Count == 0)
        {
            radialMenuController.CloseMenu();
            return;
        }

        RadialMenuData previousMenu = menuStack.Pop();
        radialMenuController.SetMenu(previousMenu);
        radialMenuController.OpenMenu();
    }
}