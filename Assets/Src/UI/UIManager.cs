using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Colors colors;
    //gameplay
    public Image staminaBar;
    public TMPro.TextMeshProUGUI redTeamScore;
    public TMPro.TextMeshProUGUI blueTeamScore;
    public GameObject countdownText;

    //gameover
    public GameObject gameoverPanel;
    public Transform scoreboardRowsParent;
    public TMP_Text scoreboardWinningTeamText;
    public GameObject scoreboardHeaderRow;

    //prefabs
    public GameObject playerIconTemplate;
    
    private List<GameObject> generatedPlayerIcons = new List<GameObject>();
    void Awake() {
        Instance = this;
    }

    private SkillButton skill1Button;
    private SkillButton skill2Button;
    private SkillButton catchButton;    

    public Transform eventLogContainerContent;
    public ScrollRect eventLogScrollRect;

    // Start is called before the first frame update
    void Start()
    {        
        RoomManager.Instance.OnRoomUsersUpdate += GenerateGameSummaryUI;
        StatsManager.Instance.StatsDisplayUpdated += GenerateScoreboard;

        skill1Button = this.transform.Find("PlayerUI/Skill1Button").GetComponent<SkillButton>();
        skill2Button = this.transform.Find("PlayerUI/Skill2Button").GetComponent<SkillButton>();
        catchButton = this.transform.Find("PlayerUI/CatchButton").GetComponent<SkillButton>();

        Cursor.visible = false;
    }

    void OnDestroy() {
        RoomManager.Instance.OnRoomUsersUpdate -= GenerateGameSummaryUI;
        StatsManager.Instance.StatsDisplayUpdated -= GenerateScoreboard;
    }

    #region Gameplay UI
    public void DisplayCountdown(int count) {
        countdownText.SetActive(true);
        TMP_Text textMeshPro = countdownText.GetComponent<TMPro.TMP_Text>(); 
        Animation animation = countdownText.GetComponent<Animation>();
        AudioSource audioSource = countdownText.GetComponent<AudioSource>();
        if(count > 0) {
            audioSource.clip = PrefabsManager.Instance.countdown1;
            textMeshPro.fontSize = 600;
            textMeshPro.text = $"{count}";
            animation.Rewind();
            audioSource.Play();
            animation.Play("CountdownAnim");
        }
        else {
            audioSource.clip = PrefabsManager.Instance.countdown2;
            textMeshPro.fontSize = 300;
            textMeshPro.text = "ROUND START";
            animation.Rewind();
            audioSource.Play();
            animation.Play("CountdownAnim");
        }
    }

    public void GenerateGameSummaryUI() {
        List<User> users = RoomManager.Instance.GetUsers(); 
        int teamSize = RoomManager.Instance.roomSize.Value;

        Transform gameSummaryUI = this.transform.Find("GameSummaryUI");
        for(int i=0; i < generatedPlayerIcons.Count; i++) {
            generatedPlayerIcons[i].transform.SetParent(null);
            GameObject.Destroy(generatedPlayerIcons[i]);
        }

        generatedPlayerIcons.Clear();

        for(int i=0; i<teamSize;i++) {
            foreach(Team team in Team.GetValues(typeof(Team))) {
                GameObject playerIconGameObject = GameObject.Instantiate(playerIconTemplate, gameSummaryUI);
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

        User user = PlayerController.LocalInstance.GetUser();
        Transform playerUI = this.transform.Find("PlayerUI");
        Transform passive = this.transform.Find("PlayerUI/Passive");

        Transform character = this.transform.Find("PlayerUI/Character");
        character.GetComponent<Image>().sprite = PrefabsManager.Instance.IconForCharacter(user.character);
        character.GetComponent<Image>().color = new Color32(255,255,255,255);

        LocalPlayer localPlayer = PlayerController.LocalInstance.GetPlayer();
                
        if(localPlayer != null) {
            skill1Button.icon = localPlayer.skills[0].icon;
            skill2Button.icon = localPlayer.skills.Count > 1 ? localPlayer.skills[1].icon : PrefabsManager.Instance.cloneCatchPlaceholderIcon;
            catchButton.icon = localPlayer.catchSkill.icon;
            skill1Button.curCooldown = localPlayer.skill1CooldownTime;
            skill2Button.curCooldown = localPlayer.skill2CooldownTime;
            catchButton.curCooldown = localPlayer.catchCooldownTime;

            skill1Button.maxCooldown = localPlayer.skills[0].cooldown;
            if(localPlayer.skills.Count > 1) {
                skill2Button.maxCooldown = localPlayer.skills[1].cooldown;
            }

            catchButton.maxCooldown = localPlayer.catchSkill.cooldown;
        }
    }


    public void LogEvent(string text) {
        GameObject eventLogText = GameObject.Instantiate(PrefabsManager.Instance.eventLogText, eventLogContainerContent);
        eventLogText.GetComponentInChildren<TextMeshProUGUI>().text = text;
        Canvas.ForceUpdateCanvases ();
        LayoutRebuilder.ForceRebuildLayoutImmediate(eventLogContainerContent.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases ();
        eventLogScrollRect.verticalNormalizedPosition = 0f;
        Canvas.ForceUpdateCanvases ();
    }
        
    #endregion

    #region Gameover UI
    public void DisplayGameOver(Team winningTeam) {
        gameoverPanel.SetActive(true);
        Team localPlayerTeam = PlayerController.LocalInstance.GetUser().team;
        Color32 textColor = (localPlayerTeam == Team.BLUE) ? colors.textBlue : colors.textRed;
        bool thisPlayerWon = (localPlayerTeam == winningTeam);
        string winningTeamText = thisPlayerWon ? "VICTORY" : "DEFEAT";
        scoreboardWinningTeamText.text = winningTeamText;
        scoreboardWinningTeamText.color = textColor;

        GenerateScoreboard();
    }

    public void GenerateScoreboard() {
        List<GameStat> stats = StatsManager.Instance.GetStats();

        //destroy old
        for(int i=scoreboardRowsParent.childCount-1; i>=1 && i < scoreboardRowsParent.childCount; i--) {
            Transform child = scoreboardRowsParent.GetChild(i);
            child.SetParent(null);
            GameObject.Destroy(child.gameObject);
        }

        foreach(GameStat stat in stats) {
            GameObject statRow = GameObject.Instantiate(scoreboardHeaderRow, scoreboardRowsParent);
            statRow.transform.Find("Icon").GetComponent<Image>().sprite = PrefabsManager.Instance.IconForCharacter(stat.user.character);
            statRow.transform.Find("Icon").GetComponent<Image>().color = Color.white;
            statRow.transform.Find("Username").GetComponent<TMP_Text>().text = stat.user.username;
            statRow.transform.Find("Username").GetComponent<TMP_Text>().color = (stat.user.team == Team.BLUE) ? colors.textBlue : colors.textRed;
            statRow.transform.Find("Flags Scored").GetComponent<TMP_Text>().text = stat.flagsScored.ToString();
            statRow.transform.Find("Players Captured").GetComponent<TMP_Text>().text = stat.playersCaptured.ToString();
            statRow.transform.Find("Players Freed").GetComponent<TMP_Text>().text = stat.playersFreed.ToString();
            statRow.transform.Find("Times In Jail").GetComponent<TMP_Text>().text = stat.timesInJail.ToString();
            statRow.transform.Find("Time In Opponent Territory").GetComponent<TMP_Text>().text = stat.timeInEnemyTerritory.ToString("F1")+"s";
            statRow.transform.Find("Time with Flag").GetComponent<TMP_Text>().text = stat.timeWithFlag.ToString("F1")+"s";
            statRow.transform.Find("MVP").GetComponent<TMP_Text>().text = stat.isMVP.ToString();
        }        
    }

    public void LeaveGame() {
        SceneTransitionManager.Instance.GameToMainMenu();
    }

    public void FreezeCamera() {
        CinemachineVirtualCamera camera = GameObject.FindGameObjectWithTag("CinemachineCamera").GetComponent<CinemachineVirtualCamera>();
        camera.enabled = false;

        //show mouse
        Cursor.visible = true;
    }

    #endregion
    // Update is called once per frame
    void Update()
    {
        UpdatePlayerUI();
        if(PlayerController.LocalInstance != null) {
            LocalPlayer localPlayer = PlayerController.LocalInstance.GetPlayer();
            if(localPlayer != null) {
                float stamina = localPlayer.GetStaminaFraction();
                staminaBar.fillAmount = stamina;
            }
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
