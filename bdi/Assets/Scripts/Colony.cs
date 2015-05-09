﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Colony : MonoBehaviour {
	
	public GameObject prefabInd;
	IList<GameObject> individuals;
	float foodCount;
	const float foodMultiplier = 5f;
	const float specialFoodMultiplier = 10f;
	const float timeToRemoveHealth = 10f;
	float minLimitFoodToPopulate = 20f;
	string colonyLetter;

	void Awake () {
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
		for(int i = 0; i < 8; i++) {
			maxAttempts = 0;
			// TODO: Ver clearSpace
			do {
				position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
				position += transform.position;

				clearSpace = Physics.CheckSphere(position, 1f);
			} while (maxAttempts < 100 && !clearSpace);

			individual = Instantiate(prefabInd, position, Quaternion.identity) as GameObject;
			rotation = Random.Range(0, 4) * 90;
			individual.transform.Rotate(0f, rotation, 0f);
			individual.tag = playerTag;
			Move indComponent = individual.GetComponent<Move> ();
			indComponent.SetColonyPosition(this.transform.position);
			// Muda a cor do Robot2
			individual.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Renderer>().material.color = individualColor;
			individuals.Add (individual);
		}
	}

	// Use this for initialization
	void Start () {
		foodCount = 0;
		//Add colony to each of them
		/*foreach (GameObject individual in individuals) {
			if (individual == null) {
				continue;
			}
			Move indComponent = individual.GetComponent<Move> ();
			indComponent.SetColonyPosition(this.transform.position);
		}*/
	}

	void Update () {
		IList<GameObject> individualsAtBase = new List<GameObject>();
		int individualsAtBaseCount = 0;
		foreach (GameObject ind in individuals) {
			if (ind == null) {
				individuals.Remove(ind);
				continue;
			}
			Move indComponent = ind.GetComponent<Move> ();
			if(indComponent.AtBase()) {
				individualsAtBase.Add(ind);
				individualsAtBaseCount++;
			}
		}

		//If has plenty of food and at least 2 are at base, populate new inds
		if (HasFoodToPopulate () && individualsAtBaseCount >= 2) {
			minLimitFoodToPopulate += 2f;
			Color individualColor;
			string playerTag;
			if(gameObject.tag == "ColA") {
				individualColor = Color.blue;
				playerTag = "PlayerA";
			} else {
				individualColor = Color.green;
				playerTag = "PlayerB";
			}
			int maxAttempts = 0;
			bool clearSpace;
			Vector3 position;
			do {
				position = new Vector3(Random.Range(-10f, 10f), 0, Random.Range(-10f, 10f));
				position += transform.position;
				
				clearSpace = Physics.CheckSphere(position, 1f);
			} while (maxAttempts < 100 && !clearSpace);
			
			GameObject individual = Instantiate(prefabInd, position, Quaternion.identity) as GameObject;
			int rotation = Random.Range(0, 4) * 90;
			individual.transform.Rotate(0f, rotation, 0f);
			individual.tag = playerTag;
			// Muda a cor do Robot2
			individual.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Renderer>().material.color = individualColor;
			this.individuals.Add (individual);
		}

		if (foodCount > 0) { 
			foreach (GameObject ind in individualsAtBase) {
				if (ind == null) {
					continue;
				}
				Move indComponent = ind.GetComponent<Move> ();
				if (indComponent.HasLowLife () && foodCount > 0) {
					indComponent.EatFood(5f);
					foodCount -= 5f;
				}
			}
		}
	}

	public void IncreaseFood (string food_tag) {
		if (food_tag == "Food") {
			foodCount += 1*foodMultiplier;
		} else {
			foodCount += 1*specialFoodMultiplier;
		}
		//foodCount += 1*foodMultiplier;
		Debug.Log ("Score: " + foodCount);
		if(colonyLetter == "A") {
			Text text = GameObject.Find("PointsA").GetComponent<Text>();
			text.text = "" + foodCount;
		} else {
			Text text = GameObject.Find("PointsB").GetComponent<Text>();
			text.text = "" + foodCount;
		}
	}


	public bool HasFoodToPopulate () {
		return foodCount >= minLimitFoodToPopulate;
	}


	void OnTriggerExit (Collider collider) {
		if(collider.gameObject.tag.StartsWith("Player") && collider.gameObject.tag.Substring(6) == colonyLetter) {
			Move move = collider.GetComponent<Move>();
			move.SetAtBase(false, null);
		}
	}

	void OnTriggerEnter (Collider collider) {
		//Debug.Log ("Enter " + collider.gameObject.tag.Substring(6));
		if(collider.gameObject.tag.StartsWith("Player") && collider.gameObject.tag.Substring(6) == colonyLetter) {
			Move move = collider.GetComponent<Move>();
			move.SetAtBase(true, gameObject);
		}
	}

}