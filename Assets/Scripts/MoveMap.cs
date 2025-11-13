using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM
using Photon.Pun;             

public class MoveMap : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    [Tooltip("The AR camera used for the scene.")]
    private Camera arCamera;

    [Header("Movement Settings")]
    [Tooltip("How fast the map moves when you drag on it.")]
    public float dragSpeed = 0.002f;

    [Tooltip("How fast the map moves towards/away from the camera during pinch.")]
    public float zoomSpeed = 0.01f;

    // Photon
    private PhotonView photonView;   

    // Internal state
    private bool isDragging = false;
    private Vector2 dragStartScreenPos;
    private Vector3 dragStartWorldPos;

    // For pinch front/back movement
    private bool isPinching = false;
    private float initialPinchDistance;
    private Vector3 initialMapPosition;

    private void Awake()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        photonView = GetComponent<PhotonView>();
        if (photonView == null)
        {
            Debug.LogError("[" + gameObject.name + "][MoveMap]: No PhotonView on this object! Add one to sync movement.");
        }
    }

    private void Reset()
    {
        arCamera = Camera.main;
        if (arCamera == null)
        {
            Debug.LogWarning("[" + gameObject.name + "][MoveMap]: No AR camera assigned, using Camera.main.");
        }
    }

    private void Update()
    {
        // Only the current owner should drive movement
        if (photonView != null && !photonView.IsMine)
            return;

        if (arCamera == null)
        {
            Debug.LogError("[" + gameObject.name + "][MoveMap]: Missing AR camera reference.");
            return;
        }

        HandleOneFingerDrag();
        HandlePinchFrontBack();
    }

    private void HandleOneFingerDrag()
    {
        // Use the generic Pointer from the new Input System
        if (Pointer.current == null)
            return;

        var pointer = Pointer.current;

        // Begin drag
        if (pointer.press.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = arCamera.ScreenPointToRay(pointer.position.ReadValue());

            // Make sure we only start dragging when the ray actually hits THIS map object
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    //grab ownership when starting drag
                    if (photonView != null)
                        photonView.RequestOwnership();

                    isDragging = true;
                    dragStartScreenPos = pointer.position.ReadValue();
                    dragStartWorldPos = transform.position;
                }
            }
        }
        // End drag
        else if (pointer.press.wasReleasedThisFrame)
        {
            isDragging = false;
        }
        // While dragging
        else if (isDragging && pointer.press.isPressed)
        {
            Vector2 currentPos = pointer.position.ReadValue();
            Vector2 delta = currentPos - dragStartScreenPos;

            // Move relative to camera's orientation
            Vector3 right = arCamera.transform.right; // left/right
            Vector3 up = arCamera.transform.up;       // up/down

            // delta.x controls move along camera right, delta.y along camera up
            Vector3 translation =
                (right * delta.x + up * delta.y) * dragSpeed;

            transform.position = dragStartWorldPos + translation;
        }
    }

    private void HandlePinchFrontBack()
    {
        // Touchscreen required for pinch
        if (Touchscreen.current == null)
        {
            isPinching = false;
            return;
        }

        var touches = Touchscreen.current.touches;

        // Need at least two touches for pinch
        if (touches.Count < 2 ||
            !touches[0].press.isPressed ||
            !touches[1].press.isPressed)
        {
            isPinching = false;
            return;
        }

        // Read positions
        Vector2 p0 = touches[0].position.ReadValue();
        Vector2 p1 = touches[1].position.ReadValue();

        float currentDistance = Vector2.Distance(p0, p1);

        if (!isPinching)
        {
            // also grab ownership when starting a pinch
            if (photonView != null)
                photonView.RequestOwnership();

            isPinching = true;
            initialPinchDistance = currentDistance;
            initialMapPosition = transform.position;
        }
        else
        {
            float distanceDelta = currentDistance - initialPinchDistance;

            // Move map along camera forward/backward
            Vector3 forward = arCamera.transform.forward;
            Vector3 translation = forward * (distanceDelta * zoomSpeed);

            transform.position = initialMapPosition + translation;
        }
    }
}
