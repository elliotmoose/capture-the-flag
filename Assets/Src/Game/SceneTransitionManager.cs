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


    public void RoomToGameScene(List<User> users) {
        SceneSwitchProgress sceneSwitchProgress = NetworkSceneManager.SwitchScene("Game");
        sceneSwitchProgress.OnComplete += (bool timeOut) => {
            GameManager.Instance.users = users;            
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
