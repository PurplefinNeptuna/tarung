using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class Player : Actor {
	public List<Tuple<Vector3Int, List<Utility.Direction>>> playerMove;

	public Player(string actorType, Vector3Int startpos, int team) {
		TeamID = team;
		ActorDataOrigin = Resources.Load<ActorData>("Actors/"+actorType);
		Position = startpos;
		State = TurnState.INACTIVE;
	}

	public override void Start() {
	}

	public override void Run() {
		if (Health > 0) {
			GetMovablePosition(MoveRadius);
			if (playerMove != null && playerMove.Count > 0)
				State = TurnState.MOVE;
			else {
				string mData = "CSM|-1";
				if (Main.main.isMulti)
					Client.Instance.Send(mData);
				State = TurnState.WAIT;
			}
			Main.main.ActorUpdate += Update;
		}
		else {
			if (Main.main.isMulti)
				Client.Instance.Send("CRC|" + Idx);
			Position = new Vector3Int(-1000, -1000, 0);
		}
	}

	public override void Update() {
		if (Input.GetMouseButtonDown(0)) {
			Vector3 worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int cellpos = Main.main.grid.WorldToCell(worldpos);

			if (State == TurnState.MOVE) {
				if (Main.main.ground.GetColor(cellpos) == Main.main.moveColor) {
					ResetMovablePosition();
					string mData = "CSM|";
					mData += Idx + "|";
					mData += cellpos.x + "#" + cellpos.y;
					if (Main.main.isMulti)
						Client.Instance.Send(mData);
					Position = cellpos;
					State = TurnState.WAIT;
				}
				else if (Main.main.ground.GetColor(cellpos) == Main.main.activeColor) {
					ResetMovablePosition();
					string mData = "CSM|-1";
					if (Main.main.isMulti)
						Client.Instance.Send(mData);
					State = TurnState.WAIT;
				}
			}
			else if (State == TurnState.ATK) {
				if (Main.main.ground.GetColor(cellpos) == Main.main.attackColor) {
					Actor target = Main.main.actors.FirstOrDefault(x => x.Position == cellpos && x.Health > 0);
					target.Health -= Attack;
					ResetMovablePosition();
					string mData = "CSA|";
					mData += Idx + "|";
					mData += target.Idx;
					if (Main.main.isMulti)
						Client.Instance.Send(mData);
					EndTurn();
				}
				else if (Main.main.ground.GetColor(cellpos) == Main.main.activeColor) {
					ResetMovablePosition();
					string mData = "CSA|-1";
					if (Main.main.isMulti)
						Client.Instance.Send(mData);
					EndTurn();
				}
			}
		}
		else if (State == TurnState.WAIT) {
			GetMovablePosition(AttackRadius, true);
			if (playerMove == null || playerMove.Count == 0) {
				string mData = "CSA|-1";
				if (Main.main.isMulti)
					Client.Instance.Send(mData);
				EndTurn();
			}
			else
				State = TurnState.ATK;
		}
	}

	public void EndTurn() {
		State = TurnState.INACTIVE;
		Main.main.ActorUpdate -= Update;
	}

	void GetMovablePosition(int radius, bool searchActor = false) {
		playerMove = Utility.SelectableArea(Position, Main.main.tiles, radius, TeamID, searchActor);
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
