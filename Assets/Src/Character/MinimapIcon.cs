using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapIcon : MonoBehaviour
{
    public LocalPlayer target;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!target) return;
        this.GetComponent<RectTransform>().position = target.transform.position;
        GetComponent<Image>().color = (target.team == Team.BLUE ? UIManager.Instance.colors.textBlue : UIManager.Instance.colors.textRed);
    }
}
