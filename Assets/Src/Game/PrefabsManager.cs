using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;

public class PrefabsManager : NetworkBehaviour
{
    public static PrefabsManager Instance;
    public GameObject smoke;

    public Sprite warriorIcon;
    public Sprite mageIcon;
    public Sprite thiefIcon;

    void Awake() {
        Instance = this;
    }

    public Sprite IconForCharacter(Character character) {
        switch (character)
        {
            case Character.Warrior:
                return warriorIcon;
            case Character.Mage:
                return mageIcon;
            case Character.Thief:
                return thiefIcon;
            default: 
                return warriorIcon;
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
