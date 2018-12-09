using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class MPlayer : Actor {
	public List<Tuple<Vector3Int, List<Utility.Direction>>> playerMove;
	public MPlayer(string actorType, Vector3Int startpos, int team) {
		TeamID = team;
		ActorDataOrigin = Resources.Load<ActorData>("Actors/" + actorType);
		Position = startpos;
		State = TurnState.INACTIVE;
	}

	public override void Run() {
		if (Health > 0) {
			State = TurnState.MOVE;
			Main.main.waiting = this;
		}
		else {
			Client.Instance.Send("CRC|" + Idx);
			instant = true;
			Position = new Vector3Int(-1000, -1000, 0);
		}
	}
	public override void Start() {
	}
	public override void Update() {
	}

	public void TryMove(Vector3Int tar) {
		GetMovablePosition(MoveRadius, false);

		ActorMoveList = playerMove.SingleOrDefault(x => x.Item1 == tar).Item2;
		if (ActorMoveList == null) {
			ActorMoveList = new List<Utility.Direction>();
		}

		if (ActorMoveList.Count > 0) {
			Position = tar;
			State = TurnState.ATK;
		}
		else {
			Debug.Log("Client Error Move");
		}
	}
	public void TryAttack(Actor target) {
		GetMovablePosition(AttackRadius, true);
		if (playerMove.SingleOrDefault(x => x.Item1 == target.Position) != null) {
			target.Health -= Attack;
			ActorAnim.FaceAttackTarget(target.Position);
			Main.main.waiting = null;
			State = TurnState.INACTIVE;
		}
		else {
			Debug.Log("Client Error Attack");
		}
	}
	void GetMovablePosition(int radius, bool searchActor = false) {
		playerMove = Utility.SelectableArea(Position, Main.main.tiles, radius, TeamID, searchActor);
	}
}
