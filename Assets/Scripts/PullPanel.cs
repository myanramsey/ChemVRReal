using GLTFast.Schema;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PullPanel : MonoBehaviour
{
    [SerializeField] private Transform vrPlayer;

    [Header("Movement Settings")]
    [Tooltip("The distance from the player the UI panel will be when fully pulled.")]
    public float distanceFromPlayer = 1.0f;
    [Tooltip("How quickly the UI panel is pulled.")]
    public float moveSpeed = 5.0f;
    [Tooltip("The minimum distance the player must be from the UI panel to pull it.")]
    public float minDistance = 2.0f;

    private bool isMoving = false;

    private float height;
    private float xRot;
    private float zRot;

    private void Start()
    {
        height = transform.position.y;
        xRot = transform.rotation.eulerAngles.x;
        zRot = transform.rotation.eulerAngles.z;
    }

    public void EnablePull()
    {
        float distance = Vector3.Distance(transform.position, vrPlayer.position);
        if (distance <= minDistance) return;
        
        isMoving = true;
    }

    public void DisablePull() 
    {
        isMoving = false;
    }

    private void Update()
    {
        if (!isMoving) return;

        // Moves UI panel towards player and facing player 
        Vector3 targetPos = vrPlayer.position + (vrPlayer.forward * distanceFromPlayer);
        targetPos.y = height;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);

        Quaternion targetRot = Quaternion.LookRotation(transform.position - vrPlayer.position);
        transform.rotation = Quaternion.Euler(xRot, targetRot.eulerAngles.y, zRot);
    }
}

/*
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRUIRayDrag : MonoBehaviour
{
    public XRRayInteractor rayInteractor;

    private bool isDragging;

    private Vector3 offset;
    private float distanceToCamera;

    void Update()
    {
        if (!rayInteractor) return;

        // Trigger pressed = start/end drag
        if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            if (rayInteractor.selectAction.action.WasPressedThisFrame())
            {
                StartDrag(hit);
            }

            if (isDragging)
            {
                UpdateDrag();
            }

            if (rayInteractor.selectAction.action.WasReleasedThisFrame())
            {
                isDragging = false;
            }
        }
    }

    void StartDrag(RaycastHit hit)
    {
        isDragging = true;

        distanceToCamera = Vector3.Distance(
            rayInteractor.transform.position,
            transform.position
        );

        offset = transform.position - hit.point;
    }

    void UpdateDrag()
    {
        Ray ray = new Ray(rayInteractor.transform.position,
                          rayInteractor.transform.forward);

        Vector3 targetPoint = ray.GetPoint(distanceToCamera);

        transform.position = targetPoint + offset;
    }
}

Smooth follow:
transform.position = Vector3.Lerp(
    transform.position,
    targetPoint + offset,
    20f * Time.deltaTime
);
*/
