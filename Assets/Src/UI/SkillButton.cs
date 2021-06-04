using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{

    public float maxCooldown;
    public float curCooldown;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(curCooldown > 0) {
            curCooldown -= Time.deltaTime;            
            this.transform.Find("CooldownOverlay").gameObject.SetActive(true);
            this.transform.Find("CooldownOverlay/CooldownText").GetComponent<TMPro.TMP_Text>().text = curCooldown.ToString("F1");
            // this.transform.Find("CooldownOverlay").GetComponent<Image>().fillAmount = curCooldown/Mathf.Max(maxCooldown, 0.01f);
        }
        else {
            this.transform.Find("CooldownOverlay").gameObject.SetActive(false);
        }
    }
}
