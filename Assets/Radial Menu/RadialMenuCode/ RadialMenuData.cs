using UnityEngine;

[CreateAssetMenu(fileName = "RadialMenuData", menuName = "Radial Menu/Menu Data")]
public class RadialMenuData : ScriptableObject
{
    public string menuName;
    public RadialMenuOption[] options;
}