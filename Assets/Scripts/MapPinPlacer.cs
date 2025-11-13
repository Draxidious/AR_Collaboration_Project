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
    public GameObject parent;   // map root (so pins follow the map)

    private PhotonView photonView;

    void Awake()
    {
        if (arCamera == null)
            arCamera = Camera.main;

        photonView = GetComponent<PhotonView>();  // PhotonView is on ARCamera
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
        if (arCamera == null)
            return;

        Ray ray = arCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // No filter here, same as original script
            Vector3 worldPos = hit.point;
            Vector3 worldNormal = hit.normal;

            // If we're in a Photon room, sync to everyone; otherwise just spawn locally
            if (photonView != null && PhotonNetwork.InRoom)
            {
                photonView.RPC(nameof(RPC_SpawnPin), RpcTarget.AllBuffered, worldPos, worldNormal);
            }
            else
            {
                RPC_SpawnPin(worldPos, worldNormal);
            }
        }
    }

    [PunRPC]
    void RPC_SpawnPin(Vector3 worldPos, Vector3 worldNormal)
    {
        if (pinPrefab == null || parent == null)
            return;

        GameObject pin = Instantiate(pinPrefab, worldPos, Quaternion.identity);

        // Parent to map so pin stays attached to the moving tracked target
        pin.transform.SetParent(parent.transform, true);

        // Optional: make pin stand upright according to surface
        pin.transform.up = worldNormal;
    }
}
