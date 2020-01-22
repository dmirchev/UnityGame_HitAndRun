using System.IO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    //Room info
    public static PhotonRoom instance { get { return s_Instance; } }
    protected static PhotonRoom s_Instance;
    private PhotonView PV;

    //Player info
    public int maxPlayers;
    Player[] photonPlayers;
    public int playersInRoom;
    public int myNumberInRoom;

    //Delayed Start
    public float timeToStart;

    public bool execute;

    int seed;

    void Awake()
    {
        if(PhotonRoom.instance == null)
        {
            PhotonRoom.s_Instance = this;
        }
        else
        {
            if(PhotonRoom.instance != this)
            {
                Destroy(PhotonRoom.instance.gameObject);
                PhotonRoom.s_Instance = this;
            }
        }
        DontDestroyOnLoad(this.gameObject);

        PV = GetComponent<PhotonView>();

        execute = false;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        //Scene
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        //Scene
    }

    void Start()
    {
        timeToStart = 15;
        maxPlayers = 2;
    }

    void Update()
    {
        if(!execute)
            return;

        if(GetPlayersCount() == 2)
        {
            GameState.instance.Enter();
            execute = false;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("We are connected to the room!");

        execute = true;

        LoadoutState.instance.SetViewState(0, false);
        LoadoutState.instance.SetViewState(1, false);

        if(PhotonNetwork.IsMasterClient)
        {
            int s = Random.Range(0, 10);
            PV.RPC("SetSeed", RpcTarget.AllBuffered, s);
        }

        GameState.instance.SetViewState(0, true);

        OtherJoin();
    }

    [PunRPC]
    void SetSeed(int s)
    {
        Random.InitState(s);
    }

    public void OtherJoin()
    {
        PhotonNetwork.Instantiate(Path.Combine("PhotonNetworkPlayer"), Vector3.zero.With(y: playersInRoom * 2, z: Random.Range(-5, -6)), Quaternion.identity, 0);
    }

    public int GetPlayersCount()
    {
        return PhotonNetwork.PlayerList.Length;
    }
}
