using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using Smooth;

public class TestManager : MonoBehaviour
{
    public bool autoStartHost = true;
    public Character character;
    // Start is called before the first frame update

    void Awake() {
        if(autoStartHost) {
            GameObject.Find("RedFlag").GetComponent<SmoothSyncMLAPI>().enabled = false;
            GameObject.Find("BlueFlag").GetComponent<SmoothSyncMLAPI>().enabled = false;
        }
    }
    void Start()
    {
        if(autoStartHost) {
            this.gameObject.AddComponent<UserManager>();
            // this.gameObject.AddComponent<RoomManager>().CreateRoom();
            RoomManager.Instance.CreateRoom();
            RoomManager.Instance.UserRequestSelectCharacter(NetworkManager.Singleton.LocalClientId, character);
            // NetworkManager.Singleton.StartHost();
            // List<User> users = new List<User>();
            // User me = new User(NetworkManager.Singleton.LocalClientId, Team.BLUE, "testUser", Character.Warrior);
            // users.Add(me);
            GameManager.Instance.StartGame();
            GameObject.Find("RedFlag").GetComponent<SmoothSyncMLAPI>().enabled = true;
            GameObject.Find("BlueFlag").GetComponent<SmoothSyncMLAPI>().enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
