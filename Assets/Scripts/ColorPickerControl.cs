using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ColorPickerControl : MonoBehaviour
{
    public float currentHue, currentSat, currentVal;

    private Texture2D hueTexture, svTexture, outTexture;

    [SerializeField]
    private RawImage hueImage, svImage, outImage;

    [SerializeField]
    private Slider hueSlider;

    [SerializeField]
    private TMP_InputField hexInputField;

    [SerializeField]
    MeshRenderer meshRenderer;

    private void Start()
    {
        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();
        UpdateObjectColor();
    }

    private void CreateHueImage()
    {
        hueTexture = new Texture2D(1, 16);
        hueTexture.wrapMode= TextureWrapMode.Clamp;
        hueTexture.name = "HueTexture";

        // Iterate through pixels of texture and set pixel color based on height
        for (int i = 0; i < hueTexture.height; i++)
        {
            hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1, 1.0f));
        }
        hueTexture.Apply();
        currentHue = 0;

        hueImage.texture = hueTexture;
    }

    private void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "SVTexture";

        // Iterate through pixels of texture and set pixel color based on width and height
        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }
        svTexture.Apply();
        currentSat = 0;
        currentVal = 0;

        svImage.texture = svTexture;
    }

    private void CreateOutputImage()
    {
        outTexture = new Texture2D(1, 16);
        outTexture.wrapMode = TextureWrapMode.Clamp;
        outTexture.name = "OutputTexture";

        Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);

        // Iterate through pixels of texture and set each pixel to currently selected color
        for (int i = 0; i < outTexture.height; i++)
        {
            outTexture.SetPixel(0, i, currentColor);
        }
        outTexture.Apply();

        outImage.texture = outTexture;
    }

    // Update GameObject's mesh renderer with selected color
    private void UpdateObjectColor()
    {
        Color currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);
        
        for (int i = 0; i < outTexture.height; i++)
        {
            outTexture.SetPixel(0, i, currentColor);
        }
        outTexture.Apply();

        hexInputField.text = ColorUtility.ToHtmlStringRGB(currentColor);

        meshRenderer.material.color = currentColor;
    }

    // Update the SV panel whenever saturation and/or value are changed
    public void SetSV(float s, float v)
    {
        currentSat = s;
        currentVal = v;

        UpdateObjectColor();
    }

    // Update the SV panel whenever the hue is changed
    public void UpdateSVImage()
    {
        currentHue = hueSlider.value;

        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.width; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }
        svTexture.Apply();

        UpdateObjectColor();
    }

    // Hex field usage
    public void OnTextInput()
    {
        if (hexInputField.text.Length < 6) return;

        Color newColor;
        if (ColorUtility.TryParseHtmlString("#" + hexInputField.text, out newColor))
        {
            Color.RGBToHSV(newColor, out currentHue, out currentSat, out currentVal);
        }

        hueSlider.value = currentHue;

        hexInputField.text = "";

        UpdateObjectColor();
    }
}
