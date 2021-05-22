using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start()
    {
        
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
