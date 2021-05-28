using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MLAPI;
using MLAPI.Transports.UNET;
using MLAPI.SceneManagement;
using UnityEngine.UI;
using TMPro;



public enum MenuPage {
    Null,
    SetUsername,
    Home,
    JoinRoom,
    Room
}

public class MenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    MenuPage currentPage = MenuPage.SetUsername;
    public TMP_InputField playerNameInput;
    public TMP_InputField IpAddressInput;
    
    //Room
    public GameObject redTeamPlayerRows;
    public GameObject blueTeamPlayerRows;

    public void SetCurrentPage(string pageName) {
        MenuPage result;
        System.Enum.TryParse<MenuPage>(pageName, true, out result);
        if(result == MenuPage.Null) {
            Debug.LogError($"SetCurrentPage failed: No page with name {pageName}");
            return;
        }
        
        MenuPage previousPage = currentPage;
        currentPage = result;
        
        foreach(Transform child in transform) {
            child.gameObject.SetActive(child.gameObject.name == currentPage.ToString());
        }
        
        if(currentPage == MenuPage.Room) {
            RoomManager.Instance.OnRoomUsersUpdate += UpdateRoomPage;
        }
        else if(previousPage == MenuPage.Room) {
            RoomManager.Instance.OnRoomUsersUpdate -= UpdateRoomPage;
        }
    }

    public void StartHost()
    {
        RoomManager roomManager = RoomManager.Instance;
        roomManager.CreateRoom();
        GenerateRoomPage(roomManager.maxPlayersPerTeam);
        UpdateRoomPage();
        //if (NetworkManager.Singleton.IsListening)
        //{
        //    NetworkSceneManager.SwitchScene("Game");
        //}
        //else
        //{
        //    SceneManager.LoadSceneAsync("Game");
        //}
        //SceneManager.LoadScene("Game", LoadSceneMode.Single);

    }

    public void JoinRoom()
    {                
        RoomManager.Instance.JoinRoom(IpAddressInput.text);
        
        //if (NetworkManager.Singleton.IsListening)
        //{
        //    NetworkSceneManager.SwitchScene("Game");
        //}
        //else
        //{
        //    SceneManager.LoadSceneAsync("Game");
        //}
        //SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }

    public void UpdatePlayerName()
    {
        // GameManager gameManagerScript = gameManager.GetComponent<GameManager>();
        // gameManagerScript.playerName = playerNameInput.text;
    }

    void Start()
    {
        
    }

    public void StartGame() {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateRoomPage() {
        RoomManager roomManager = RoomManager.Instance;

        List<User> redTeamUsers = new List<User>();
        List<User> blueTeamUsers = new List<User>();
        foreach(User user in roomManager.roomUsers) {
            if(user.team == Team.BLUE) {
                blueTeamUsers.Add(user);
            }
            else {
                redTeamUsers.Add(user);
            }
        }

        for(int i=0; i<roomManager.maxPlayersPerTeam; i++) {
            User redUser = redTeamUsers.ElementAtOrDefault<User>(i);
            if(redUser != null) {
                redTeamPlayerRows.transform.GetChild(i+1).GetComponentInChildren<TextMeshProUGUI>().text = redUser.username;
            }
            User blueUser = blueTeamUsers.ElementAtOrDefault<User>(i);
            if(blueUser != null) {
                blueTeamPlayerRows.transform.GetChild(i+1).GetComponentInChildren<TextMeshProUGUI>().text = blueUser.username;
            }
        }
    }

    void GenerateRoomPage(int noOfPlayersPerTeam) {

        //remove unwanted children
        for(int x=0; x<redTeamPlayerRows.transform.childCount; x++) {
            if(x > 1) {
                redTeamPlayerRows.transform.GetChild(x).SetParent(null);
            }
        }
        
        for(int x=0; x<blueTeamPlayerRows.transform.childCount; x++) {
            if(x > 1) {
                blueTeamPlayerRows.transform.GetChild(x).SetParent(null);
            }
        }

        Transform redTemplate = redTeamPlayerRows.transform.GetChild(1);
        Transform blueTemplate = blueTeamPlayerRows.transform.GetChild(1);
        for(int i=0; i<noOfPlayersPerTeam-1; i++) {
            GameObject.Instantiate(redTemplate.gameObject, Vector3.zero, Quaternion.identity, redTeamPlayerRows.transform);
            GameObject.Instantiate(blueTemplate.gameObject, Vector3.zero, Quaternion.identity, blueTeamPlayerRows.transform);            
        }        
    }
}
