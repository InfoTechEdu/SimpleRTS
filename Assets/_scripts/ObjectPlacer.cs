using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ObjectPlacer : MonoBehaviour
{
    public LayerMask mask;

    GameObject currentPlacing;
    bool placing = false;

    public GameObject CurrentPlacing { get => currentPlacing; set => currentPlacing = value; }
    public bool Placing { get => placing; set => placing = value; }

    public void SetObject(GameObject placingObject)
    {
        currentPlacing = placingObject;
        Debug.Log("Object for placing was set");
        
        //currentPlacing.transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void Update()
    {
        if (currentPlacing == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, mask))
        {
            if (!placing)
            {
                placing = true;
                return;
            }

            currentPlacing.transform.position = hit.point;

            if (Input.GetMouseButtonDown(0))
            {
                Put();
                RebakeNavMesh();
            }
        }
        else
        {
            Debug.Log("Not raycasted");
        }
    }

    private void Put()
    {
        currentPlacing = null;
        placing = false;
        Debug.Log("Placed");
    }
    private void RebakeNavMesh()
    {
        FindObjectOfType<NavMeshSurface>().BuildNavMesh();
    }
}
