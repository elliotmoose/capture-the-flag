using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using Smooth;

public class TestManager : MonoBehaviour
{
    public bool autoStartHost = true;
    public Character character;
    public Team team;
    // Start is called before the first frame update

    void Awake() {

        if(SceneTransitionManager.cameFromMainMenu && autoStartHost) {
            Debug.LogWarning("Auto start host was enabled, but came from main menu. Ignoring...");
            return;
        }
        if(autoStartHost) {

            GameObject.Find("NetworkManager").AddComponent<SceneTransitionManager>();
            this.gameObject.AddComponent<UserManager>();
            RoomManager.Instance.CreateRoom();
            RoomManager.Instance.UserRequestSelectCharacter(NetworkManager.Singleton.LocalClientId, character);
            RoomManager.Instance.UserRequestJoinTeam(NetworkManager.Singleton.LocalClientId, team);
            // NetworkManager.Singleton.StartHost();
            // List<User> users = new List<User>();
            // User me = new User(NetworkManager.Singleton.LocalClientId, Team.BLUE, "testUser", Character.Warrior);
            // users.Add(me);
            GameManager.Instance.StartGame();
            // GameObject.Find("RedFlag").GetComponent<SmoothSyncMLAPI>().enabled = true;
            // GameObject.Find("BlueFlag").GetComponent<SmoothSyncMLAPI>().enabled = true;
        }
    }
}
