using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI.Spawning;
using MLAPI;

public class MinimapCameraFollower : MonoBehaviour
{
    public static MinimapCameraFollower Instance;
    GameObject target;
    public Transform minimapCanvas;

    // public GameObject 

    void Awake() {
        Instance = this;
    }
    // Update is called once per frame
    void Update()
    {
        AttachToPlayerIfNeeded();
    }

    void AttachToPlayerIfNeeded() {
        if(!NetworkManager.Singleton.IsClient) return;
        if(target) return;

        NetworkObject localPlayerNetObj = NetworkSpawnManager.GetLocalPlayerObject();
        if(!localPlayerNetObj) return;
        
        GameObject localPlayerObject = localPlayerNetObj.gameObject; //error here refers to usercontroller that is not yet despawning
        PlayerController playerController = localPlayerObject.GetComponent<PlayerController>();
        if(!playerController) return;

        LocalPlayer localPlayer = playerController.GetPlayer();
        if(!localPlayer) return;
        target = localPlayer.gameObject;        
        GameObject camera = GameObject.FindGameObjectWithTag("MinimapCam");
        camera.transform.parent = target.transform;
        camera.transform.localPosition = new Vector3(0, 50, 0);
        camera.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

        GameObject icon = GameObject.FindGameObjectWithTag("MinimapIcon");
        //icon.transform.parent = target.transform;
        //icon.transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);

        //GameObject map = GameObject.FindGameObjectWithTag("Map2d");
        //map.transform.position = target.transform.position;
        //map.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        GenerateScreenIcons();
    }

    void GenerateScreenIcons() {
        foreach(LocalPlayer localPlayer in LocalPlayer.AllPlayers())
        {
            GameObject iconGO = GameObject.Instantiate(PrefabsManager.Instance.minimapPlayerIcon, minimapCanvas);
            iconGO.GetComponent<Image>().color = (localPlayer.team == Team.BLUE ? UIManager.Instance.colors.textBlue : UIManager.Instance.colors.textRed);
            iconGO.GetComponent<MinimapIcon>().target = localPlayer;
        }
    }
}


