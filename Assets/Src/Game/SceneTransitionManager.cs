using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MLAPI;
using MLAPI.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    //called by server only
    public void RoomToGameScene(List<User> users, int roomSize) {
        SceneSwitchProgress sceneSwitchProgress = NetworkSceneManager.SwitchScene("Game");
        sceneSwitchProgress.OnClientLoadedScene += (ulong clientId) => {
            foreach(User user in users) {
                if(user.clientId == clientId) {
                    Debug.Log($"== SceneTransitionManager: {user.username} has changed scene successfully!");
                }
            }
        };

        sceneSwitchProgress.OnComplete += (bool timeOut) => {
            if(timeOut) {
                Debug.Log("== SceneTransitionManager: connection timed out!!");
            }

            //handover room state
            foreach(User user in users) {
                RoomManager.Instance.roomUsers.Add(user);
            }

            RoomManager.Instance.roomSize.Value = roomSize;

            GameManager.Instance.StartGame();  
        }; 
    }

    //called by all
    public void GameToMainMenu() {
        if(NetworkManager.Singleton.IsClient) NetworkManager.Singleton.StopClient();
        if(NetworkManager.Singleton.IsHost) NetworkManager.Singleton.StopHost();
        GameObject.Destroy(this.gameObject);
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    // called first
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // called second
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "MainMenu") {
            if(UserManager.Instance.GetUsernameFresh() != "") {
                MenuManager.Instance.SetCurrentPage("Home");
                Debug.Log("Set as home!");
            }
        }
    }

    // called when the game is terminated
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
