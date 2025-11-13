using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonConnector : MonoBehaviourPunCallbacks
{
    [Header("Photon Settings")]
    public string gameVersion = "1.0";
    public string roomName = "AR_Map_Room";

    void Start()
    {
        // Make sure we sync scene state between players if you ever need it
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("[PhotonConnector] Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PhotonConnector] Connected to Master. Joining/Creating room...");

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 4
        };

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[PhotonConnector] Joined room: " + roomName);
        // From this point on both users are in the same Photon room
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("[PhotonConnector] Disconnected from Photon: " + cause.ToString());
    }
}
