using UnityEngine;
using UnityEngine.InputSystem;
using System;
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
#if UNITY_EDITOR
        ScreenCapture.CaptureScreenshot("Screenshot-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
#endif

        yield return new WaitForEndOfFrame();

        Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] png = tex.EncodeToPNG();
        Debug.Log($"PNG size = {png.Length}");
        SavePNG(png);
        Destroy(tex);
    }

    private void SavePNG(byte[] pngBytes)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass mediaStore = new AndroidJavaClass("android.provider.MediaStore$Images");
        AndroidJavaObject collection = mediaStore.CallStatic<AndroidJavaObject>("getContentUri", "external");

        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        using var resolver = activity.Call<AndroidJavaObject>("getContentResolver");

        using var values = new AndroidJavaObject("android.content.ContentValues");

        string filename = $"Screenshot-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.png";

        values.Call("put", "display_name", filename);
        values.Call("put", "mime_type", "image/png");
        values.Call("put", "relative_path", "DCIM/ChemVR");
        values.Call("put", "is_pending", 1);

        AndroidJavaObject uri = resolver.Call<AndroidJavaObject>("insert", collection, values);

        if (uri == null)
        {
            Debug.LogError("MediaStore insert failed");
            return;
        }

        using (AndroidJavaObject stream = resolver.Call<AndroidJavaObject>("openOutputStream", uri))
        {
            stream.Call("write", pngBytes, 0, pngBytes.Length);
            stream.Call("flush");
            stream.Call("close");
        }

        values.Call("put", "is_pending", 0);
        resolver.Call("update", uri, values, null, null);
#endif
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
