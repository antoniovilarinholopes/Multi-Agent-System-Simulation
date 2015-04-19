using UnityEngine;
using System.Collections.Generic;

public class Colony : MonoBehaviour {
	
	public GameObject prefabInd;
	IList<GameObject> individuals;

	void Awake() {
		individuals = new List<GameObject>();

		GameObject individual;
		Vector3 position;
		int rotation;
		int maxAttempts;
		bool clearSpace;
		Color individualColor;
		string playerTag;
		if(gameObject.tag == "ColA") {
			individualColor = Color.blue;
			playerTag = "PlayerA";
		} else {
			individualColor = Color.red;
			playerTag = "PlayerB";
		}
		for(int i = 0; i < 4; i++) {
			maxAttempts = 0;
			do {
				position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
				position += transform.position;

				clearSpace = Physics.CheckSphere(position, 1f);
			} while (maxAttempts < 100 && !clearSpace);

			individual = Instantiate(prefabInd, position, Quaternion.identity) as GameObject;
			rotation = Random.Range(0, 4) * 90;
			individual.transform.Rotate(0f, rotation, 0f);
			individual.tag = playerTag;
			// Muda a cor do Robot2
			individual.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Renderer>().material.color = individualColor;
			individuals.Add (individual);
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}