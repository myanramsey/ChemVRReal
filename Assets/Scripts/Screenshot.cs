using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.IO;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.UI;
using Unity.XR.CoreUtils;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using static UnityEngine.GraphicsBuffer;
using static System.Net.Mime.MediaTypeNames;

public class Screenshot : MonoBehaviour
{
    [SerializeField] private HapticImpulsePlayer haptics;

    [SerializeField] private InputActionProperty button;

    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private CanvasGroup screenshotNotification;

    [SerializeField] private GameObject screenshotMenu;
    [SerializeField] private GameObject captureField;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private RawImage rawImage;

    private Texture2D texture;

    [Header("Haptic Settings")]
    [Range(0, 1)] public float intensity = 0f;
    public float duration = 0f;

    [Header("Animation Settings")]
    public float fadeDuration = 1f;
    public float waitDuration = 1f;

    private bool isOpen = false;
    private byte[] png;

    private ContinuousMoveProvider movement;
    private ContinuousTurnProvider turning;

    private float height;
    private float xRot;
    private float zRot;

    private void Start()
    {
        movement = FindAnyObjectByType<ContinuousMoveProvider>();
        turning = FindAnyObjectByType<ContinuousTurnProvider>();

        height = screenshotMenu.transform.position.y;
        xRot = screenshotMenu.transform.rotation.eulerAngles.x;
        zRot = screenshotMenu.transform.rotation.eulerAngles.z;

        texture = new Texture2D(renderTexture.width, renderTexture.height);
    }

    private void OnEnable()
    {
        button.action.Enable();
    }

    private void OnDisable()
    {
        button.action.Disable();
    }

    private void Update()
    {
        if (isOpen) return;

        if (button.action.IsPressed())
        {
            // Show capture field
            captureField.SetActive(true);
        }

        if (!button.action.WasReleasedThisFrame()) return;

        StartCoroutine(ScreenshotRoutine());

        // Fade animation
        //StartCoroutine(FadeSequence());
    }

    IEnumerator ScreenshotRoutine()
    {
        // Remove capture field
        captureField.SetActive(false);

        yield return new WaitUntil(() => !captureField.activeInHierarchy);
        yield return new WaitForEndOfFrame();

        // Capture screenshot and save preview of image from render texture
        yield return StartCoroutine(TakeScreenshot());

        // Haptics to let player know screenshot was taken
        TriggerHaptic();

        // Spawn screenshot menu in front and facing player
        Transform vrPlayer = xrOrigin.Camera.transform;

        Vector3 targetPos = vrPlayer.position + (vrPlayer.forward * 1.5f);
        targetPos.y = height;
        screenshotMenu.transform.position = targetPos;

        Quaternion targetRot = Quaternion.LookRotation(vrPlayer.forward);
        screenshotMenu.transform.rotation = Quaternion.Euler(xRot, targetRot.eulerAngles.y, zRot);

        // Open save screenshot menu
        screenshotMenu.SetActive(true);

        isOpen = true;
    }

    IEnumerator TakeScreenshot()
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        png = texture.EncodeToPNG();
        Texture2D decodedTexture = new Texture2D(renderTexture.width, renderTexture.height);
        decodedTexture.LoadImage(png);
        rawImage.texture = decodedTexture;

        RenderTexture.active = previous;

        yield return null;
    }

    public void SavePNG()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        // Save to Android's public Pictures directory so it shows in Quest's files
        string filename = "Screenshot-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";

        NativeGallery.SaveImageToGallery(png, "ChemVR Screenshots", filename, (success, path) =>
        {
            if (success) Debug.Log("Screenshot saved successfully to: " + path);
            else Debug.Log("Failed to save screenshot.");
        });
#endif
    }

    private void TriggerHaptic()
    {
        haptics.SendHapticImpulse(intensity, duration);
    }

    public void CloseMenu()
    {
        screenshotMenu.SetActive(false);
        isOpen = false;
    }

    private IEnumerator Fade(CanvasGroup cg, float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure float value fully reaches end alpha
        if (endAlpha == 0f)
        {
            cg.alpha = 0f;
        }
        else if (endAlpha == 1f)
        {
            cg.alpha = 1f;
        }
    }

    private IEnumerator FadeSequence()
    {
        // Screenshot notification gradually appears
        yield return StartCoroutine(Fade(screenshotNotification, 0f, 1f, fadeDuration));

        // Duration that screenshot notifcation is fully visible
        yield return new WaitForSeconds(waitDuration);

        // Screenshot notification gradually disappears
        yield return StartCoroutine(Fade(screenshotNotification, 1f, 0f, fadeDuration));
    }
}
