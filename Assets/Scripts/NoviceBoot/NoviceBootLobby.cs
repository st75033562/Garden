using UnityEngine;
using System.Collections;

public class NoviceBootLobby : MonoBehaviour {
    public GameObject go;
	// Use this for initialization
	void Start () {
        Instantiate(go, go.transform.position, go.transform.rotation, transform);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
