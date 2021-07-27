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
    public GameObject knockbackField;
    public GameObject cloneTrail;

    public AudioClip laserSound;
    public AudioClip buffSound;
    public AudioClip smokeSound;
    public AudioClip boostSound;
    public AudioClip slamSound;
    public AudioClip teleportSound;
    public AudioClip electricitySound;
    public AudioClip invisSound;

    public AudioClip jailSound;
    public AudioClip countdown1;
    public AudioClip countdown2;

    public Sprite berserkerIcon;
    public Sprite adeptIcon;
    public Sprite rogueIcon;
    public Sprite lancerIcon;

    public Sprite teleportIcon;
    public Sprite empIcon;
    public Sprite invisibilityIcon;
    public Sprite cloneCatchPlaceholderIcon;
    public Sprite inductiveChargeIcon;
    public Sprite extendedReachIcon;
    public Sprite boostIcon;
    public Sprite knockbackIcon;
    public Sprite catchIcon;
    public Sprite cloneCatchIcon;

    public Sprite adeptPassiveIcon;
    public Sprite berserkerPassiveIcon;
    public Sprite roguePassiveIcon;
    public Sprite lancerPassiveIcon;
    public GameObject minimapPlayerIcon;

    public GameObject playerUsername;
    
    public GameObject flag;

    public GameObject eventLogText;
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