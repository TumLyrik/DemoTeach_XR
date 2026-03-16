using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        { Debug.Log("start Collision"); }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.name == "Player")
        { Debug.Log("in Collision"); }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Player")
        { Debug.Log("leave Collision"); }
    }
}
