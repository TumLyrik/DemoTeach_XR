using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(AudioSource))]
public class triggerEnter : MonoBehaviour {


	public Collider target;
	private int numberOfHits = 0; // Note how declaring the “numberOfHits” variable as private  won’t make it show up in the Inspector
	public AudioClip beep;
    private Renderer objectRenderer; // 用于访问 Mesh 的材质
    AudioSource audioSource;
    public Collider target_tcp;
    private Renderer objectRenderer_tcp; // 用于访问 Mesh 的材质

    // Use this for initialization
    void Start () {
		audioSource = GetComponent<AudioSource>();
        objectRenderer = target.GetComponent<Renderer>();
        objectRenderer_tcp = target_tcp.GetComponent<Renderer>();
        // 确保对象有材质，否则抛出警告
        if (objectRenderer == null)
        {
            Debug.LogWarning("No Renderer found on the object. Please add a Mesh Renderer.");
        }
    }

	// This script is only triggered upon entering the trigger zone – “OnTriggerEnter” – so we don’t need 
	// to worry about multiple counts per visit in the trigger zone.
	void OnTriggerEnter(Collider cubeTrigger) {
		if (cubeTrigger == target)  
		{
			numberOfHits = numberOfHits + 1;
			audioSource.PlayOneShot(beep, 0.7F);
			print("Bumped: " + numberOfHits + " times!");
            if (objectRenderer != null)
            {
                objectRenderer.material.color = new Color(1.0f, 0.2f, 0.2f);
                objectRenderer_tcp.material.color = new Color(1.0f, 0.2f, 0.2f);
            }
        }
	}

    private void OnTriggerExit(Collider cubeTrigger)
    {
        if (cubeTrigger == target)
        {
            if (objectRenderer != null)
            {
                objectRenderer.material.color = new Color(1.0f, 1.0f, 1.0f);
                objectRenderer_tcp.material.color = new Color(1.0f, 1.0f, 1.0f);
            }
        }
    }

    //If you would like to render the trigger zone invisible just uncheck the game object’s 
    //“Mesh Renderer” property in the Inspector, resp. set the Mesh Renderer to "none".

    // Update is called once per frame
    void Update () {

    }
}
