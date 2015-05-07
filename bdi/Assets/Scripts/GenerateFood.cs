using UnityEngine;
using System.Collections;

public class GenerateFood : MonoBehaviour {

	public GameObject foodPrefab;
	float spawnTime = 1f;
	Vector3[] spawnPoints; 

	// Use this for initialization
	void Start () {
		Vector3 pointForward = this.transform.position + Vector3.forward;
		Vector3 pointLeft = this.transform.position + Vector3.left;
		Vector3 pointRight = this.transform.position + Vector3.right;
		Vector3 pointBack = this.transform.position + Vector3.back;
		spawnPoints = new Vector3[]{pointForward, pointRight, pointLeft, pointBack};
		InvokeRepeating ("SpawnFood", spawnTime, spawnTime);
	}

	void SpawnFood () {
		int spawnPointIndex = Random.Range (0, spawnPoints.Length);
		//FIXME not working why?
		bool clearSpace = Physics.CheckSphere (spawnPoints[spawnPointIndex], 2f);
		if (clearSpace) {
			Instantiate (foodPrefab, spawnPoints[spawnPointIndex], Quaternion.identity);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
