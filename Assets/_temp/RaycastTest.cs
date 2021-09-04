using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private Ray ray;
    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        LayerMask mask = LayerMask.GetMask("Land");
        //if (Physics.Raycast(ray, out hit, 1000, mask, QueryTriggerInteraction.UseGlobal))

        
        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, mask))
        {
            Debug.Log("hit.collider - " + hit.collider);
        }
    }

    private void OnDrawGizmos()
    {
            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 100f);
    }
}
