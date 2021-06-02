using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class CharacterSelectionManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] Heroes;

    [SerializeField]
    GameObject SelectionScreen;

    int HeroIndex;

    // Start is called before the first frame update
    void Start()
    {
        InstantiateHero();
        ChangeHero();
    }

    public void InstantiateHero()
    {
        // GameObject go = GameObject.Instantiate(Heroes[HeroIndex], Vector3.zero, Quaternion.identity);
        // go.GetComponent<Player>().enabled = false;
    }

    public void ChangeHero()
    {
        for (int i = 0; i < Heroes.Length; i++)
        {
            if (i == HeroIndex)
            {
                Heroes[i].gameObject.SetActive(true);
            }
            else
            {
                Heroes[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectWarrior()
    {
        HeroIndex = 0;
        UserController.LocalInstance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, Character.Warrior);
        ChangeHero();
    }

    public void SelectMage()
    {
        HeroIndex = 1;
        UserController.LocalInstance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, Character.Mage);
        ChangeHero();
    }

    public void SelectNinja()
    {
        HeroIndex = 2;
        UserController.LocalInstance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, Character.Thief);
        ChangeHero();
    }

    // Update is called once per frame
    void Update()
    {
        if(Heroes[HeroIndex] != null) {
            Heroes[HeroIndex].transform.Rotate(0, -80*Time.deltaTime, 0, Space.World);
        }
    }
}
