using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;

public class PrefabsManager : NetworkBehaviour
{
    public static PrefabsManager Instance;
    public GameObject smoke;
    public GameObject catchField;
    public GameObject teleportField;
    public Sprite berserkerIcon;
    public Sprite adeptIcon;
    public Sprite rogueIcon;
    public Sprite lancerIcon;

    public GameObject playerUsername;
    
    public GameObject flag;

    void Awake() {
        Instance = this;
    }

    public Sprite IconForCharacter(Character character) {
        switch (character)
        {
            case Character.Berserker:
                return berserkerIcon;
            case Character.Adept:
                return adeptIcon;
            case Character.Rogue:
                return rogueIcon;
            case Character.Lancer:
                return lancerIcon;
            default: 
                return berserkerIcon;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}