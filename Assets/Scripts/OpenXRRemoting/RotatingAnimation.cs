using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingAnimation : MonoBehaviour
{
    public float speed=20.0f;

    void Update()
    {
        transform.Rotate(transform.forward * speed * Time.deltaTime);
    }
}
