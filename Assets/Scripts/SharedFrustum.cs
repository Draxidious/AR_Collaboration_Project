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
using Photon.Pun;

// Class for sharing one's viewpoint or frustum with other users.
public class SharedFrustum : MonoBehaviour
{
    // The AR camera used for the scene.
    [SerializeField]
    [Tooltip("The AR camera used for the scene.")]
    private Camera m_ARCamera;
    public Camera ARCamera { get { return m_ARCamera; } set { m_ARCamera = value; } }

    // The PhotonView used to synchronize this object across clients.
    [SerializeField]
    [Tooltip("The PhotonView used to synchronize this object across clients.")]
    private PhotonView m_PhotonView;
    public PhotonView PhotonView { get { return m_PhotonView; } set { m_PhotonView = value; } }

    // The name of the GameObject that serves as the task space.
    [SerializeField]
    [Tooltip("The name of the GameObject that serves as the task space.")]
    private string m_TaskSpaceName;
    public string TaskSpaceName { get { return m_TaskSpaceName; } set { m_TaskSpaceName = value; } }

    // The GameObject that serves as the task space.
    private GameObject m_TaskSpace;
    public GameObject TaskSpace { get { return m_TaskSpace; } set { m_TaskSpace = value; } }

    // Reset function for initializing the class.
    void Reset()
    {
        // Set the main camera as the AR camera.
        m_ARCamera = Camera.main;
        // Provide a warning if a main camera was not found.
        if (m_ARCamera == null) { Debug.LogWarning("[" + gameObject.name + "][SharedFrustum]: Did not find a main camera to use as the AR Camera."); }

        // Attempt to find a local PhotonView.
        m_PhotonView = GetComponent<PhotonView>();
        // Provide a warning if a PhotonView was not found.
        if (m_PhotonView == null) { Debug.LogWarning("[" + gameObject.name + "][SharedFrustum]: Did not find a local PhotonView to use."); }
        // Provide a warning if a camera was found.
        else { Debug.LogWarning("[" + gameObject.name + "][SharedFrustum]: Found a local PhotonView to use."); }
    }

    // Start is called before the first frame update.
    void Start()
    {
        // Re-initialize the class.
        Reset();
    }

    // Update is called once per frame.
    void Update()
    {
        // If the class is properly configured.
        if (m_ARCamera != null && m_PhotonView != null)
        {
            // If the task space has not yet been retrieved.
            if (m_TaskSpace == null)
            {
                // Attempt to retrieve the task space.
                m_TaskSpace = GameObject.Find(m_TaskSpaceName);
            }
            // If the task space exists.
            if (m_TaskSpace != null)
            {
                // Make the local object a child of the task space.
                transform.parent = m_TaskSpace.transform;

                // If the local object is under local control (i.e., it was created locally).
                if (m_PhotonView.AmController)
                {
                    // Move the local object to where the local AR camera is.
                    transform.position = m_ARCamera.transform.position;
                    transform.rotation = m_ARCamera.transform.rotation;
                }
            }
            // Provide an error if the task space was not found.
            if (m_TaskSpace == null) { Debug.LogError("[" + gameObject.name + "][SharedFrustum]: Could not find a GameObject by the Task Space Name."); }
        }
        // Provide an error if the AR camera was not found.
        if (m_ARCamera == null) { Debug.LogError("[" + gameObject.name + "][SharedFrustum]: Missing the AR Camera."); }
        // Provide an error if the PhotonView was not found.
        if (m_PhotonView == null) { Debug.LogError("[" + gameObject.name + "][SharedFrustum]: Missing the Photon View."); }
    }
}
