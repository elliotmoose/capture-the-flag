using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    public static UserManager Instance;
    public string username = "";
    // Start is called before the first frame update
    void Awake()
    {
        if(Instance != null) {
            this.username = Instance.username;
        }
        
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
