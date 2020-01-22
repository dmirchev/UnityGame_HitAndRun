using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour, IPunObservable
{
    public static GameState instance { get { return s_Instance; } }
    protected static GameState s_Instance;

    public PhotonView PV;
 
    public GameObject[] views;
    public Text seedText;
    public Button boostButton;

    public int pointsLength;
    public int obstacleCount;
    public int currentObstaclePoint;
    public Vector2[] obstacleSize;
    public MeshFilter[] obstacle;
    public MeshCollider[] obstacleColliders;

    public bool execute;

    void Awake()
    {
        Initialize();

        execute = false;
    }

    void Initialize()
    {
        s_Instance = this;
    }

    public void SetViewState(int index, bool state)
    {
        views[index].SetActive(state);
    }

    public void GetBoost()
    {
        int playersCount = this.transform.childCount;

        Debug.Log(playersCount);
        for (int i = 0; i < playersCount; i++)
        {
            Debug.Log(this.transform.GetChild(i).name, this.transform.GetChild(i).gameObject);
            this.transform.GetChild(i).GetComponent<PlayerMovement>().Boost();
        }
    }

    public void SetSeedText(string sd)
    {
        seedText.text = sd;
    }

    public void Enter()
    {
        SetViewState(0, false);
        SetViewState(1, true);

        Debug.Log("ENTER GAME");

        Init();

        // PV.RPC("SpawnNewObstacle", RpcTarget.All);
    }

    public void SpawnNewObstacleFromPlayer()
    {
        if(execute)
        {
            int startIndex = Random.Range(currentObstaclePoint, pointsLength);
            int obstacleLength = Random.Range(6, 10);
            bool side = Random.Range(0 , 2) == 0 ? true : false; 

            PV.RPC("SpawnNewObstacle", RpcTarget.All, new object[]{
                startIndex, obstacleLength, side
            });
        }
    }

    [PunRPC]
    void SpawnNewObstacle(int startIndex, int obstacleLength, bool side)
    {
        CreateObstacle(startIndex, obstacleLength, side);
    }

    public void Init()
    {
        execute = true;

        pointsLength = RoadCreator.instance.evenPoints.Length - 10;
        obstacleCount = -1;
        currentObstaclePoint = 10;

        obstacleSize = new Vector2[obstacle.Length];
    }

    void Update()
    {
        // testText.text = PhotonRoom.instance.GetPlayersCount();
    }

    public void GetNextObstacleCount()
    {
        obstacleCount++;

        if(obstacleCount > 1)
            obstacleCount = 0;
    }

    public void CreateObstacle(int startIndex, int obstacleLength, bool side)//(int length, float edge)
    {
        GetNextObstacleCount();

        currentObstaclePoint += obstacleLength + 10;

        // If We Have Spawend To The End
        // We Skip the Safe Zone of 10 before and 10 points after the start
        if(currentObstaclePoint > pointsLength)
        {
            currentObstaclePoint = 10 + obstacleLength;
            startIndex = 10;
        }

        Mesh mesh = RoadCreator.instance.CreateObstacle(startIndex, obstacleLength, side);
        obstacle[obstacleCount].mesh = mesh;
        obstacleColliders[obstacleCount].sharedMesh = mesh;

        SetObstacleSize(obstacleCount, startIndex, obstacleLength);

    }

    public void SetObstacleSize(int index, int start, int end)
    {
        obstacleSize[index].x = start;
        obstacleSize[index].y = end;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(obstacleCount);
        }
        else if(stream.IsReading)
        {
            obstacleCount = (int)stream.ReceiveNext();
        }
    }
}
