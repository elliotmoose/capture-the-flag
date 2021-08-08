using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapRadarIcon : MonoBehaviour
{
    public LocalPlayer ref_target;
    public LocalPlayer target;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!target) return;
        float dist = Mathf.Pow(Mathf.Pow((float)(target.transform.position.x - ref_target.transform.position.x), 2) + Mathf.Pow((float)(target.transform.position.y - ref_target.transform.position.y), 2) + Mathf.Pow((float)(target.transform.position.z - ref_target.transform.position.z), 2), (float)0.5);
        bool isInsideMinimap = CheckInMinimap(dist);
        float x,y,z;
        if (dist != 0)
        {
            x = (float)(target.transform.position.x - ref_target.transform.position.x)/dist;
            y = (float)(target.transform.position.y - ref_target.transform.position.y)/dist;
            z = (float)(target.transform.position.z - ref_target.transform.position.z)/dist;
            
            Debug.Log("WHEE");
            //Debug.Log(target.transform.position);
            //Debug.Log(ref_target.transform.position);
            //Debug.Log(new Vector3(x,y,z));
            //Debug.Log(dist);
        }
        else
        {
            x = (float)Mathf.Sqrt(1);
            y = (float)Mathf.Sqrt(1);
            z = (float)Mathf.Sqrt(1);
        }

        // Change rotation of triangle here as well.. 
        Vector2 toVector = new Vector2(x, z);
        float angle = Vector2.SignedAngle(Vector2.up, toVector);
        this.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0.0f, 0.0f, angle);

        GetComponent<Image>().color = (target.team == Team.BLUE ? UIManager.Instance.colors.textBlue : UIManager.Instance.colors.textRed);
        if (!target.isInvisToLocalPlayer)
        {
            if (isInsideMinimap)
            {
                this.GetComponent<RectTransform>().position = ref_target.transform.position + new Vector3(x * 3, y * 3, z * 3);
                GetComponent<Image>().enabled = false;
            }
            else
            {
                this.GetComponent<RectTransform>().position = ref_target.transform.position + new Vector3(x * 45, y * 45, z * 45);
                GetComponent<Image>().enabled = true;
            }
        }
        else
        {
            GetComponent<Image>().enabled = !target.isInvisToLocalPlayer;
        }
    }

    bool CheckInMinimap(float dist)
    {
        
        if(dist >= 50.0f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

}
