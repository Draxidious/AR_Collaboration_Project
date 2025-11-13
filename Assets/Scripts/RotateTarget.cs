using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem; // NEW INPUT SYSTEM

// Class for providing multiple perspectives of known targets.
public class RotateTarget : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The AR camera used for the scene.")]
    private Camera m_ARCamera;
    public Camera ARCamera { get { return m_ARCamera; } set { m_ARCamera = value; } }

    // Private variables for handling the rotations.
    private bool m_Rotating;
    private Vector2 m_RotatingFromScreenPos;
    private Quaternion m_InitialRotation;

    void Reset()
    {
        // Set the main camera as the AR camera.
        m_ARCamera = Camera.main;
        if (m_ARCamera == null)
        {
            Debug.LogWarning("[" + gameObject.name + "][RotateTarget]: Did not find a main camera to use as the AR Camera.");
        }
    }

    void Update()
    {
        if (m_ARCamera == null)
        {
            m_ARCamera = Camera.main;
            if (m_ARCamera == null)
            {
                Debug.LogError("[" + gameObject.name + "][RotateTarget]: Missing the AR Camera.");
                return;
            }
        }

        // Use the generic pointer (works for mouse in Editor + first touch on device)
        var pointer = Pointer.current;
        if (pointer == null)
            return;

        // BEGIN ROTATION
        if (pointer.press.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            // Raycast from pointer position
            Vector2 screenPos = pointer.position.ReadValue();
            Ray ray = m_ARCamera.ScreenPointToRay(screenPos);

            // Ensure ray can hit triggers if needed
            bool queriesHitTriggers = Physics.queriesHitTriggers;
            Physics.queriesHitTriggers = true;

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    m_Rotating = true;
                    m_RotatingFromScreenPos = screenPos;
                    m_InitialRotation = transform.rotation;
                }
            }

            Physics.queriesHitTriggers = queriesHitTriggers;
        }
        // END ROTATION
        else if (pointer.press.wasReleasedThisFrame)
        {
            m_Rotating = false;
        }
        // CONTINUE ROTATION
        else if (m_Rotating && pointer.press.isPressed)
        {
            Vector2 currentPos = pointer.position.ReadValue();
            Vector2 delta = currentPos - m_RotatingFromScreenPos;

            // Reset to initial rotation each frame, then reapply based on delta
            transform.rotation = m_InitialRotation;

            // Match the old script's behavior (note the signs)
            float deltaX = currentPos.x - m_RotatingFromScreenPos.x;
            float deltaY = currentPos.y - m_RotatingFromScreenPos.y;

            float yawDeg = (m_RotatingFromScreenPos.x - currentPos.x) / m_ARCamera.scaledPixelWidth * 360.0f;   // horizontal
            float pitchDeg = -(m_RotatingFromScreenPos.y - currentPos.y) / m_ARCamera.scaledPixelHeight * 360.0f; // vertical

            // Rotate horizontally relative to the camera (around camera up)
            transform.Rotate(m_ARCamera.transform.up, yawDeg, Space.World);
            // Rotate vertically relative to the camera (around camera right)
            transform.Rotate(m_ARCamera.transform.right, pitchDeg, Space.World);
        }
    }
}
