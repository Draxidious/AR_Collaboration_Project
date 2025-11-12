/*
*   Copyright (C) 2021 University of Central Florida, created by Dr. Ryan P. McMahan.
*
*   This program is free software: you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.
*
*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU General Public License for more details.
*
*   You should have received a copy of the GNU General Public License
*   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*
*   Primary Author Contact:  Dr. Ryan P. McMahan <rpm@ucf.edu>
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
//using Photon.Pun;

// Class for providing multiple perspectives of known targets.
public class RotateTarget : MonoBehaviour
{
    // The AR camera used for the scene.
    [SerializeField]
    [Tooltip("The AR camera used for the scene.")]
    private Camera m_ARCamera;
    public Camera ARCamera { get { return m_ARCamera; } set { m_ARCamera = value; } }
    
    // The PhotonView used to synchronize this object across clients.
    //[SerializeField]
    //[Tooltip("The PhotonView used to synchronize this object across clients.")]
    //private PhotonView m_PhotonView;
    //public PhotonView PhotonView { get { return m_PhotonView; } set { m_PhotonView = value; } }
    
    // Private variables for handling the rotations.
    private bool m_Rotating;
    private float m_RotatingFromX;
    private float m_RotatingFromY;
    private Quaternion m_InitialRotation;

    // Reset function for initializing the class.
    void Reset()
    {
        // Set the main camera as the AR camera.
        m_ARCamera = Camera.main;
        // Provide a warning if a main camera was not found.
        if (m_ARCamera == null) { Debug.LogWarning("[" + gameObject.name + "][RotateTarget]: Did not find a main camera to use as the AR Camera."); }
        
        // Attempt to find a local PhotonView.
        //m_PhotonView = GetComponent<PhotonView>();
        // Provide a warning if a PhotonView was not found.
        //if (m_PhotonView == null) { Debug.LogWarning("[" + gameObject.name + "][RotateTarget]: Did not find a local PhotonView to use."); }
        // Provide a warning if a camera was found.
        else { Debug.LogWarning("[" + gameObject.name + "][RotateTarget]: Found a local PhotonView to use."); }
    }

    // Update is called once per frame.
    void Update()
    {
        // If the class is properly configured.
        if (m_ARCamera != null) // && m_PhotonView != null)
        {
            // Determine whether the left mouse button or touch input were pressed this frame.
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;
                // Track whether raycasts initially hit triggers.
                bool queriesHitTriggers = Physics.queriesHitTriggers;
                // Ensure raycasts hit triggers.
                Physics.queriesHitTriggers = true;
                // Track the screen raycast hit based on mouse position.
                RaycastHit hit;
                Ray ray = m_ARCamera.ScreenPointToRay(Input.mousePosition);
                // If something was hit.
                if (Physics.Raycast(ray, out hit))
                {
                    // Determine if the local transform was hit.
                    if (transform == hit.transform)
                    {
                        // Request control of the object.
                        //m_PhotonView.RequestOwnership();

                        // Track that the local object is being rotated and where the rotation starts from.
                        m_Rotating = true;
                        m_RotatingFromX = Input.mousePosition.x;
                        m_RotatingFromY = Input.mousePosition.y;
                        m_InitialRotation = transform.rotation;
                    }
                }
                // Reset whether queries hit triggers.
                Physics.queriesHitTriggers = queriesHitTriggers;
            }
            // Determine whether the left mouse button or touch input were released this frame.
            else if (Input.GetMouseButtonUp(0))
            {
                // Stop rotating.
                m_Rotating = false;
            }
            // Determine whether currently rotating.
            else if (m_Rotating)
            {
                // Position the object back to its original rotation.
                transform.rotation = m_InitialRotation;
                // Rotate the object horizontally relative to the camera.
                transform.Rotate(m_ARCamera.transform.up, (m_RotatingFromX - Input.mousePosition.x) / m_ARCamera.scaledPixelWidth * 360.0f, Space.World);
                // Rotate the object vertically relative to the camera.
                transform.Rotate(m_ARCamera.transform.right, -(m_RotatingFromY - Input.mousePosition.y) / m_ARCamera.scaledPixelHeight * 360.0f, Space.World);
            }
        }
        // Provide an error if the AR camera was not found.
        if (m_ARCamera == null) { Debug.LogError("[" + gameObject.name + "][RotateTarget]: Missing the AR Camera."); }
        // Provide an error if the PhotonView was not found.
        //if (m_PhotonView == null) { Debug.LogError("[" + gameObject.name + "][RotateTarget]: Missing the Photon View."); }
    }
}
