using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using TMPro;
public class CharacterSelectionManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] Characters;

    public Image[] skillIcons;
    public TextMeshProUGUI[] skillNames;
    public GameObject skillDescriptionContainer;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI skillDescription;
    public TextMeshProUGUI skillCooldown;

    private Skill[] displaySkills = new Skill[3];
    // Start is called before the first frame update
    void Start()
    {
        SelectCharacterAtIndex(0); //select berserker as default
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

        switch (characters[characterIndex])
        {
            case Character.Berserker:
                displaySkills[0] = new Boost();//passive
                displaySkills[1] = new Boost();
                displaySkills[2] = new Knockback();
                break;
            case Character.Adept:
                displaySkills[0] = new Teleport();//passive
                displaySkills[1] = new Teleport();
                displaySkills[2] = new Slow();
                break;
            case Character.Rogue:
                displaySkills[0] = new Invisibility();//passive
                displaySkills[1] = new Invisibility();
                displaySkills[2] = new CloneCatch();
                break;
            case Character.Lancer:
                displaySkills[0] = new Smoke();//passive
                displaySkills[1] = new Smoke();
                displaySkills[2] = new Reach();
                break;
            default:
                Debug.LogError("Invalid character type");
                break;
        }        
        
        for(int i=0; i<skillIcons.Length; i++) {
            skillIcons[i].sprite = displaySkills[i].icon;
            skillNames[i].text = displaySkills[i].name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // if(Characters[HeroIndex] != null) {
        //     Characters[HeroIndex].transform.Rotate(0, -80*Time.deltaTime, 0, Space.World);
        // }
    }

    public void OnPointerEnterSkillIcon(int index) {
        skillDescriptionContainer.SetActive(true);
        skillName.text = displaySkills[index].name;
        skillDescription.text = displaySkills[index].description;
        skillCooldown.text = $"Cooldown: {displaySkills[index].cooldown:#.#}s";
    }
    public void OnPointerExitSkillIcon(int index) {
        skillDescriptionContainer.SetActive(false);
    }
}
