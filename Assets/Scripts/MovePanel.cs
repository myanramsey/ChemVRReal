using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class MovePanel : MonoBehaviour
{
    [SerializeField] private XROrigin xrOrigin;
    [SerializeField] private XRRayInteractor leftRayInteractor;
    [SerializeField] private XRRayInteractor rightRayInteractor;
    [SerializeField] private InputActionProperty leftGripButton;
    [SerializeField] private InputActionProperty rightGripButton;

    [Header("Movement Settings")]
    [Tooltip("The distance from the player the UI panel will be when fully pulled.")]
    public float distanceFromPlayer = 1.0f;
    [Tooltip("How quickly the UI panel is pulled.")]
    public float moveSpeed = 5.0f;
    [Tooltip("The minimum distance the player must be from the UI panel to pull it.")]
    public float minDistance = 2.0f;

    private bool isPulling = false;
    private bool isDragging = false;

    private float height;
    private float xRot;
    private float zRot;

    private float distance = -1.0f;
    private Vector3 grabOffset = Vector3.zero;

    private void OnEnable()
    {
        leftGripButton.action.Enable();
        rightGripButton.action.Enable();
    }

    private void OnDisable()
    {
        leftGripButton.action.Disable();
        rightGripButton.action.Disable();
    }

    private void Start()
    {
        height = transform.position.y;
        xRot = transform.rotation.eulerAngles.x;
        zRot = transform.rotation.eulerAngles.z;
    }

    public void EnablePullOrDrag()
    {
        Transform vrPlayer = xrOrigin.Camera.transform;

        float distanceFromPlayer = Vector3.Distance(transform.position, vrPlayer.position);
        if (distanceFromPlayer < minDistance)
        {
            isDragging = true;
        }
        else
        {
            isPulling = true;
        }
    }

    public void DisablePullOrDrag() 
    {
        if (isPulling)
        {
            isPulling = false;
        }
        else if (isDragging)
        {
            isDragging = false;
            distance = -1.0f;
            grabOffset = Vector3.zero;
        }
    }

    private void Update()
    {
        XRRayInteractor activeRay = null;
        if (leftGripButton.action.IsPressed())
        {
            activeRay = leftRayInteractor;
        }
        else if (rightGripButton.action.IsPressed())
        { 
            activeRay = rightRayInteractor;
        }

        if (activeRay == null)
        {
            DisablePullOrDrag();
            return;
        }

        if (!TryGetRaycastHit(activeRay, out RaycastHit raycastHit)) return;

        if (!isDragging)
        {
            // Check to see if the hit GameObject is this GameObject
            GameObject hit = raycastHit.collider?.gameObject;
            if (hit != gameObject) return;
        }

        if (!isPulling && !isDragging)
        {
            EnablePullOrDrag();
        }
        
        if (isPulling)
        {
            PullPanel();
        }
        else if (isDragging)
        {
            DragPanel(activeRay, raycastHit);
        }
    }

    private bool TryGetRaycastHit(XRRayInteractor ray, out RaycastHit hit)
    {
        ray.TryGetCurrentRaycast(out RaycastHit? raycastHit, out _, out _, out _, out bool isUIHit);
        if (!isUIHit && raycastHit.HasValue)
        {
            hit = raycastHit.Value;
            return true;
        }

        hit = default;
        return false;
    }

    // Pulls UI panel from a distance towards player and facing player 
    private void PullPanel()
    {
        Transform vrPlayer = xrOrigin.Camera.transform;

        Vector3 targetPos = vrPlayer.position + (vrPlayer.forward * distanceFromPlayer);
        targetPos.y = height;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * moveSpeed);

        Quaternion targetRot = Quaternion.LookRotation(transform.position - vrPlayer.position);
        transform.rotation = Quaternion.Euler(xRot, targetRot.eulerAngles.y, zRot);
    }

    // Drags UI panel around with player and facing player
    private void DragPanel(XRRayInteractor activeRay, RaycastHit raycastHit)
    {
        Ray ray = new Ray(activeRay.transform.position, activeRay.transform.forward);
        if (distance == -1.0f)
        {
            distance = raycastHit.distance;
        }
        if (grabOffset == Vector3.zero)
        {
            grabOffset = transform.position - raycastHit.point;
        }
        Transform vrPlayer = xrOrigin.Camera.transform;

        Vector3 targetPoint = ray.GetPoint(distance) + grabOffset;
        transform.position = Vector3.Lerp(transform.position, targetPoint, Time.deltaTime * moveSpeed);

        Quaternion targetRot = Quaternion.LookRotation(transform.position - vrPlayer.position);
        transform.rotation = Quaternion.Euler(xRot, targetRot.eulerAngles.y, zRot);
    }
}

