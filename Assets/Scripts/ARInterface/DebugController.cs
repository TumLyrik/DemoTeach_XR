using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugController : MonoBehaviour
{
    public GameObject origin;
    public Movement movementSolderingStation;
    bool debugging = true;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.X))
        {
            debugging = !debugging;
            origin.SetActive(debugging);
            movementSolderingStation.enabled = debugging;
        }
    }
}
