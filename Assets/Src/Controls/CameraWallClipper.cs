using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWallClipper : MonoBehaviour
{
    // Start is called before the first frame update
    List<MeshRenderer> lastFrameMeshRenderers = new List<MeshRenderer>();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach(MeshRenderer renderer in lastFrameMeshRenderers) {
            renderer.enabled = true;
        }

        lastFrameMeshRenderers.Clear();
        
        if(!PlayerController.LocalInstance) return;
        LocalPlayer player = PlayerController.LocalInstance.GetPlayer();
        if(!player) return;
        List<float> rayAngles = new List<float>{0};
        float rayDistanceFraction = 0.95f; //fix bugs where the wall is infront of player but still gets culled
        foreach(float angle in rayAngles) {
            RaycastHit[] hits = Physics.RaycastAll(player.transform.position, Quaternion.Euler(0, angle, 0) * this.transform.rotation * Vector3.back, (this.transform.position - player.transform.position).magnitude * rayDistanceFraction);
            DeactivateMeshRenderers(hits);            
        }
    }

    void DeactivateMeshRenderers(RaycastHit[] hits) {
        foreach(RaycastHit hit in hits) {
            if(hit.collider.gameObject.layer != LayerMask.NameToLayer("Terrain")) continue;
            if(hit.collider.gameObject.GetComponentInChildren<IgnoreTerrainClipping>()) continue;
            MeshRenderer[] hitRenderers = hit.collider.gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach(MeshRenderer renderer in hitRenderers) {
                if(renderer.enabled) {
                    renderer.enabled = false;
                    lastFrameMeshRenderers.Add(renderer);
                }
            }
        }
    }
}
