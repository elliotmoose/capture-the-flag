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

    void Awake() {
        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
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

    // Update is called once per frame
    void Update()
    {
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
