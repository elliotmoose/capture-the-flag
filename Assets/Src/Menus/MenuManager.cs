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
    public static MenuManager Instance;
    void Awake() {
        Instance = this;
    }
    // Start is called before the first frame update
    MenuPage currentPage = MenuPage.SetUsername;
    public TMP_InputField playerNameInput;
    public TMP_InputField IpAddressInput;
    
    //Room
    public GameObject redTeamPlayerRows;
    public GameObject blueTeamPlayerRows;
    public GameObject startGameButton;

    void Start() {
        //subscribe to room events
        RoomManager.Instance.OnRoomUsersUpdate += UpdateRoomPage;
        RoomManager.Instance.OnClientJoinRoom += OnJoinRoom;
        RoomManager.Instance.OnClientLeaveRoom += OnLeaveRoom;
    }
    void OnDestroy() {
        //subscribe to room events
        RoomManager.Instance.OnRoomUsersUpdate -= UpdateRoomPage;
        RoomManager.Instance.OnClientJoinRoom -= OnJoinRoom;
        RoomManager.Instance.OnClientLeaveRoom -= OnLeaveRoom;
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
        UserManager.Instance.SetUsername(playerNameInput.text);
    }

    public void StartHost()
    {
        RoomManager.Instance.CreateRoom();
        UpdateRoomPage();
        SetCurrentPage("Room");
        startGameButton.SetActive(true);
    }

    public void JoinRoom()
    {                
        SetCurrentPage("Connecting");        
        RoomManager.Instance.JoinRoom(IpAddressInput.text);
    }

    public void CancelJoinRoom() {
        SetCurrentPage("Home");
        NetworkManager.Singleton.StopClient();
    }

    void OnJoinRoom() {        
        UpdateRoomPage();
        SetCurrentPage("Room");
        startGameButton.SetActive(false);
    }

    public void SelectCharacter(Character character) {
        UserController.LocalInstance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, character);
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

        GenerateRoomPage(roomManager.roomSize.Value);

        
        List<User> blueTeamUsers = roomManager.FindUsersWithTeam(Team.BLUE);
        
        Color32 white = new Color32(255, 255, 255, 255);
        Color32 gray = new Color32(159, 159, 159, 255);

        for(int i=0; i<roomManager.roomSize.Value; i++) {            
            foreach(Team team in Team.GetValues(typeof(Team))) {
                List<User> users = roomManager.FindUsersWithTeam(team);
                GameObject playerRows = (team == Team.BLUE ? blueTeamPlayerRows : redTeamPlayerRows);
            
                if(i < users.Count) {
                    User user = users[i];
                    playerRows.transform.GetChild(i+1).GetComponentInChildren<TextMeshProUGUI>().text = user.username;
                    playerRows.transform.GetChild(i+1).GetComponentInChildren<Image>().sprite = PrefabsManager.Instance.IconForCharacter(user.character);
                    playerRows.transform.GetChild(i+1).GetComponentInChildren<Image>().color = white;
                }
                else {
                    playerRows.transform.GetChild(i+1).GetComponentInChildren<TextMeshProUGUI>().text = "Empty Slot";
                    playerRows.transform.GetChild(i+1).GetComponentInChildren<Image>().sprite = null;
                    playerRows.transform.GetChild(i+1).GetComponentInChildren<Image>().color = gray;
                }
            }
        }
    }    

    public void LeaveRoom()
    {                
        RoomManager.Instance.LeaveRoom();
    }

    void OnLeaveRoom() {
        SetCurrentPage("Home");
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
        redTemplate.GetComponentInChildren<Button>().onClick.AddListener(()=>{
            RequestJoinTeam(Team.RED, 0);            
        });
        blueTemplate.GetComponentInChildren<Button>().onClick.AddListener(()=>{
            RequestJoinTeam(Team.BLUE, 0);            
        });
        
        for(int i=1; i<noOfPlayersPerTeam; i++) {
            GameObject redPlayerButton = GameObject.Instantiate(redTemplate.gameObject, Vector3.zero, Quaternion.identity, redTeamPlayerRows.transform);
            GameObject bluePlayerButton = GameObject.Instantiate(blueTemplate.gameObject, Vector3.zero, Quaternion.identity, blueTeamPlayerRows.transform);            

            int copy = i; //copy the index value because it passes by reference
            redPlayerButton.GetComponentInChildren<Button>().onClick.AddListener(()=>{
                RequestJoinTeam(Team.RED, copy);            
            });
            bluePlayerButton.GetComponentInChildren<Button>().onClick.AddListener(()=>{
                RequestJoinTeam(Team.BLUE, copy);            
            });
        }
    }

    void RequestJoinTeam(Team team, int slotIndex) {
        UserController.LocalInstance.JoinTeamServerRpc(NetworkManager.Singleton.LocalClientId, team);
    }

    #endregion
}
