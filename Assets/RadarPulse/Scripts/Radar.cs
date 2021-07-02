/* 
    ------------------- Code Monkey -------------------
    
    Thank you for downloading the Code Monkey Utilities
    I hope you find them useful in your projects
    If you have any questions use the contact form
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour {

    [SerializeField] private Transform pfRadarPing;
    [SerializeField] private LayerMask radarLayerMask;

    private Transform sweepTransform;
    private float rotationSpeed;
    private float radarDistance;
    private List<Collider2D> colliderList;

    private void Awake() {
        sweepTransform = transform.Find("Sweep");
        rotationSpeed = 180f;
        radarDistance = 150f;
        colliderList = new List<Collider2D>();
    }

    public static Vector3 GetVectorFromAngle(float angle)
    {
        // angle = 0 -> 360
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    private void Update() {
        float previousRotation = (sweepTransform.eulerAngles.z % 360) - 180;
        sweepTransform.eulerAngles -= new Vector3(0, 0, rotationSpeed * Time.deltaTime);
        float currentRotation = (sweepTransform.eulerAngles.z % 360) - 180;

        if (previousRotation < 0 && currentRotation >= 0) {
            // Half rotation
            colliderList.Clear();
        }

        RaycastHit2D[] raycastHit2DArray = Physics2D.RaycastAll(transform.position, GetVectorFromAngle(sweepTransform.eulerAngles.z), radarDistance, radarLayerMask);
        foreach (RaycastHit2D raycastHit2D in raycastHit2DArray) {
            if (raycastHit2D.collider != null) {
                // Hit something
                if (!colliderList.Contains(raycastHit2D.collider)) {
                    // Hit this one for the first time
                    colliderList.Add(raycastHit2D.collider);
                    //CMDebug.TextPopup("Ping!", raycastHit2D.point);
                    RadarPing radarPing = Instantiate(pfRadarPing, raycastHit2D.point, Quaternion.identity).GetComponent<RadarPing>();
                    //radarPing.transform.SetParent(transform);
                    if (raycastHit2D.collider.gameObject.GetComponent<Player>() != null) {
                        // Hit a Player
                        radarPing.SetColor(new Color(1, 0, 0));
                    }
                    radarPing.SetDisappearTimer(360f / rotationSpeed * 1f);
                }
            }
        }
        
        if (Input.GetKeyDown(KeyCode.T)) {
            rotationSpeed += 20;
            Debug.Log("rotationSpeed: " + rotationSpeed);
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            rotationSpeed -= 20;
            Debug.Log("rotationSpeed: " + rotationSpeed);
        }
    }

}
