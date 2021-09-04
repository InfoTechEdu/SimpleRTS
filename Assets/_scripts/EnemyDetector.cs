//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class EnemyDetector : MonoBehaviour
//{
//    readonly float chechkingDelay = 2f;
//    float lastCheckedAt;

//    private void Start()
//    {
//        lastCheckedAt = Time.time;
//    }


//    private void Update()
//    {
//        if (Time.time - lastCheckedAt < chechkingDelay)
//        {
//            return;
//        }

//        Collider[] colliders = Physics.OverlapSphere(transform.position, 20f);
//        foreach (var c in colliders)
//        {
//            if(IsEnemyUnit(c.transform))
//                transform.parent.GetComponent<UnitRTS>().OnEnemyDetected(c.transform);
//        }

//        lastCheckedAt = Time.time;
//    }

//    //private void OnTriggerEnter(Collider other)
//    //{
//    //    Debug.Log($"Temp log. Trigger enter. {transform.parent.name}. Triggered: {other.name}");

//    //    if(IsEnemyUnit(other.transform))
//    //        transform.parent.GetComponent<UnitRTS>().OnEnemyDetected(other.transform);
//    //}

//    private bool IsEnemyUnit(Transform other)
//    {
//        return other.GetComponent<UnitRTS>() != null && other.tag != transform.parent.tag;
//    }
//}
