using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class CharacterSelectionManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] Characters;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SelectCharacterAtIndex(int characterIndex) {
        for (int i = 0; i < Characters.Length; i++)
        {
            if (i == characterIndex)
            {
                Characters[i].gameObject.SetActive(true);
            }
            else
            {
                Characters[i].gameObject.SetActive(false);
            }
        }

        Character[] characters = {Character.Berserker, Character.Adept, Character.Rogue, Character.Lancer};
        UserController.LocalInstance.SelectCharacterServerRpc(NetworkManager.Singleton.LocalClientId, characters[characterIndex]);        
    }

    // Update is called once per frame
    void Update()
    {
        // if(Characters[HeroIndex] != null) {
        //     Characters[HeroIndex].transform.Rotate(0, -80*Time.deltaTime, 0, Space.World);
        // }
    }
}
