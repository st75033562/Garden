using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour {
	
	public Vector3 rotationOffset;

	private Transform cachedTransform;

	void Awake () 
	{
		cachedTransform = transform;
	}
	
	void Update () 
	{
		cachedTransform.Rotate(rotationOffset*Time.deltaTime);
	}
}
