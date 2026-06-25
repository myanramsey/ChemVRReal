using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Experimental.GraphView.Port;

public class ColorPickerControl : MonoBehaviour
{
    private float currentHue, currentSat, currentVal = 0f;
    private float currentOpacity = 1f;

    private Texture2D hueTexture, svTexture, outTexture, opacityTexture;

    [SerializeField]
    private RawImage hueImage, svImage, outImage, opacityImage;

    [SerializeField]
    private Slider hueSlider, opacitySlider;

    [SerializeField]
    private TMP_InputField hexInputField;

    private Color startingColor, startingColor2, currentColor;

    private GameObject molecule;

    private MeshRenderer meshRenderer;
    private MeshRenderer meshRenderer2;

    private void OnEnable()
    {
        meshRenderer = null;
        meshRenderer2 = null;

        CreateHueImage();
        CreateOpacityImage();
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

        hueImage.texture = hueTexture;
    }

    private void CreateOpacityImage()
    {
        opacityTexture = new Texture2D(1, 16);
        opacityTexture.wrapMode = TextureWrapMode.Clamp;
        opacityTexture.name = "OpacityTexture";

        Color opacity = currentColor;

        // Iterate through pixels of texture and set pixel opacity based on height
        for (int i = 0; i < opacityTexture.height; i++)
        {
            opacity.a = 1.0f - ((float)i / opacityTexture.height);
            opacityTexture.SetPixel(0, i, opacity);
        }
        opacityTexture.Apply();

        opacityImage.texture = opacityTexture;
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
        currentColor = Color.HSVToRGB(currentHue, currentSat, currentVal);
        Color opacity = currentColor;
        currentColor.a = 1.0f - opacitySlider.value;

        for (int i = 0; i < outTexture.height; i++)
        {
            opacity.a = 1.0f - ((float)i / opacityTexture.height);
            opacityTexture.SetPixel(0, i, opacity);
            outTexture.SetPixel(0, i, currentColor);
        }
        opacityTexture.Apply();
        outTexture.Apply();

        hexInputField.text = ColorUtility.ToHtmlStringRGB(currentColor);

        if (meshRenderer != null)
        {
            meshRenderer.material.color = currentColor;
        }
        if (meshRenderer2 != null)
        {
            meshRenderer2.material.color = currentColor;
        }
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

    // Closes color picker menu and returns GameObject to starting color
    public void Back()
    {
        molecule.GetComponent<Outline>().enabled = false;

        if (meshRenderer == null)
        {
            gameObject.SetActive(false);
            return;
        }

        meshRenderer.gameObject.GetComponent<Outline>().enabled = false;
        meshRenderer.material.color = startingColor;
        if (meshRenderer2 != null)
        {
            meshRenderer2.material.color = startingColor2;
            meshRenderer2.gameObject.GetComponent<Outline>().enabled = false;
        }

        gameObject.SetActive(false);
    }

    // Closes color picker menu and leaves GameObject as confirmed color
    public void Confirm()
    {
        if (meshRenderer == null) return;

        molecule.GetComponent<Outline>().enabled = false;

        meshRenderer.gameObject.GetComponent<Outline>().enabled = false;
        if (meshRenderer2 != null)
        {
            meshRenderer2.gameObject.GetComponent<Outline>().enabled = false;
        }

        gameObject.SetActive(false);
    }

    // Sets reference to molecule GameObject whose color/opacity will be changed
    public void SetGameObject(GameObject gameObject)
    {
        molecule = gameObject;
    }

    // Sets reference to MeshRenderer of part of molecule whose color/opacity will be changed
    public void SetMeshRenderer(MeshRenderer mr, MeshRenderer mr2)
    {
        meshRenderer = mr;
        meshRenderer2 = mr2;

        startingColor = meshRenderer.material.color;
        if (meshRenderer2 != null)
        {
            startingColor2 = meshRenderer2.material.color;
        }
    }
}
