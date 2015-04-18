using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{	
	bool endOfWorld, foodAhead, obstacle, enemyInFront, hasFood, atBase;
	GameObject food, enemy;
	public float distance, smooth;

	void Start() {
		endOfWorld = false;
		foodAhead = false;
		obstacle = false;
		enemyInFront = false;
		hasFood = false;
		atBase = false;
		enemyInFront = false;
	}

	void Update() {
		if (EndOfWorld ()) {
			SendBack ();
		} else if (FoodAhead () && !HasFood ()) {
			PickFood ();
		} else if (AtBase () && HasFood ()) {
			DropFood ();
		} else if (EnemyAhead ()){
			//for now send back 
			RunFromEnemy ();
		} else {
			MoveRandomly ();
		}
	}

	void RunFromEnemy() {
		enemy = null;
		enemyInFront = false;
		SendBack ();
		MoveForward ();
	}

	bool EnemyAhead() {
		return enemyInFront;
	}
	bool EndOfWorld() {
		return endOfWorld;
	}

	public void SetEndOfWorld() {
		endOfWorld = true;
	}

	public void SetEnemyInFront(GameObject enemy) {
		enemyInFront = true;
		this.enemy = enemy;
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

	public void SetFood(GameObject food) {
		this.food = food;
		this.foodAhead = true;
	}

	bool AtBase (){
		return atBase;
	}

	void PickFood (){
		this.foodAhead = false;
		this.hasFood = true;
	}

	void DropFood (){
		this.food = null;
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

		if(HasFood()) {
			Vector3 currentPosition = food.transform.position;
			Vector3 newPosition = transform.position +
				transform.TransformDirection(Vector3.forward) * distance;

			this.food.transform.position = 
				Vector3.Lerp (currentPosition, newPosition, Time.deltaTime*smooth);
		}
	}
}

