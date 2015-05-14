using UnityEngine;
using System.Collections;

public class GenerateFood : MonoBehaviour {

	public GameObject foodPrefab;
	float spawnTime = 5.0f;
	Vector3[] spawnPoints; 

	// Use this for initialization
	void Start () {
		float mul_factor = 3.0f;
		Vector3 pointForward = this.transform.position + Vector3.forward*mul_factor;
		Vector3 pointLeft = this.transform.position + Vector3.left*mul_factor;
		Vector3 pointRight = this.transform.position + Vector3.right*mul_factor;
		Vector3 pointBack = this.transform.position + Vector3.back*mul_factor;
		spawnPoints = new Vector3[]{pointForward, pointRight, pointLeft, pointBack};
		InvokeRepeating ("SpawnFood", spawnTime, spawnTime);
	}

	void SpawnFood () {
		int spawnPointIndex = Random.Range (0, spawnPoints.Length);
		// Check if there is already Food in that position
		Collider[] hitColliders = Physics.OverlapSphere(spawnPoints[spawnPointIndex], 2f);
		bool clearSpace = true;
		foreach(Collider hitCollider in hitColliders) {
			if(hitCollider.CompareTag("Food")) {
				clearSpace = false;
				break;
			}
		}
		//Debug.Log("SpawnPoint: " + spawnPoints[spawnPointIndex] + ", Clear: " + clearSpace);
		if(clearSpace) {
			Instantiate (foodPrefab, spawnPoints[spawnPointIndex], Quaternion.identity);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
