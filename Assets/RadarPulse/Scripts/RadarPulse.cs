using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarPulse : MonoBehaviour {

    [SerializeField] private Transform pfRadarPing;
    [SerializeField] private LayerMask radarLayerMask;

    private Transform pulseTransform;
    private float range;
    private float rangeMax;
    private float rangeSpeed;
    private float fadeRange;
    private SpriteRenderer pulseSpriteRenderer;
    private Color pulseColor;
    private List<Collider2D> alreadyPingedColliderList;

    private void Awake() {
        pulseTransform = transform.Find("Pulse");
        pulseSpriteRenderer = pulseTransform.GetComponent<SpriteRenderer>();
        pulseColor = pulseSpriteRenderer.color;
        rangeMax = 50f;
        fadeRange = 5f;
        rangeSpeed = rangeMax;
        alreadyPingedColliderList = new List<Collider2D>();
    }

    private void Update() {
        range += rangeSpeed * Time.deltaTime;
        if (range > rangeMax) {
            range = 0f;
            alreadyPingedColliderList.Clear();
        }
        pulseTransform.localScale = new Vector3(range, range);

        RaycastHit2D[] raycastHit2DArray = Physics2D.CircleCastAll(transform.position, range / 2f, Vector2.zero, 0f, radarLayerMask);
        foreach (RaycastHit2D raycastHit2D in raycastHit2DArray) {
            if (raycastHit2D.collider != null) {
                // Hit something
                if (!alreadyPingedColliderList.Contains(raycastHit2D.collider)) {
                    alreadyPingedColliderList.Add(raycastHit2D.collider);
                    //CMDebug.TextPopup("Ping!", raycastHit2D.point);
                    Transform radarPingTransform = Instantiate(pfRadarPing, raycastHit2D.point, Quaternion.identity);
                    RadarPing radarPing = radarPingTransform.GetComponent<RadarPing>();
                    Debug.Log("HELLO");
                    if (raycastHit2D.collider.gameObject.GetComponent<Player>() != null) {
                        // Hit a Player
                        Debug.Log("Hit Player!");
                        radarPing.SetColor(new Color(1, 0, 0));
                    }
                    radarPing.SetDisappearTimer(rangeMax / rangeSpeed * 0.5f);
                }
            }
        }

        // Fade Pulse
        if (range > rangeMax - fadeRange) {
            pulseColor.a = Mathf.Lerp(0f, 1f, (rangeMax - range) / fadeRange);
        } else {
            pulseColor.a = 1f;
        }
        pulseSpriteRenderer.color = pulseColor;
        
        // Key Controls
        if (Input.GetKeyDown(KeyCode.K)) {
            rangeMax += 30f;
            Debug.Log("rangeMax: " + rangeMax);
        }
        if (Input.GetKeyDown(KeyCode.J)) {
            rangeMax -= 30f;
            Debug.Log("rangeMax: " + rangeMax);
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            rangeSpeed += 30f;
            Debug.Log("rangeSpeed: " + rangeSpeed);
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            rangeSpeed -= 30f;
            Debug.Log("rangeSpeed: " + rangeSpeed);
        }
    }

}
