using UnityEngine;
using System.Collections;

public class SetRandomColor : MonoBehaviour 
{	
	// Initializes things
	void Start () 
	{
		//GetComponent<Renderer>().material.color = new Color(Random.value, Random.value, Random.value);
        GetComponent<Renderer>().material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
    }
}
