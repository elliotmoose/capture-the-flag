using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using Smooth;

public class TestManager : MonoBehaviour
{
    public bool autoStartConnection = true;
    public bool twoPlayer = false;
    public bool isHost = false;
    bool gameStarted = false;
    public Character character;
    // Start is called before the first frame update

    void Start() {
        if(SceneTransitionManager.cameFromMainMenu && autoStartConnection) {
            Debug.LogWarning("Auto start host was enabled, but came from main menu. Ignoring...");
            return;
        }
        if(autoStartConnection) {
            // GameObject.Find("NetworkManager").AddComponent<SceneTransitionManager>();
            this.gameObject.AddComponent<UserManager>();

            if(isHost) {
                RoomManager.Instance.CreateRoom();
                RoomManager.Instance.UserRequestSelectCharacter(NetworkManager.Singleton.LocalClientId, character);
                RoomManager.Instance.UserRequestJoinTeam(NetworkManager.Singleton.LocalClientId, Team.RED);
            }
            else {
                RoomManager.Instance.JoinRoom("");
                RoomManager.Instance.UserRequestSelectCharacter(NetworkManager.Singleton.LocalClientId, character);
                RoomManager.Instance.UserRequestJoinTeam(NetworkManager.Singleton.LocalClientId, Team.BLUE);
            }
            // NetworkManager.Singleton.StartHost();
            // List<User> users = new List<User>();
            // User me = new User(NetworkManager.Singleton.LocalClientId, Team.BLUE, "testUser", Character.Warrior);
            // users.Add(me);            
            // GameObject.Find("RedFlag").GetComponent<SmoothSyncMLAPI>().enabled = true;
            // GameObject.Find("BlueFlag").GetComponent<SmoothSyncMLAPI>().enabled = true;
        }
    }

    void Update() {
        if(!isHost || gameStarted) {return;}
        if(SceneTransitionManager.cameFromMainMenu && autoStartConnection) {
            Debug.LogWarning("Auto start host was enabled, but came from main menu. Ignoring...");
            return;
        }
        if(twoPlayer && RoomManager.Instance.roomUsers.Count == 2) {
            GameManager.Instance.StartGame();
            gameStarted = true;
        }
        else if(!twoPlayer) {
            GameManager.Instance.StartGame();
            gameStarted = true;
        }
    }
}
