using UnityEngine;
using System.Collections;

public class Move : MonoBehaviour
{	
	bool endOfWorld, hasFood, atBase;
	bool enemyInAhead,foodAhead, obstacleAhead, agentAhead;
	bool isFoodOnSight, isSpecFoodOnSight, isEnemyOnSight, isObstacleOnSight, isColonyOnSight, isFoodSourceOnSight;
	GameObject foodOnSight, specFoodOnSight, obstacleOnSight, enemyOnSight, colonyOnSight, foodSourceOnSight;
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
		enemyInAhead = false;
		hasFood = false;
		atBase = false;
		enemyInAhead = false;
		agentAhead = false;
		isFoodOnSight = false;
		isEnemyOnSight = false;
		isObstacleOnSight = false;
		isSpecFoodOnSight = false;
		myColor = transform.GetChild (0).GetChild (0).gameObject.GetComponent<Renderer> ().material.color;
	}

	/*
	 * Agent main loop
	 * Purely reactive Agent
	 * Function inside if's are sensors and the if's body has effectors
	 */
	void Update() {
		// Decreases Agent life over time
		// Value can be a public variable
		DecreaseLife(0.5f);

		if (EndOfWorld ()) {
			SendBack ();
		} else if (AgentAhead ()) {
			EvadeAgent ();
		} else if (FoodAhead () && !HasFood ()) {
			PickFood ();
		} else if (AtBase () && HasFood ()) {
			DropFood ();
		} else if (EnemyAhead ()) {
			HitEnemy ();
		} else if (ObstacleAhead ()) {
			HitWall ();
		} else if (SpecFoodOnSight () && !HasFood ()) {
			PursueFood (specFoodOnSight);
		} else if (FoodOnSight () && !HasFood ()) {
			PursueFood (foodOnSight);
		} else if(FoodSourceOnSight() && !HasFood ()) {
			Pursue(foodSourceOnSight);
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
	
	public bool AtBase () {
		return atBase;
	}

	bool AgentAhead () {
		return agentAhead;
	}

	bool ColonyOnSight ()	{
		return isColonyOnSight;
	}

	bool EndOfWorld () {
		return endOfWorld;
	}

	bool EnemyAhead() {
		return enemyInAhead;
	}

	
	bool EnemyOnSight () {
		return isEnemyOnSight;
	}


	bool FoodAhead (){
		return foodAhead;
	}
	
	bool FoodOnSight () {
		return isFoodOnSight;
	}

	bool FoodSourceOnSight() {
		return isFoodSourceOnSight;
	}

	public bool HasLowLife () {
		return health <= 5;
	}
	
	public bool HasFood ()	{
		return hasFood;
	}

	
	bool ObstacleOnSight ()	{
		return isObstacleOnSight;
	}

	bool SpecFoodOnSight () {
		return isSpecFoodOnSight;
	}

	bool ObstacleAhead() {
		return obstacleAhead;
	}

	/*
	 * Actuators
	 */

	void DropFood() {
		hasFood = false;
		string food_tag = food.tag;
		Destroy(food);
		food = null;
		Colony colonyComp = colony.GetComponent<Colony>();
		if(colonyComp != null) {
			colonyComp.IncreaseFood(food_tag);
		}
	}
	
	void EvadeMonster () {
		EvadeAgent();
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
	
	void GotoBase () {
		Pursue (this.colonyOnSight);
	}

	private void HitEnemy() {
		if (enemy != null) {
			Monster monster = enemy.GetComponent<Monster> ();
			monster.TakeDamage (gameObject);
		} else {
			enemyInAhead = false;

		}
	}

	private void HitWall() {
		if(wallObj != null) {
			Wall wall = wallObj.GetComponent<Wall>();
			wall.HitWall();
		} else {
			obstacleAhead = false;
		}
	}

	void MoveRandomly () {
		int rand = Random.Range(1,1000);
		if (rand <= 2) {
			transform.Rotate (0f,-90f,0f);
		} else if(rand <= 4) {
			transform.Rotate (0f,90f,0f);
		} else {
			MoveForward();
		}
	}

	void PickFood () {
		this.foodAhead = false;
		this.hasFood = true;
		PickUpable foodComp = food.GetComponent<PickUpable> ();
		if(foodComp != null) {
			foodComp.SetBeingCarried (true);
			foodComp.SetCarrying (gameObject);
		}
	}
	
	void PursueFood (GameObject food) {
		Pursue(food);
	}

	void PursueMonster () {
		Pursue (this.enemyOnSight);
	}
	
	void PursueObstacle () {
		Pursue(this.obstacleOnSight);
	}

	void TryToDestroyEnemy () {
		if (health > 10f) {
			PursueMonster ();
		} else {
			EvadeMonster ();
		}
	}
	

	
	/////////////////////////////////////////////////////////////////////////

	/*
	 * Auxiliar
	 */

	void CleanSight () {
		SetIsFoodOnSight (false, null);
		SetIsEnemyOnSight (false, null);
		SetIsObstacleOnSight (false, null);
		SetIsColonyOnSight (false, null);
		SetIsSpecFoodOnSight (false, null);
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

	void OnTriggerEnter(Collider collider) {
		if(collider.gameObject.tag == "PlayerA" || collider.gameObject.tag == "PlayerB") {
			//Debug.Log ("Collision");
			agentAhead = true;
		}
	}

	void Pursue(GameObject target) {
		//pursuing = true;
		Vector3 targetDir = target.transform.position - transform.position;
		//targetDir.y = this.transform.position.y;
		targetDir.y = 0.0f;
		float step  = smooth * Time.deltaTime;
		Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0f);
		transform.rotation = Quaternion.LookRotation(newDir);
		MoveForward();
	}

	void RunFromEnemy() {
		enemy = null;
		enemyInAhead = false;
		SendBack ();
	}

	////////////////////////////////////////////////////////////////////////


	/*
	 * Public (Being called outside Agent)
	 */

	
	public void DecreaseLife (float lifeDecreased) {
		health -= Time.deltaTime * lifeDecreased;
		if (health <= 0) {
			Debug.Log ("Agent Died");
			if(HasFood()) {
				PickUpable foodBeingCarried = this.food.GetComponent<PickUpable>();
				foodBeingCarried.SetBeingCarried(false);
				foodBeingCarried.SetCarrying(null);
			}
			Object.Destroy(this.gameObject);
		}
	}


	
	public void EatFood (float healHealth) {
		this.health += healHealth;
	}

	public void SendBack () {
		transform.Rotate (0f, 180f, 0f);
		endOfWorld = false;
		return;
	}

	// Setters
	public void SetIsFoodOnSight (bool isFoodOnSight, GameObject foodOnSight) {
		this.isFoodOnSight = isFoodOnSight;
		this.foodOnSight = foodOnSight;
	}

	public void SetIsSpecFoodOnSight (bool isSpecFoodOnSight, GameObject specFoodOnSight) {
		this.isSpecFoodOnSight = isSpecFoodOnSight;
		this.specFoodOnSight = specFoodOnSight;
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

	public void SetIsFoodSourceOnSight (bool isFoodSourceOnSight, GameObject foodSourceOnSight){
		this.isFoodSourceOnSight = isFoodSourceOnSight;
		this.foodSourceOnSight = foodSourceOnSight;
	}

	public void SetEndOfWorld() {
		endOfWorld = true;
	}
	
	public void SetEnemyInFront(GameObject enemy) {
		enemyInAhead = true;
		this.enemy = enemy;
	}
	
	// Box collider (mundo) chama esta funcao quando o agente sai
	public void SetWallAhead(GameObject wall) {
		obstacleAhead = true;
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
		//Color color = mat.color;
		mat.color = flashColour;
		DecreaseLife (hitRate);

		//Debug.Log ("Agent Hitpoints: " + health);
		mat.color = Color.Lerp (flashColour, myColor, flashSpeed * Time.deltaTime);
	}




}

