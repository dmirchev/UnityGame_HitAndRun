using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadoutState : MonoBehaviour
{
    public static LoadoutState instance { get { return s_Instance; } }
    protected static LoadoutState s_Instance;

    public GameObject[] views;

    public InputField createRoomInput, joinRoomInput;
    public Text createMessage, joinMessage;
    public bool executeOrLeave = true;
    string empty = "";

    void Awake()
    {
        s_Instance = this;

        executeOrLeave = true;

        SetCreateMessage(empty);
        SetJoinMessage(empty);
    }

    public void SetViewState(int index, bool state)
    {
        views[index].SetActive(state);
    }

    public void SetCreateMessage(string message)
    {
        createMessage.text = message;
    }

    public void OnClickCreateRoom()
    {
        SetCreateMessage(empty);
        Debug.Log(PhotonLobby.instance);
        PhotonLobby.instance.OnCreateRoom(createRoomInput.text);
    }

    public void SetJoinMessage(string message)
    {
        joinMessage.text = message;
    }

    public void OnClickJoinRoom()
    {
        SetJoinMessage(empty);
        PhotonLobby.instance.OnJoinRoom(joinRoomInput.text);
    }
}
