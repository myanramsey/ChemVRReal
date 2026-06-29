using UnityEngine;
using Unity.XR.CoreUtils;
using System.Collections;

public class XROriginAligner : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] Transform menuTarget;
    [SerializeField] private bool alignToMenuOnStart = true;

    void Start()
    {
       if (alignToMenuOnStart)
        {
            StartCoroutine(AlignRoutine());
        }
    }

    IEnumerator AlignRoutine()
    {
        // Wait for XR tracking to fully initialize 
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);

        AlignToMenu();
    }

    private void AlignToMenu()
    {
        if (xrOrigin == null || menuTarget == null) return;

        Transform camera = xrOrigin.Camera.transform;

        // Get forward direction of headset
        Vector3 headsetForward = camera.forward;
        headsetForward.y = 0;
        headsetForward.Normalize();

        // Get the direction towards the menu or desired look direction
        Vector3 toMenu = menuTarget.position - camera.position;
        toMenu.y = 0;
        toMenu.Normalize();

        // Calculation rotation to face menu and apply to XR origin
        Quaternion rotationOffset = Quaternion.FromToRotation(headsetForward, toMenu);
        xrOrigin.transform.rotation = rotationOffset * xrOrigin.transform.rotation;
    }
}
