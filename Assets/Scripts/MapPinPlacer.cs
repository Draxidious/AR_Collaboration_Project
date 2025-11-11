using UnityEngine;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM
using Vuforia;

public class MapPinPlacer : MonoBehaviour
{
    [Header("References")]
    public Camera arCamera;
    public GameObject pinPrefab;
    public ImageTargetBehaviour mapTarget;

    void Update()
    {
        // Only allow pin placement if the map is currently tracked
        if (mapTarget.TargetStatus.Status != Status.TRACKED &&
            mapTarget.TargetStatus.Status != Status.EXTENDED_TRACKED)
            return;

        // If the user tapped/clicked this frame:
        if (WasScreenTapped())
        {
            print("Screen tapped - placing pin");
            RaycastFromScreen(Pointer.current.position.ReadValue());
        }
    }

    bool WasScreenTapped()
    {
        // Touch (mobile)
        if (Touchscreen.current != null)
        {
            if (Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
                return true;
        }

        // Mouse / Editor
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                return true;
        }

        return false;
    }

    void RaycastFromScreen(Vector2 screenPos)
    {
        Ray ray = arCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject pin = Instantiate(pinPrefab, hit.point, Quaternion.identity);

            // Parent to map so pin stays attached to the moving tracked target
            pin.transform.SetParent(mapTarget.transform, true);

            // Optional: make pin stand upright according to surface
            pin.transform.up = hit.normal;
        }
    }
}
