using UnityEngine;

[System.Serializable]
public class RadialMenuOption
{
    public string id;
    public string displayText;
    public Color displayColor = Color.white;
    public RadialMenuData subMenu;
}