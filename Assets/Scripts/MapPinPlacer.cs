using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM
using Vuforia;
using Photon.Pun;

public class MapPinPlacer : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera;
    public GameObject pinPrefab;
    public ImageTargetBehaviour mapTarget;
    public GameObject parent;   // map root so pins move with map

    private PhotonView photonView;

    void Awake()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (parent == null)
            Debug.LogError("[MapPinPlacer] Parent (map root) is not assigned.");

        photonView = GetComponent<PhotonView>();  // PhotonView on ARCamera
        if (photonView == null)
        {
            Debug.LogWarning("[MapPinPlacer] No PhotonView on ARCamera. " +
                             "Pins will still work locally but won't sync.");
        }
    }

    void Update()
    {
        // Only allow pin placement if the map is currently tracked
        if (mapTarget != null &&
            mapTarget.TargetStatus.Status != Status.TRACKED &&
            mapTarget.TargetStatus.Status != Status.EXTENDED_TRACKED)
            return;

        if (Pointer.current == null)
            return;

        // If the user tapped/clicked this frame:
        if (WasScreenTapped())
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 screenPos = Pointer.current.position.ReadValue();
            RaycastFromScreen(screenPos);
        }
    }

    bool WasScreenTapped()
    {
        // Touch (mobile)
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        // Mouse / Editor
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        return false;
    }

    void RaycastFromScreen(Vector2 screenPos)
    {
        if (arCamera == null || parent == null)
            return;

        Ray ray = arCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            Vector3 worldPos = hit.point;
            Vector3 worldNormal = hit.normal;

            // Convert to map-local coordinates before sending
            Vector3 localPos = parent.transform.InverseTransformPoint(worldPos);
            Vector3 localNormal = parent.transform.InverseTransformDirection(worldNormal);

            if (photonView != null && PhotonNetwork.InRoom)
            {
                // Send LOCAL coords over network
                photonView.RPC(nameof(RPC_SpawnPin), RpcTarget.AllBuffered, localPos, localNormal);
            }
            else
            {
                // Single-player / offline  still works
                RPC_SpawnPin(localPos, localNormal);
            }
        }
    }

    [PunRPC]
    void RPC_SpawnPin(Vector3 localPos, Vector3 localNormal)
    {
        if (pinPrefab == null || parent == null)
            return;

        // Instantiate as child of the map parent
        GameObject pin = Instantiate(pinPrefab, parent.transform);

        // Use localPosition so it matches map-local space
        pin.transform.localPosition = localPos;

        // Reconstruct world-space orientation from local normal
        Vector3 worldNormal = parent.transform.TransformDirection(localNormal);
        pin.transform.up = worldNormal;
    }
}
