using System.Collections;
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
    JoinRoom
}

public class MenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    MenuPage currentPage = MenuPage.SetUsername;
    public TMP_InputField playerNameInput;
    public TMP_InputField IpAddressInput;
    public GameObject gameManager; 


    public void SetCurrentPage(string pageName) {
        MenuPage result;
        System.Enum.TryParse<MenuPage>(pageName, true, out result);
        if(result == MenuPage.Null) {
            Debug.LogError($"SetCurrentPage failed: No page with name {pageName}");
            return;
        }
        currentPage = result;
        UpdateMenuPage();
    }

    public void Host()
    {
        NetworkManager.Singleton.StartHost();
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

    public void Join()
    {
        
        if (IpAddressInput.text.Length <= 0)
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = "127.0.0.1";
            GameManager gameManagerScript = gameManager.GetComponent<GameManager>();
            gameManagerScript.IpAddress = "127.0.0.1";
        }
        else
        {
            NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = IpAddressInput.text;
            GameManager gameManagerScript = gameManager.GetComponent<GameManager>();
            gameManagerScript.IpAddress = IpAddressInput.text;
        }
        NetworkManager.Singleton.StartClient();
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
        GameManager gameManagerScript = gameManager.GetComponent<GameManager>();
        gameManagerScript.playerName = playerNameInput.text;
    }

    void Start()
    {
        //NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck; 
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
    {
        bool approve = false;
        bool createPlayerObject = true;
        callback(createPlayerObject, null, approve, new Vector3(0, 0, 0), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateMenuPage() {
        foreach(Transform child in transform) {
            child.gameObject.SetActive(child.gameObject.name == currentPage.ToString());
        }
    }
}
