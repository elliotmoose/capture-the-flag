using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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


    public void RoomToGameScene(List<User> users, int roomSize) {
        SceneSwitchProgress sceneSwitchProgress = NetworkSceneManager.SwitchScene("Game");
        sceneSwitchProgress.OnComplete += (bool timeOut) => {
            //handover room state
            foreach(User user in users) {
                RoomManager.Instance.roomUsers.Add(user);
            }

            RoomManager.Instance.roomSize.Value = roomSize;

            GameManager.Instance.StartGame();  
            
        };     
    }

    public void SwitchScene(string sceneName) {
        SceneSwitchProgress sceneSwitchProgress = NetworkSceneManager.SwitchScene(sceneName);        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
