using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Colony : MonoBehaviour {
	
	public GameObject prefabInd;
	public GameObject prefabSpecFood;
	IList<GameObject> individuals;
	float foodCount;
	const float foodMultiplier = 5f;
	const float specialFoodMultiplier = 15f;
	float minLimitFoodToPopulate = 20f;
	bool isUnderAttack;
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

		//FIXME
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
			indComponent.SetColor(individualColor);
//			Move indComponent = individual.GetComponent<Move> ();
			// Muda a cor do Robot2
			individual.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Renderer>().material.color = individualColor;
			individuals.Add (individual);
		}
	}

	// Use this for initialization
	void Start () {
		foodCount = 0;
		isUnderAttack = false;
		//Add colony to each of them
		IList<GameObject> indList = new List<GameObject>(individuals);
		foreach(GameObject ind in indList) {
			if (ind == null) {
				individuals.Remove(ind);
				continue;
			}
			Move indComponent = ind.GetComponent<Move> ();
			indComponent.SetColonyPosition(this.transform.position);
			indComponent.SetMyColony(this.gameObject);
		}
		// Create special food and start auction every 20 secs
		InvokeRepeating ("CreateSpecFood", 20f, 20f);
	}

	void Update () {
		IList<GameObject> individualsAtBase = new List<GameObject>();
		int individualsAtBaseCount = 0;
		IList<GameObject> indList = new List<GameObject>(individuals);
		foreach(GameObject ind in indList) {
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
			minLimitFoodToPopulate += 4.0f;
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
			Move indComponent = individual.GetComponent<Move> ();
			indComponent.SetColonyPosition(this.transform.position);
			indComponent.SetColor(individualColor);
			indComponent.SetMyColony(this.gameObject);
			// Muda a cor do Robot2
			individual.transform.GetChild(0).GetChild(0).gameObject.GetComponent<Renderer>().material.color = individualColor;
			this.individuals.Add (individual);
		}

		if (foodCount > 0) { 
			foreach(GameObject ind in indList) {
				if (ind == null) {
					individuals.Remove(ind);
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


	public bool IsUnderAttack () {
		return isUnderAttack;
	}

	public float HowManyAtBase () {
		float individualsAtBaseCount = 0f;
		IList<GameObject> indList = new List<GameObject>(individuals);
		foreach(GameObject ind in indList) {
			if (ind == null) {
				individuals.Remove(ind);
				continue;
			}
			Move indComponent = ind.GetComponent<Move> ();
			if(indComponent.AtBase()) {
				individualsAtBaseCount++;
			}
		}
		return individualsAtBaseCount;
	}

	public void SetIsUnderAttack(bool isUnderAttack) {
		this.isUnderAttack = isUnderAttack;
	}

	public bool HasFoodToPopulate () {
		return foodCount >= minLimitFoodToPopulate;
	}


	void OnTriggerExit (Collider collider) {
		if(collider.gameObject.tag.StartsWith("Player") && collider.gameObject.tag.Substring(6) == colonyLetter) {
			Move move = collider.GetComponent<Move>();
			move.SetAtBase(false, null);
		}
		if (collider.gameObject.tag == "Monster") {
			isUnderAttack = false;
		}
	}

	void OnTriggerEnter (Collider collider) {
		if(collider.gameObject.tag.StartsWith("Player") && collider.gameObject.tag.Substring(6) == colonyLetter) {
			Move move = collider.GetComponent<Move>();
			move.SetAtBase(true, gameObject);
		}
		if (collider.gameObject.tag == "Monster") {
			isUnderAttack = true;
			Monster m = collider.GetComponent<Monster>();
			m.SetInColony(true, gameObject);
		}
	}

	/*
	void OnTriggerStay (Collider collider) {
		if(collider.gameObject.tag.StartsWith("Player") && collider.gameObject.tag.Substring(6) == colonyLetter) {
			Move move = collider.GetComponent<Move>();
			move.SetAtBase(true, gameObject);
		}
		if (collider.gameObject.tag == "Monster") {
			isUnderAttack = true;
			Monster m = collider.GetComponent<Monster>();
			m.SetInColony(true, gameObject);
		}
	}*/

	public void Broadcast (SpeechAtc speechAct, string tag, Vector3 obj) {
		if(speechAct == SpeechAtc.REQUEST_ADD) {
			AuctionHelp(obj);
			return;
		}
		IList<GameObject> indList = new List<GameObject>(individuals);
		foreach(GameObject ind in indList) {
			if (ind == null) {
				individuals.Remove(ind);
				continue;
			}
			Move moveComp = ind.GetComponent<Move>();
			switch (speechAct) {
				case SpeechAtc.INFORM_ADD: 
					moveComp.AddToBeliefs(tag, obj);
					break;
				case SpeechAtc.INFORM_REMOVE:
					moveComp.RemoveBelief(tag, obj);
					break;
				default:
					Debug.Log (speechAct + ":" + tag + ":" + obj);
					break;
			}
		}
	}

	void CreateSpecFood () {
		// Creates a special food somewhere in the map
		float randX = Random.Range(-100, 100);
		float randZ = Random.Range(-50, 50);
		Vector3 position = new Vector3(randX, 1.5f, randZ);
		GameObject newSpecFood = Instantiate(prefabSpecFood, position, Quaternion.identity) as GameObject;

		BeginAuction(newSpecFood);
	}


	void AuctionHelp (Vector3 objPosition) {
		/*float bid = 0.0f;
		float bestBid = float.MinValue;
		ComunicationModule bestIndividual = null;
		IList<GameObject> indList = new List<GameObject>(individuals);
		foreach(GameObject ind in indList) {
			if (ind == null) {
				individuals.Remove(ind);
				continue;
			}
			ComunicationModule comm = ind.GetComponent<ComunicationModule>();
			bid = comm.RequestBid(objPosition);
			if(bestBid < bid) {
				bestBid = bid;
				bestIndividual = comm;
			}
		}*/

		ComunicationModule bestIndividual = BestBid (objPosition);
		if(individuals.Count > 0) {
			bestIndividual.ReceiveHelpRequest(objPosition);
		}
	}


	ComunicationModule BestBid (Vector3 objPosition) {
		float bid = 0.0f;
		float bestBid = float.MinValue;
		ComunicationModule bestIndividual = null;
		IList<GameObject> indList = new List<GameObject>(individuals);
		foreach(GameObject ind in indList) {
			if (ind == null) {
				individuals.Remove(ind);
				continue;
			}
			ComunicationModule comm = ind.GetComponent<ComunicationModule>();
			bid = comm.RequestBid(objPosition);
			if(bestBid < bid) {
				bestBid = bid;
				bestIndividual = comm;
			}
		}
		return bestIndividual;
	}



	void BeginAuction(GameObject specFood) {
		Vector3 specFoodPosition = specFood.transform.position;
		/*float bid;
		float bestBid = 0;
		ComunicationModule bestIndividual = null;
		IList<GameObject> indList = new List<GameObject>(individuals);
		foreach(GameObject ind in indList) {
			if (ind == null) {
				individuals.Remove(ind);
				continue;
			}
			ComunicationModule comm = ind.GetComponent<ComunicationModule>();
			bid = comm.RequestBid(specFoodPosition);
			if(bestBid < bid) {
				bestBid = bid;
				bestIndividual = comm;
			}
		}*/
		ComunicationModule bestIndividual = BestBid (specFoodPosition);
		if(individuals.Count > 0) {
			// Add special food to best Agent beliefs
			bestIndividual.GetSpecialFood(specFoodPosition);
		}
	}

}