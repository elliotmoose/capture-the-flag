using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InGameMenuManager : MonoBehaviour
{
    public static InGameMenuManager Instance;
    string currentPage = "";
    
    public bool isMenuActive => currentPage != "";

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            SetCurrentPage(currentPage == "" ? "GameMenu" : "");
        }
    }

    public void SetCurrentPage(string pageName) {
        currentPage = pageName;                     
        Cursor.visible = currentPage == "" ? false : true;
        
        foreach(Transform child in transform) {
            //close menu
            if(currentPage == "") { 
                child.gameObject.SetActive(false);
                continue;
            }

            child.gameObject.SetActive(child.gameObject.name == currentPage);            
        }               
    }

    public void LeaveGame() {
        SceneTransitionManager.Instance.GameToMainMenu();
    }
}
