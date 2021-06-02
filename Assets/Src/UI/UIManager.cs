using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Image staminaBar;
    public TMPro.TextMeshProUGUI redTeamScore;
    public TMPro.TextMeshProUGUI blueTeamScore;
    public GameObject countdownText;

    //prefabs
    public GameObject playerIconTemplate;
    
    private List<GameObject> generatedPlayerIcons = new List<GameObject>();
    void Awake() {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.GenerateGameSummaryUI(RoomManager.Instance.GetUsers(), RoomManager.Instance.roomSize.Value);
        UIManager.Instance.GenerateGameSummaryUI(RoomManager.Instance.GetUsers(), RoomManager.Instance.roomSize.Value);

        RoomManager.Instance.OnRoomUsersUpdate += ()=>{
            UIManager.Instance.GenerateGameSummaryUI(RoomManager.Instance.GetUsers(), RoomManager.Instance.roomSize.Value);
        };
    }

    public void DisplayCountdown(int count) {
        countdownText.SetActive(true);
        TMP_Text textMeshPro = countdownText.GetComponent<TMPro.TMP_Text>(); 
        Animation animation = countdownText.GetComponent<Animation>();
        if(count > 0) {
            textMeshPro.fontSize = 600;
            textMeshPro.text = $"{count}";
            animation.Rewind();
            animation.Play("CountdownAnim");
        }
        else {
            textMeshPro.fontSize = 300;
            textMeshPro.text = "ROUND START";
            animation.Rewind();
            animation.Play("CountdownAnim");
        }
    }

    public void GenerateGameSummaryUI(List<User> users, int teamSize) {
        Transform gameSummaryUI = this.transform.Find("GameSummaryUI");
        Debug.Log(generatedPlayerIcons.Count);
        for(int i=0; i < generatedPlayerIcons.Count; i++) {
            generatedPlayerIcons[i].transform.SetParent(null);
            GameObject.Destroy(generatedPlayerIcons[i]);
        }

        generatedPlayerIcons.Clear();

        for(int i=0; i<teamSize;i++) {
            foreach(Team team in Team.GetValues(typeof(Team))) {
                GameObject playerIconGameObject = GameObject.Instantiate(playerIconTemplate, Vector3.zero, Quaternion.identity, gameSummaryUI);
                playerIconGameObject.transform.SetSiblingIndex(team == Team.BLUE ? i+3 : i);
                generatedPlayerIcons.Add(playerIconGameObject);

                List<User> teamUsers = users.FindAll((User user) => {
                    return user.team == team;
                });


                if(i < teamUsers.Count) {
                    User user = teamUsers[i];
                    playerIconGameObject.GetComponent<Image>().sprite = PrefabsManager.Instance.IconForCharacter(user.character);
                    playerIconGameObject.GetComponent<Image>().color = new Color32(255,255,255,255);
                }
            }
        }
    }

    public void UpdatePlayerUI() {
        if(PlayerController.LocalInstance == null) return;

        User user = PlayerController.LocalInstance.user.Value;
        Debug.Log($"Updating player UI for user: {user.username} {user.character}");
        Transform playerUI = this.transform.Find("PlayerUI");
        Transform character = this.transform.Find("PlayerUI/Character");
        Transform skill1Button = this.transform.Find("PlayerUI/Skill1Button");
        Transform skill2Button = this.transform.Find("PlayerUI/Skill2Button");
        Transform catchButton = this.transform.Find("PlayerUI/CatchButton");
        Transform passive = this.transform.Find("PlayerUI/Passive");

        character.GetComponent<Image>().sprite = PrefabsManager.Instance.IconForCharacter(user.character);
        character.GetComponent<Image>().color = new Color32(255,255,255,255);
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerUI();
        if(PlayerController.LocalInstance != null) {
            Player player = PlayerController.LocalInstance.GetPlayer();
            float stamina = player.curStamina.Value/player.maxStamina.Value;
            staminaBar.fillAmount = stamina;
        }

        if(GameManager.Instance != null) {
            redTeamScore.text = $"{GameManager.Instance.redTeamScore.Value}";
            blueTeamScore.text = $"{GameManager.Instance.blueTeamScore.Value}";
        }
        else {
            Debug.LogWarning("Attempting to update UI but GameManager does not exist");
        }
    }
}
