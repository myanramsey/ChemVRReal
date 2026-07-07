using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] walls;
    [SerializeField] private MeshRenderer floor;
    [SerializeField] private Slider wallSlider, floorSlider;

    private Color[] wallColors;
    private Color[] floorColors;

    private TMP_InputField wallField;
    private TMP_InputField floorField;

    private void Start()
    {
        // Initalize color arrays
        Color wallColor = walls[0].material.color;
        wallColors = new Color[] { wallColor, Color.white, Color.black, Color.darkGray, Color.gray, Color.navyBlue, Color.steelBlue, Color.darkRed, Color.orange, Color.darkOrange, Color.darkGreen};

        Color floorColor = floor.material.color;
        floorColors = new Color[] { floorColor, Color.white, Color.black, Color.darkGray, Color.gray, Color.blue, Color.deepSkyBlue, Color.softRed, Color.tan, Color.darkKhaki, Color.seaGreen};

        // Find input fields for each slider
        wallField = wallSlider.gameObject.transform.GetChild(4).GetComponent<TMP_InputField>();
        floorField = floorSlider.gameObject.transform.GetChild(4).GetComponent<TMP_InputField>();

        // Update max slider values based on number of options
        wallSlider.maxValue = wallColors.Length;
        floorSlider.maxValue = floorColors.Length;
    }

    public void ChangeWallColor()
    {
        // Update text number value
        wallField.text = wallSlider.value.ToString();

        // Update the color of all the walls
        Color wallColor = wallColors[(int)wallSlider.value - 1];

        for (int i = 0; i < wallColors.Length; i++)
        {
            walls[i].material.color = wallColor;
        }
    }

    public void ChangeFloorColor()
    {
        // Update text number value
        floorField.text = floorSlider.value.ToString();

        // Update the color of the floor
        Color floorColor = floorColors[(int)floorSlider.value - 1];

        floor.material.color = floorColor;
    }
}
