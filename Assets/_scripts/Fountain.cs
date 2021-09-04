using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fountain : BuildingRTS
{
    public float treatRadius = 15f;
    public float treatDelay = 1.5f;
    public int treatAmount = 30;

    Vector3 size;

    private void Start()
    {
        size = GetComponent<BoxCollider>().bounds.size;

        StartCoroutine(TreatCoroutine());
    }

    private IEnumerator TreatCoroutine()
    {
        while (true)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, treatRadius + (size / 2f).magnitude);
            var filtered = colliders.Where((c) => c.transform.tag == "Defender").ToList();
            filtered.ForEach(unit => unit.GetComponent<Health>().Treat(treatAmount));

            yield return new WaitForSeconds(treatDelay);
        }
    }
}
