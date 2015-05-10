using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Planner {

	private IList<Vector3> beliefs;
	private Queue<Move.IntentionDetails> intentions;
	private IList<Action> actions;

	private Vector3 colonyPosition;

	public Planner(Vector3 colonyPosition) {
		this.colonyPosition = colonyPosition;
	}

	public void UpdatePlanner(IList<Vector3> beliefs, IList<Move.IntentionDetails> intentions, IList<Action> actions) {
		this.beliefs = beliefs;
		this.intentions = new Queue<Move.IntentionDetails>(intentions);
		this.actions = actions;
	}

	public Queue<PlanAction> Plan() {
		//Num planner mais avancado em vez de fazer dequeue teria de ir a uma lista e escolher qual a proxima intencao
		Move.IntentionDetails intentionDetails = intentions.Dequeue ();
		Queue<PlanAction> plan = new Queue<PlanAction> (); 

		Intention intention = intentionDetails.Intention();
		if (intention == Intention.SEARCH_FOOD) {
		} else if (intention == Intention.GET_FOOD_AT) {
			plan.Enqueue(new PlanAction(Action.MOVE_TO, intentionDetails.Position()));
			plan.Enqueue(new PlanAction(Action.PICK_FOOD));
			plan.Enqueue(new PlanAction(Action.MOVE_TO, colonyPosition));
		} else if (intention == Intention.DESTROY_WALL_AT) {
			plan.Enqueue(new PlanAction(Action.MOVE_TO, intentionDetails.Position()));
			plan.Enqueue(new PlanAction(Action.DESTROY_WALL));
		} else if (intention == Intention.GOTO_FOODSOURCE_AT) {
			plan.Enqueue(new PlanAction(Action.MOVE_TO, intentionDetails.Position()));
		} else if (intention == Intention.ATTACK_MONSTER_AT) {
			plan.Enqueue(new PlanAction(Action.MOVE_TO, intentionDetails.Position()));
			plan.Enqueue(new PlanAction(Action.FIGHT_MONSTER));
		} else if (intention == Intention.GOTO_COL_AT) {
			plan.Enqueue(new PlanAction(Action.MOVE_TO, colonyPosition));
		} else if (intention == Intention.POPULATE_AT) {
			plan.Enqueue(new PlanAction(Action.MOVE_TO, colonyPosition));
			plan.Enqueue(new PlanAction(Action.POPULATE));
		} else if (intention == Intention.HELP_OTHER_AT) {
			plan.Enqueue(new PlanAction(Action.MOVE_TO, intentionDetails.Position()));
		} else if (intention == Intention.EAT_FOOD) {
			plan.Enqueue(new PlanAction(Action.MOVE_TO, colonyPosition));
			plan.Enqueue(new PlanAction(Action.EAT));
		}

		return plan;
	}
}

public class PlanAction {
	Action action;
	Vector3 position;

	public PlanAction (Action action) {
		this.action = action;
	}

	public PlanAction (Action action, Vector3 position) {
		this.action = action;
		this.position = position;
	}
}
