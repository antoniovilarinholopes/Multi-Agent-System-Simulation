using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{	
	bool endOfWorld, foodAhead, obstacle, enemyInFront, hasFood, atBase, wallAhead, agentAhead;
	GameObject food, enemy, wallObj, colony;
	public float distance, smooth;
	private float hitPoints = 20f;
	private float hitRate = 2f;
	private const float SPEED = 10f;
	
	void Start() {
		endOfWorld = false;
		foodAhead = false;
		obstacle = false;
		enemyInFront = false;
		hasFood = false;
		atBase = false;
		enemyInFront = false;
		agentAhead = false;
	}

	/*
	 * Agent main loop
	 * Purely reactive Agent
	 * Function inside if's are sensors and the if's body has effectors
	 */
	void Update() {
		if (EndOfWorld ()) {
			SendBack ();
		} else if (AgentAhead()) {
			EvadeAgent();
		} else if (FoodAhead () && !HasFood ()) {
			PickFood ();
		} else if (AtBase () && HasFood ()) {
			DropFood ();
		} else if (EnemyAhead ()){
			HitEnemy();
		} else if (WallAhead()) {
			HitWall();
		} else {
			MoveRandomly ();
		}
	}

	void OnTriggerEnter(Collider collider) {
		if(collider.gameObject.tag == "PlayerA" || collider.gameObject.tag == "PlayerB") {
			//Debug.Log ("Collision");
			agentAhead = true;
		}
	}

	private void HitWall() {
		if(wallObj != null) {
			Wall wall = wallObj.GetComponent<Wall>();
			wall.HitWall();
		} else {
			wallAhead = false;
		}
	}

	private void HitEnemy() {
		if (enemy != null) {
			Monster monster = enemy.GetComponent<Monster> ();
			monster.TakeDamage (gameObject);
		} else {
			enemyInFront = false;
		}
	}

	public void TakeDamage() {
		hitPoints -= Time.deltaTime * hitRate;
		Debug.Log ("Agent Hitpoints: " + hitPoints);
		if (hitPoints <= 0) {
			Debug.Log ("Agent Died");
			if(HasFood()) {
				this.food.GetComponent<PickUpable>().SetBeignCarried(false);
			}
			Object.Destroy(this.gameObject);
		}
	}
	
	void RunFromEnemy() {
		enemy = null;
		enemyInFront = false;
		SendBack ();
		//MoveForward ();
	}

	bool AgentAhead ()
	{
		return agentAhead;
	}

	bool EnemyAhead() {
		return enemyInFront;
	}
	bool EndOfWorld() {
		return endOfWorld;
	}

	bool WallAhead() {
		return wallAhead;
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

	public void SetWallAhead(GameObject wall) {
		wallAhead = true;
		this.wallObj = wall;
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
		transform.Translate (Vector3.forward * Time.deltaTime * SPEED, Space.Self);
		// Move food along with him
		if(HasFood()) {
			Vector3 currentPosition = food.transform.position;
			Vector3 newPosition = transform.position +
				transform.TransformDirection(Vector3.forward) * distance;

			this.food.transform.position = 
				Vector3.Lerp (currentPosition, newPosition, Time.deltaTime*smooth);
		}
	}

	void EvadeAgent () {
		int rand = Random.Range(1, 4);
		if (rand <= 2) {
			transform.Rotate (0f,-90f,0f);
		} else {
			transform.Rotate (0f,90f,0f);
		}
		agentAhead = false;
	}

	void DropFood() {
		hasFood = false;
		Destroy(food);
		Colony colonyComp = colony.GetComponent<Colony>();
		if(colonyComp != null) {
			colonyComp.IncreaseScore();
		}
	}

	public void SetAtBase (bool atBase, GameObject colony)	{
		this.atBase = atBase;
		this.colony = colony;
	}
}

