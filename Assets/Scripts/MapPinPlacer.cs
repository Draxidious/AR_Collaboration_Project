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

        photonView = GetComponent<PhotonView>(); // PhotonView must be on the same GameObject
        if (photonView == null)
        {
            Debug.LogWarning("[MapPinPlacer] No PhotonView found. Pins will not sync.");
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
            // STEP 1 — Instantiate locally
            GameObject pin = Instantiate(pinPrefab, hit.point, Quaternion.identity);
            pin.transform.SetParent(parent.transform, true);
            pin.transform.up = hit.normal;

            // STEP 2 — Capture its LOCAL transform relative to parent
            Vector3 localPos = pin.transform.localPosition;
            Quaternion localRot = pin.transform.localRotation;
            Vector3 localScale = pin.transform.localScale; // important fix!

            // STEP 3 — Send transform to other clients
            if (PhotonNetwork.InRoom)
            {
                photonView.RPC(nameof(RPC_SpawnPinWithLocalTransform),
                               RpcTarget.OthersBuffered,
                               localPos, localRot, localScale);
            }
        }
    }

    [PunRPC]
    void RPC_SpawnPinWithLocalTransform(Vector3 localPos, Quaternion localRot, Vector3 localScale)
    {
        if (pinPrefab == null || parent == null)
            return;

        GameObject pin = Instantiate(pinPrefab);
        pin.transform.SetParent(parent.transform, false);

        pin.transform.localPosition = localPos;
        pin.transform.localRotation = localRot;
        pin.transform.localScale = localScale; // APPLY SCALE FIX
    }
}
