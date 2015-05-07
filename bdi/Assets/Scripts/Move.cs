﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum Desire {GET_FOOD, DEFEND_COL, HELP_OTHER, HELP_SELF, POPULATE}
public enum Intention {SEARCH_FOOD, GET_FOOD_AT, GOTO_FOODSOURCE_AT, ATTACK_MONSTER_AT, GOTO_COL_AT, POPULATE_AT, HELP_OTHER_AT, EAT_FOOD}

public class Move : MonoBehaviour
{	
	bool endOfWorld, hasFood, atBase;
	bool enemyInAhead,foodAhead, obstacleAhead, agentAhead;
	bool isFoodOnSight, isSpecFoodOnSight, isEnemyOnSight, isObstacleOnSight, isColonyOnSight;
	bool isFoodSourceOnSight;
	GameObject foodOnSight, specFoodOnSight, obstacleOnSight, enemyOnSight, colonyOnSight, foodSourceOnSight;
	GameObject food, enemy, wallObj, colony;
	GameObject myColony;
	Color flashColour = Color.red;
	Color myColor;
	float flashSpeed = 5f;
	Vector3 myColonyPosition;
	public float distance, smooth;
	private float health = 20f;
	private float hitRate = 2f;
	private const float SPEED = 10f;
	//IList<Desire> myDesires; 
	Dictionary<Desire, float> myDesires;
	Dictionary<Intention, Vector3> myIntentions;
	Dictionary<Vector3, string> myBeliefs;
	Plan currentPlan;


	void Awake () {
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
		currentPlan = null;
		myColor = transform.GetChild (0).GetChild (0).gameObject.GetComponent<Renderer> ().material.color;

		myDesires = new Dictionary<Desire,float> ();
		//myDesires = new List<Desire> ();
		InitializeDesires ();

		//myIntentions = new Dictionary<Intention, Vector3> ();
		//FIXME no initial intentions???
		//myIntentions [Intention.SEARCH_FOOD] = null;
		myBeliefs = new Dictionary<Vector3,string> ();

	}

	/*
	 * Agent main loop
	 * BDI Agent
	 * The commitment is single-minded
	 */
	void Update() {
		// Decreases Agent life over time
		// Value can be a public variable
		DecreaseLife(0.5f);


		//p <- nextPercetp = what is seeing
		//pseudo only, obvious that they will be updated with passage by ref
		//single commitment
		if (currentPlan == null) {
			currentPlan = null;
			Brf ();
			Options ();
			Filter ();
			currentPlan = PlanNewPlan ();
			currentPlan.Execute ();
		} else {
			if(currentPlan.IsEmpty () || Succeeded () || Impossible ()) {
				currentPlan = null;
				return;
			}
			currentPlan.Execute ();
			Brf ();
			if(!Sound ()) {
				currentPlan = PlanNewPlan ();
			}
		}

	}

	/*
	 * BDI
	 */

	void Brf () {
		// The beliefs are constantly updated with the IsOnSight.
		// However it's necessary to update that in the position of the agent
		// there is nothing but him. Necessary in cases such as picking up food.
		Vector3 myPosition = this.transform.position;
		if (myBeliefs.ContainsKey (myPosition)) {
			myBeliefs.Remove(myPosition);
		}
		myPosition.y += 1.5f;
		if (myBeliefs.ContainsKey (myPosition)) {
			myBeliefs.Remove(myPosition);
		}
	}

	void Options () {
		//using myBeliefs and myIntentions update myDesires
		if (KnowWhereFoodIs () || KnowWhereFoodSourceIs ()) {
			float get_food_multiplier = 1f;
			//lower weight to get food if already carrying
			if (!HasFood ()) {
				get_food_multiplier = 0.3f;
			}
			myDesires [Desire.GET_FOOD] = 1f * get_food_multiplier;
		} else {
			myDesires [Desire.GET_FOOD] = 0.5f;
		}

		if (HasLowLife ()) {
			float help_self_multiplier = 0.7f;
			if (HasFood ()) {
				help_self_multiplier = 1f;
			}
			myDesires [Desire.HELP_SELF] = 1f * help_self_multiplier;
		} else {
			myDesires [Desire.HELP_SELF] = 0.3f;
		}

		if (ColonyBeingAttacked ()) {
			if (AtBase ()) {
				myDesires [Desire.DEFEND_COL] = 1f;
			} else {
				float defend_col_multiplier = 1f;
				float number_of_ind_at_base = HowManyAtBase ();
				//The more there are at base the less I want to go there
				if (number_of_ind_at_base > 0) {
					defend_col_multiplier = defend_col_multiplier / number_of_ind_at_base;
				}
				myDesires [Desire.DEFEND_COL] = 1f * defend_col_multiplier;
			}
		} else {
			myDesires [Desire.DEFEND_COL] = 0;
		}

		if (FoodToPopulate ()) {
			float populate_multiplier = 1f;
			float number_of_ind_at_base = HowManyAtBase ();
			if (number_of_ind_at_base < 2) {
				populate_multiplier = 0.7f;
			}
			myDesires [Desire.POPULATE] = 1f * populate_multiplier;
		} else {
			myDesires [Desire.POPULATE] = 0.1f;
		}

		//FIXME 
		//TODO HELP_OTHER

	}

	void Filter () {
		//using my@(Beliefs,Intentions,Desires) update myIntentions
	}


	Plan PlanNewPlan () {
		//uses myBeliefs and myIntentions
		return new Plan (null, this.gameObject);
	}

	/*
	 *BDI aux
	 */

	bool ColonyBeingAttacked () {
		Colony myColComponent = myColony.GetComponent<Colony> ();
		return myColComponent.IsUnderAttack ();
	}

	bool FoodToPopulate () {
		Colony myColComponent = myColony.GetComponent<Colony> ();
		return myColComponent.HasFoodToPopulate ();
	}

	float HowManyAtBase () {
		Colony myColComponent = myColony.GetComponent<Colony> ();
		return myColComponent.HowManyAtBase ();
	}

	bool Impossible () {
		//is it possible that with my intention i complete my beliefs?
		return false;
	}

	bool KnowWhereFoodIs () {
		return myBeliefs.ContainsValue ("Food");
	}

	bool KnowWhereFoodSourceIs () {
		return myBeliefs.ContainsValue ("FoodSource");
	}

	bool Sound () {
		//has the plan gone wrong?
		return true;
	}

	bool Succeeded () {
		//have i done what i wanted to?
		return false;
	}

	/*
	class IntentionPosition {
		Intention intention;
		Vector3 position;

		public IntentionPosition(Intention intention, Vector3 position) {
			this.intention = intention;
			this.position = position;
		}

		public Intention Intention () {
			return this.intention;
		}

		public Vector3 Position () {
			return this.position;
		}

	}*/

	//FIXME Template for now, must be reviewd!!!
	class Plan {
		IList<string> steps;
		GameObject ind;
		
		public Plan (IList<string> steps, GameObject ind) {
			this.steps = steps;
			this.ind = ind;
		}
		
		public bool IsEmpty () {
			if (steps == null) {
				return true;
			}
			return steps.Count == 0;
		}
		
		public string Head () {
			if (steps == null) {
				return "true";
			}
			return steps[0];
		}
		
		public void Execute () {
			var step = Head ();
			//FIXME make ind execute step
			if (steps == null) {
				return;
			}
			steps.RemoveAt (0);
		}
		
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

	public bool HasLowLife () {
		return health <= 5;
	}
	
	bool HasFood ()	{
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
		Vector3 foodPosition = food.transform.position;
		// We do not need to know where the food we have is
		if(myBeliefs.ContainsKey(foodPosition)) {
			myBeliefs.Remove(foodPosition);
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

	void InitializeDesires () {
		/*foreach (Desire desire in System.Enum.GetValues(typeof(Desire))) {
			myDesires.Add(desire);
		}*/

		foreach (Desire desire in System.Enum.GetValues(typeof(Desire))) {
			myDesires[desire] = 1;
		}
		/*
		myDesires.Add (Desire.GET_FOOD);
		myDesires.Add (Desire.DEFEND_COL);
		myDesires.Add (Desire.HELP_OTHER);
		myDesires.Add (Desire.HELP_SELF);
		myDesires.Add (Desire.POPULATE);*/
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
		targetDir.y = 0;
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
				Vector3 myPosition = this.transform.position;
				myPosition.y = 1.5f;
				foodBeingCarried.transform.position = myPosition;
			}
			Object.Destroy(this.gameObject);
		}
	}


	
	public void EatFood (float healHealth) {
		this.health += healHealth;
	}

	// FIXME: Do we need the public here?
	public void SendBack () {
		transform.Rotate (0f, 180f, 0f);
		endOfWorld = false;
		return;
	}

	// Setters

	public void SetColonyPosition (Vector3 position) {
		myColonyPosition = position;
		myBeliefs [position] = "MyCol";
	}

	public void SetMyColony (GameObject myColony) {
		this.myColony = myColony;
	}

	public void SetIsFoodSourceOnSight (bool isFoodSourceOnSight, GameObject foodSourceOnSight){
		this.isFoodSourceOnSight = isFoodSourceOnSight;
		this.foodSourceOnSight = foodSourceOnSight;
		if(isFoodSourceOnSight) {
			Vector3 foodSourcePosition = new Vector3(foodSourceOnSight.transform.position.x, foodSourceOnSight.transform.position.y, foodSourceOnSight.transform.position.z);
			if(!myBeliefs.ContainsKey(foodSourcePosition)) {
				myBeliefs [foodSourcePosition] = "FoodSource";
			}
		}
	}

	public void SetIsFoodOnSight (bool isFoodOnSight, GameObject foodOnSight) {
		this.isFoodOnSight = isFoodOnSight;
		this.foodOnSight = foodOnSight;
		if(isFoodOnSight) {
			Vector3 foodPosition = new Vector3(foodOnSight.transform.position.x, foodOnSight.transform.position.y, foodOnSight.transform.position.z);
			if(!myBeliefs.ContainsKey(foodPosition)) {
				myBeliefs[foodPosition] = "Food";
			}
		}
	}

	public void SetIsSpecFoodOnSight (bool isSpecFoodOnSight, GameObject specFoodOnSight) {
		this.isSpecFoodOnSight = isSpecFoodOnSight;
		this.specFoodOnSight = specFoodOnSight;
		if(isSpecFoodOnSight) {
			Vector3 foodPosition = new Vector3(specFoodOnSight.transform.position.x, specFoodOnSight.transform.position.y, specFoodOnSight.transform.position.z);
			if(!myBeliefs.ContainsKey(foodPosition)) {
				myBeliefs[foodPosition] = "SpecFood";
			}
		}
	}

	public void SetIsEnemyOnSight (bool isEnemyOnSight, GameObject enemyOnSight) {
		this.isEnemyOnSight = isEnemyOnSight;
		this.enemyOnSight = enemyOnSight;
		// TODO: Do we need this?
		Vector3 enemyPosition = enemyOnSight.transform.position;
		/* FIXME verify that if has the key, it was not a monster
		if(!myBeliefs.ContainsKey(enemyPosition)) {
			myBeliefs[enemyPosition] = "Monster";
		}*/
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

		Debug.Log ("Agent Hitpoints: " + health);
		mat.color = Color.Lerp (flashColour, myColor, flashSpeed * Time.deltaTime);
	}




}

