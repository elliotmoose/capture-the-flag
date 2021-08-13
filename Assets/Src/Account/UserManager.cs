using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance;
    string username = "";

    public bool resetUsername = false;
    // Start is called before the first frame update
    void Awake()
    {
        if(resetUsername) {
            PlayerPrefs.SetString("username", "");
        }
        username = PlayerPrefs.GetString("username");
        Instance = this;
    }

    public void SetUsername(string newUsername) {
        PlayerPrefs.SetString("username", newUsername);
        this.username = newUsername;
    }

    public string GetUsername() {        
        return this.username;
    }
    
    public string GetUsernameFresh() {  
        username = PlayerPrefs.GetString("username");     
        return this.username;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
