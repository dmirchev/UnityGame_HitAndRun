using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public static PlayerInfo instance { get { return s_Instance; } }
    protected static PlayerInfo s_Instance;

    public int playerColor;

    void Awake()
    {
        s_Instance = this;
    }

    void Start()
    {
        if(PlayerPrefs.HasKey("MyCharacter"))
        {
            playerColor = PlayerPrefs.GetInt("MyCharacter");
        }
        else
        {
            playerColor = 0;
            PlayerPrefs.SetInt("MyCharacter", playerColor);
        }
    }
}
