using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.IO;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class Screenshot : MonoBehaviour
{
    [SerializeField] private HapticImpulsePlayer haptics;

    [SerializeField] private InputActionProperty button;

    [SerializeField] private CanvasGroup screenshotNotifcation;

    [Header("Haptic Settings")]
    [Range(0, 1)] public float intensity = 0f;
    public float duration = 0f;

    [Header("Animation Settings")]
    public float fadeDuration = 1f;
    public float waitDuration = 1f;

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
        if (!button.action.WasPressedThisFrame()) return;

        // Capture screenshot
        StartCoroutine(TakeScreenshot());
       
        // Haptics to let player know screenshot was taken
        TriggerHaptic();

        // Fade animation
        StartCoroutine(FadeSequence());
    }

    IEnumerator TakeScreenshot()
    {
        yield return new WaitForEndOfFrame();

#if UNITY_EDITOR
        ScreenCapture.CaptureScreenshot("Screenshot-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
#endif

#if !UNITY_EDITOR && UNITY_ANDROID
        SavePNG();
#endif
    }

    private void SavePNG()
    {
        Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] png = tex.EncodeToPNG();
        Destroy(tex);

        // Save to Android's public Pictures directory so it shows in Quest's files
        string filename = "Screenshot-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";

        NativeGallery.SaveImageToGallery(png, "ChemVR Screenshots", filename, (success, path) =>
        {
            if (success) Debug.Log("Screenshot saved successfully to: " + path);
            else Debug.Log("Failed to save screenshot.");
        });
    }

    private void TriggerHaptic()
    {
        haptics.SendHapticImpulse(intensity, duration);
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
        yield return StartCoroutine(Fade(screenshotNotifcation, 0f, 1f, fadeDuration));

        // Duration that screenshot notifcation is fully visible
        yield return new WaitForSeconds(waitDuration);

        // Screenshot notification gradually disappears
        yield return StartCoroutine(Fade(screenshotNotifcation, 1f, 0f, fadeDuration));
    }
}
