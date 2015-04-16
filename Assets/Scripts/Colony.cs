using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Colony : MonoBehaviour {
	
	public GameObject prefabInd;
	IList<GameObject> individuals;
	int score;
	string colonyLetter;
	public int pointsPerFood;

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
			colonyLetter = "A";
		} else {
			individualColor = Color.green;
			playerTag = "PlayerB";
			colonyLetter = "B";
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
		score = 0;
	}

	public void IncreaseScore() {
		score += pointsPerFood;
		Debug.Log ("Score: " + score);
		if(colonyLetter == "A") {
			Text text = GameObject.Find("PointsA").GetComponent<Text>();
			text.text = "" + score;
		} else {
			Text text = GameObject.Find("PointsB").GetComponent<Text>();
			text.text = "" + score;
		}
	}

	void OnTriggerExit(Collider collider) {
		if(collider.gameObject.tag.StartsWith("Player") && collider.gameObject.tag.Substring(6) == colonyLetter) {
			Move move = collider.GetComponent<Move>();
			move.SetAtBase(false, null);
		}
	}

	void OnTriggerEnter(Collider collider) {
		//Debug.Log ("Enter " + collider.gameObject.tag.Substring(6));
		if(collider.gameObject.tag.StartsWith("Player") && collider.gameObject.tag.Substring(6) == colonyLetter) {
			Move move = collider.GetComponent<Move>();
			move.SetAtBase(true, gameObject);
		}
	}
}