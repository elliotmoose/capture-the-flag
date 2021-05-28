using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Image staminaBar;
    public TMPro.TextMeshProUGUI redTeamScore;
    public TMPro.TextMeshProUGUI blueTeamScore;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerController.LocalInstance != null) {
            Player player = PlayerController.LocalInstance.GetPlayer();
            float stamina = player.stamina.Value/player.maxStamina.Value;
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
