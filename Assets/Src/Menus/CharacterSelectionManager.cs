using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        GameObject.Instantiate(Heroes[HeroIndex], Vector3.zero, Quaternion.identity);
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
        ChangeHero();
    }

    public void SelectMage()
    {
        HeroIndex = 1;
        ChangeHero();
    }

    public void SelectNinja()
    {
        HeroIndex = 2;
        ChangeHero();
    }

    // Update is called once per frame
    void Update()
    {
        Heroes[HeroIndex].transform.Rotate(0, -5f, 0, Space.World);
    }
}
