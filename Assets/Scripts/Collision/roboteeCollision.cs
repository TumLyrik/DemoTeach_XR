using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class roboteeCollision : MonoBehaviour
{
    private Renderer eeRenderer; // 用于访问 Mesh 的材质
    // Start is called before the first frame update
    void Start()
    {
        eeRenderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.name != "eedisc" && other.name != "Slicer")
        {
            eeRenderer.material.color = new Color(1.0f, 0.0f, 0.0f); //red
        }
    }

    private void OnTriggerExit(Collider other)
    {
        eeRenderer.material.color = new Color(0.5f, 0.5f, 0.5f); // white
    }
}
