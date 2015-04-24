using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{	
	bool endOfWorld, foodAhead, obstacle, enemyInFront, hasFood, atBase, wallAhead, agentAhead;
	bool isFoodOnSight, isEnemyOnSight, isObstacleOnSight, isColonyOnSight;
	GameObject foodOnSight, obstacleOnSight, enemyOnSight, colonyOnSight;
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
		isFoodOnSight = false;
		isEnemyOnSight = false;
		isObstacleOnSight = false;
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
		} else if (FoodOnSight()) {
			PursueFood();
		}else if (EnemyOnSight()) {
			EvadeMonster();
		} else if (ObstacleOnSight()) {
			PursueObstacle();
		} else {
			MoveRandomly ();
		}
		CleanSight();
	}

	/*
	 * Sensors
	 */

	bool EndOfWorld() {
		return endOfWorld;
	}

	bool AgentAhead () {
		return agentAhead;
	}

	bool FoodAhead (){
		return foodAhead;
	}

	bool HasFood ()	{
		return hasFood;
	}

	bool AtBase () {
		return atBase;
	}

	bool EnemyAhead() {
		return enemyInFront;
	}

	bool WallAhead() {
		return wallAhead;
	}

	bool FoodOnSight () {
		return isFoodOnSight;
	}

	bool EnemyOnSight () {
		return isEnemyOnSight;
	}

	bool ObstacleOnSight ()	{
		return isObstacleOnSight;
	}

	/*
	 * Actuators
	 */
	// TODO: Do we need the public here?
	public void SendBack() {
		transform.Rotate (0f, 180f, 0f);
		endOfWorld = false;
		return;
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

	void PickFood (){
		this.foodAhead = false;
		this.hasFood = true;
	}

	void DropFood() {
		hasFood = false;
		Destroy(food);
		Colony colonyComp = colony.GetComponent<Colony>();
		if(colonyComp != null) {
			colonyComp.IncreaseScore();
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

	private void HitWall() {
		if(wallObj != null) {
			Wall wall = wallObj.GetComponent<Wall>();
			wall.HitWall();
		} else {
			wallAhead = false;
		}
	}

	void PursueFood () {
		Pursue(this.foodOnSight);
	}

	void EvadeMonster () {
		EvadeAgent();
	}

	void PursueObstacle () {
		Pursue(this.obstacleOnSight);
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

	/*
	 * Auxiliar
	 */

	void OnTriggerEnter(Collider collider) {
		if(collider.gameObject.tag == "PlayerA" || collider.gameObject.tag == "PlayerB") {
			//Debug.Log ("Collision");
			agentAhead = true;
		}
	}

	void RunFromEnemy() {
		enemy = null;
		enemyInFront = false;
		SendBack ();
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

	void CleanSight () {
		isFoodOnSight = false;
		isEnemyOnSight = false;
		isObstacleOnSight = false;
	}

	void Pursue(GameObject target) {
		pursuing = true;
		Vector3 targetDir = target.transform.position - transform.position;
		float step  = smooth * Time.deltaTime;
		Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
		transform.rotation = Quaternion.LookRotation(newDir);
		MoveForward();
	}

	/*
	 * Public (Being called outside Agent)
	 */

	// Setters
	public void SetIsFoodOnSight (bool isFoodOnSight, GameObject foodOnSight) {
		this.isFoodOnSight = isFoodOnSight;
		this.foodOnSight = foodOnSight;
	}

	public void SetIsEnemyOnSight (bool isEnemyOnSight, GameObject enemyOnSight) {
		this.isEnemyOnSight = isEnemyOnSight;
		this.enemyOnSight = enemyOnSight;
	}

	public void SetIsObstacleOnSight (bool isObstacleOnSight, GameObject obstacleOnSight) {
		this.isObstacleOnSight = isObstacleOnSight;
		this.obstacleOnSight = obstacleOnSight;
	}

	public void SetIsColonyOnSight (bool isColonyOnSight, GameObject colonyOnSight) {
		this.isColonyOnSight = isColonyOnSight;
		this.colonyOnSight = colonyOnSight;
	}

	public void SetEndOfWorld() {
		endOfWorld = true;
	}
	
	public void SetEnemyInFront(GameObject enemy) {
		enemyInFront = true;
		this.enemy = enemy;
	}
	
	// Box collider (mundo) chama esta funcao quando o agente sai
	public void SetWallAhead(GameObject wall) {
		wallAhead = true;
		this.wallObj = wall;
	}

	public void SetFood(GameObject food) {
		this.food = food;
		this.foodAhead = true;
	}

	public void SetAtBase (bool atBase, GameObject colony)	{
		this.atBase = atBase;
		this.colony = colony;
	}

	// Called by monsters
	public void TakeDamage() {
		hitPoints -= Time.deltaTime * hitRate;
		Debug.Log ("Agent Hitpoints: " + hitPoints);
		if (hitPoints <= 0) {
			Debug.Log ("Agent Died");
			if(HasFood()) {
				this.food.GetComponent<PickUpable>().SetBeingCarried(false);
			}
			Object.Destroy(this.gameObject);
		}
	}
}

