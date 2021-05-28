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
    Room,
    Connecting
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

    void Start() {
        //subscribe to room events
        RoomManager.Instance.OnRoomUsersUpdate += UpdateRoomPage;
        RoomManager.Instance.OnClientJoinRoom += OnJoinRoom;
    }

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
    }

    public void UpdatePlayerName()
    {
        UserManager.Instance.username = playerNameInput.text;
    }

    public void StartHost()
    {
        RoomManager.Instance.CreateRoom();
        UpdateRoomPage();
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
    
    public void LeaveRoom()
    {                
        RoomManager.Instance.LeaveRoom();
    }


    public void StartGame() {
        RoomManager.Instance.StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Room Page
    void UpdateRoomPage() {
        
        RoomManager roomManager = RoomManager.Instance;

        GenerateRoomPage(roomManager.maxPlayersPerTeam.Value);

        List<User> redTeamUsers = roomManager.FindUsersWithTeam(Team.RED);
        List<User> blueTeamUsers = roomManager.FindUsersWithTeam(Team.BLUE);
        
        for(int i=0; i<roomManager.maxPlayersPerTeam.Value; i++) {
            User? redUser = redTeamUsers.ElementAtOrDefault<User>(i);
            if(redUser != null) {
                redTeamPlayerRows.transform.GetChild(i+1).GetComponentInChildren<TextMeshProUGUI>().text = redUser.Value.username;
            }
            else {
                redTeamPlayerRows.transform.GetChild(i+1).GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
            }

            User? blueUser = blueTeamUsers.ElementAtOrDefault<User>(i);
            if(blueUser != null) {
                blueTeamPlayerRows.transform.GetChild(i+1).GetComponentInChildren<TextMeshProUGUI>().text = blueUser.Value.username;
            }
            else {
                blueTeamPlayerRows.transform.GetChild(i+1).GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
            }
        }
    }

    void OnJoinRoom() {        
        SetCurrentPage("Connecting");
        UpdateRoomPage();
        SetCurrentPage("Room");
    }

    void GenerateRoomPage(int noOfPlayersPerTeam) {
        //remove unwanted children
        for(int x=redTeamPlayerRows.transform.childCount-1; x>=0; x--) {
            if(x > 1) {
                GameObject.DestroyImmediate(redTeamPlayerRows.transform.GetChild(x).gameObject);
            }
        }
        
        for(int x=blueTeamPlayerRows.transform.childCount-1; x>=0; x--) {
            if(x > 1) {
                GameObject.DestroyImmediate(blueTeamPlayerRows.transform.GetChild(x).gameObject);
            }
        }

        Transform redTemplate = redTeamPlayerRows.transform.GetChild(1);
        Transform blueTemplate = blueTeamPlayerRows.transform.GetChild(1);
        for(int i=0; i<noOfPlayersPerTeam-1; i++) {
            GameObject.Instantiate(redTemplate.gameObject, Vector3.zero, Quaternion.identity, redTeamPlayerRows.transform);
            GameObject.Instantiate(blueTemplate.gameObject, Vector3.zero, Quaternion.identity, blueTeamPlayerRows.transform);            
        }        
    }

    #endregion
}
