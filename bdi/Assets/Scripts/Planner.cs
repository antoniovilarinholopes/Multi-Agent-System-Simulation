using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Planner {
	private Move.IntentionDetails intentionDetails;
	private Vector3 colonyPosition;

	public Planner(Vector3 colonyPosition, Move.IntentionDetails intentionDetails) {
		this.colonyPosition = colonyPosition;
		this.intentionDetails = intentionDetails;
	}

	/*
	public Planner(Vector3 colonyPosition, Move.IntentionDetails intentionDetails) {
		this.colonyPosition = colonyPosition;
		this.intentionDetails = intentionDetails;
	}

	public void UpdatePlanner(Move.IntentionDetails intentionDetails) {
		this.intentionDetails = intentionDetails;
	}*/


	public Queue<PlanAction> Plan() { 
		Queue<PlanAction> plan = new Queue<PlanAction> ();
		Intention intention = intentionDetails.Intention();
		if (intention == Intention.SEARCH_FOOD) {
			plan.Enqueue(new PlanAction(Action.MOVE_TO, intentionDetails.Position()));
		} else if (intention == Intention.GET_FOOD_AT) {
			plan.Enqueue(new PlanAction(Action.MOVE_TO, intentionDetails.Position()));
			plan.Enqueue(new PlanAction(Action.PICK_FOOD));
			plan.Enqueue(new PlanAction(Action.MOVE_TO, colonyPosition));
			plan.Enqueue(new PlanAction(Action.DROP_FOOD, colonyPosition));
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
			//FIXME is moving to col really needed? Maybe
			//plan.Enqueue(new PlanAction(Action.MOVE_TO, colonyPosition));
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

	public Action Action () {
		return this.action;
	}

	public Vector3 Position () {
		return this.position;
	}

}
