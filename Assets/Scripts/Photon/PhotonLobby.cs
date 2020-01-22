using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonLobby : MonoBehaviourPunCallbacks
{
    public static PhotonLobby instance { get { return s_Instance; } }
    private static PhotonLobby s_Instance;

    void Awake()
    {
        s_Instance = this;
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        LoadoutState.instance.SetViewState(0, true);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Player Has Connected");

        PhotonNetwork.AutomaticallySyncScene = true;
        
        LoadoutState.instance.SetViewState(0, false);
        LoadoutState.instance.SetViewState(1, true);
    }

    public void OnCreateRoom(string text)
    {
        if(text.Length >= 1)
            PhotonNetwork.CreateRoom(text, new RoomOptions() { MaxPlayers = 2}, null);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        LoadoutState.instance.SetCreateMessage(message);
    }

    public void OnJoinRoom(string text)
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.JoinOrCreateRoom(text, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        LoadoutState.instance.SetViewState(0, false);
        LoadoutState.instance.SetViewState(1, false);

        Debug.Log("On Joined Lobby");
    }

    // public override void OnJoinedRoom()
    // {
    //     Debug.Log("We are connected to the room!");
    //     // GameState.instance.Enter();
    //     PhotonRoom.instance.JoinRoom();
    // }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        LoadoutState.instance.SetJoinMessage(message);
    }

    public void OnCancelOrLeave()
    {
        PhotonNetwork.LeaveRoom();
    }
}
