using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 0.02f;
    public float rotateSpeed = 10f;

    void Update()
    {
        // Input.GetAxis will return values between -1 and 1 based on user input.
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement direction based on user input.
        Vector3 movement = new Vector3(horizontalInput, 0.0f, verticalInput);

        // Normalize the movement vector to ensure consistent speed in all directions.
        movement.Normalize();

        // Translate the object's position using the Rigidbody component.
        // Multiply by moveSpeed to control the movement speed.
        transform.Translate(movement * moveSpeed * Time.deltaTime);
 
        // movement W,A,S,D

        // Rotate the object
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up, -rotateSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E))
        {            
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
        // change the height new Vector3(0.0f, 1.0f, 0.0f)
        if (Input.GetKey(KeyCode.R))
        {            
            transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.F))
        {            
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
        }                
    }
}
