using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{	
	bool endOfWorld, foodAhead, obstacle, enemy, hasFood, atBase;

	void Start() {
		endOfWorld = false;
		foodAhead = false;
		obstacle = false;
		enemy = false;
		hasFood = false;
		atBase = false;
	}

	void Update() {
		if (EndOfWorld ()) {
			SendBack ();
		} else if (FoodAhead () && !HasFood ()) {
			PickFood ();
		} else if (AtBase () && HasFood ()) {
			DropFood ();
		} else {
			MoveRandomly ();
		}
	}

	bool EndOfWorld() {
		return endOfWorld;
	}

	public void SetEndOfWorld() {
		endOfWorld = true;
	}

	// Box collider (mundo) chama esta funcao quando o agente sai
	public void SendBack() {
		transform.Rotate (0f, 180f, 0f);
		endOfWorld = false;
		return;
	}

	bool FoodAhead (){
		return foodAhead;
	}

	bool HasFood ()	{
		return hasFood;
	}

	bool AtBase (){
		return atBase;
	}

	void PickFood (){
		return;
	}

	void DropFood (){
		return;
	}

	void MoveRandomly (){
		int rand = Random.Range(1,1000);
		if (rand <= 2) {
			transform.Rotate (0f,-90f,0f);
		} else if(rand <= 4) {
			transform.Rotate (0f,90f,0f);
		} else {
			MoveForward();
		}
	}

	void MoveForward() {
		transform.Translate (Vector3.forward * Time.deltaTime, Space.Self);
	}
}

