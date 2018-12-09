using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;


public class AIPlayer : Actor {
	public List<Tuple<Vector3Int, List<Utility.Direction>>> playerMove;
	public bool thinking = false;
	public float thinkMaxTime = 2f;
	public float thinkTime = 2f;
	public bool movableShowed = false;

	public AIPlayer(string actorType, Vector3Int startpos, int team) {
		TeamID = team;
		ActorDataOrigin = Resources.Load<ActorData>("Actors/" + actorType);
		Position = startpos;
		State = TurnState.INACTIVE;
	}

	public override void Run() {
		if (Health > 0) {
			thinking = false;
			GetMovablePosition(AttackRadius, true);
			if (playerMove != null && playerMove.Count > 0) {
				State = TurnState.ATK;
				thinking = true;
			}
			else {
				GetMovablePosition(MoveRadius);
				if (playerMove != null && playerMove.Count > 0) {
					State = TurnState.MOVE;
					thinking = true;
				}
				else {
					State = TurnState.WAIT;
				}
			}
			Main.main.ActorUpdate += Update;
		}
		else {
			instant = true;
			Position = new Vector3Int(-1000, -1000, 0);
		}
	}

	public override void Update() {
		if (!thinking) {
			if (State == TurnState.MOVE) {
				ResetMovablePosition();
				Vector3Int nearest = Utility.NearestEnemy(Position, Main.main.tiles, MoveRadius, TeamID);
				if (Position == nearest) {
					nearest = Utility.NearestEnemy(Position, Main.main.tiles, MoveRadius, TeamID, true);
				}

				ActorMoveList = playerMove.SingleOrDefault(x => x.Item1 == nearest).Item2;
				if (ActorMoveList == null) {
					ActorMoveList = new List<Utility.Direction>();
				}
				Debug.Log(ActorMoveList.Count);

				Position = nearest;
				State = TurnState.WAIT;
			}
			else if (State == TurnState.WAIT) {
				GetMovablePosition(AttackRadius, true);
				if (playerMove == null || playerMove.Count == 0) {
					EndTurn();
				}
				else {
					State = TurnState.ATK;
					thinking = true;
				}
			}
			else if (State == TurnState.ATK) {
				ResetMovablePosition();
				Vector3Int targetPos = playerMove[0].Item1;
				Actor target = Main.main.actors.FirstOrDefault(x => x.Position == targetPos && x.Health > 0);
				ActorAnim.FaceAttackTarget(targetPos);
				target.Health -= Attack;
				EndTurn();
			}
		}
		else {
			thinkTime -= Time.deltaTime;
			if (thinkTime <= 0f) {
				thinking = false;
				thinkTime = thinkMaxTime;
			}
		}
		if (!movableShowed && !inAnimation) {
			if (State == TurnState.ATK) {
				ShowMovablePosition(true);
				movableShowed = true;
			}
			else if (State == TurnState.MOVE) {
				ShowMovablePosition();
				movableShowed = true;
			}
		}
	}

	public override void Start() {
	}

	public void EndTurn() {
		State = TurnState.INACTIVE;
		Main.main.ActorUpdate -= Update;
	}

	void GetMovablePosition(int radius, bool searchActor = false) {
		playerMove = Utility.SelectableArea(Position, Main.main.tiles, radius, TeamID, searchActor);
		movableShowed = false;
	}

	void ShowMovablePosition(bool searchActor = false) {
		foreach (var moveList in playerMove) {
			Main.main.ground.SetColor(moveList.Item1, searchActor ? Main.main.attackColor : Main.main.moveColor);
		}
		if (playerMove != null && playerMove.Count > 0)
			Main.main.ground.SetColor(Position, Main.main.activeColor);
	}

	void ResetMovablePosition() {
		foreach (var moveList in playerMove) {
			Main.main.ground.SetColor(moveList.Item1, new Color(1f, 1f, 1f));
		}
		Main.main.ground.SetColor(Position, new Color(1f, 1f, 1f));
	}
}
