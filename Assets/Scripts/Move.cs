using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{	
	bool endOfWorld, foodAhead, obstacle, enemyInFront, hasFood, atBase, wallAhead, agentAhead;
	bool isFoodOnSight, isEnemyOnSight, isObstacleOnSight, isColonyOnSight;
	GameObject foodOnSight, obstacleOnSight, enemyOnSight, colonyOnSight;
	GameObject food, enemy, wallObj, colony;
	Color flashColour = Color.red;
	Color myColor;
	float flashSpeed = 5f;
	public float distance, smooth;
	private float health = 20f;
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
		myColor = transform.GetChild (0).GetChild (0).gameObject.GetComponent<Renderer> ().material.color;
	}

	/*
	 * Agent main loop
	 * Purely reactive Agent
	 * Function inside if's are sensors and the if's body has effectors
	 */
	void Update() {
		if (EndOfWorld ()) {
			SendBack ();
		} else if (AgentAhead ()) {
			//FIXME
			EvadeAgent ();
		} else if (FoodAhead () && !HasFood ()) {
			PickFood ();
		} else if (AtBase () && HasFood ()) {
			DropFood ();
		} else if (EnemyAhead ()) {
			HitEnemy ();
		} else if (WallAhead ()) {
			HitWall ();
		} else if (FoodOnSight () && !HasFood ()) {
			PursueFood ();
		} else if (HasFood () && ColonyOnSight ()){
			GotoBase ();
		} else if (ColonyOnSight () && HasLowLife ()) {
			GotoBase ();
		} else if (EnemyOnSight () && !HasFood ()) {
			TryToDestroyEnemy ();
		} else if (ObstacleOnSight () && !HasFood ()) {
			PursueObstacle ();
		} else {
			MoveRandomly ();
		}
		CleanSight ();
	}

	/*
	 * Sensors
	 */

	public bool HasLowLife () {
		return health <= 5;
	}

	bool EndOfWorld () {
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

	public bool AtBase () {
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

	bool ColonyOnSight ()	{
		return isColonyOnSight;
	}

	/*
	 * Actuators
	 */
	// TODO: Do we need the public here?
	public void SendBack () {
		transform.Rotate (0f, 180f, 0f);
		endOfWorld = false;
		return;
	}


	public void EatFood (float healHealth) {
		this.health += healHealth;
	}

	void TryToDestroyEnemy () {
		if (health > 10f) {
			PursueMonster ();
		} else {
			EvadeMonster ();
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
	
	void PickFood (){
		this.foodAhead = false;
		this.hasFood = true;
	}

	void DropFood() {
		hasFood = false;
		Destroy(food);
		food = null;
		Colony colonyComp = colony.GetComponent<Colony>();
		if(colonyComp != null) {
			colonyComp.IncreaseFood();
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

	void PursueMonster () {
		Pursue (this.enemyOnSight);
	}


	void GotoBase () {
		Pursue (this.colonyOnSight);
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
		SetIsFoodOnSight (false, null);
		SetIsEnemyOnSight (false, null);
		SetIsObstacleOnSight (false, null);
		SetIsColonyOnSight (false, null);
	}

	void Pursue(GameObject target) {
		//pursuing = true;
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
		Material mat = this.transform.GetChild(0).GetChild(0).GetComponent<Renderer> ().material;
		Color color = mat.color;
		mat.color = flashColour;
		health -= Time.deltaTime * hitRate;
		Debug.Log ("Agent Hitpoints: " + health);
		if (health <= 0) {
			Debug.Log ("Agent Died");
			if(HasFood()) {
				this.food.GetComponent<PickUpable>().SetBeingCarried(false);
			}
			Object.Destroy(this.gameObject);
		}
		mat.color = Color.Lerp (flashColour, myColor, flashSpeed * Time.deltaTime);
	}

	public void DecreaseLife (float lifeDecreased) {
		Material mat = this.transform.GetChild(0).GetChild(0).GetComponent<Renderer> ().material;
		Color color = mat.color;
		mat.color = flashColour;
		//life -= Time.deltaTime * lifeDecreased;
		health -= lifeDecreased;
		Debug.Log ("Agent Life: " + health);
		if (health <= 0) {
			Debug.Log ("Agent Died");
			if(HasFood()) {
				this.food.GetComponent<PickUpable>().SetBeingCarried(false);
			}
			Object.Destroy(this.gameObject);
		}
		mat.color = Color.Lerp (flashColour, myColor, flashSpeed * Time.deltaTime);
	}


}

