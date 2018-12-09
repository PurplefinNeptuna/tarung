using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public abstract class Actor {
	private Vector3Int _position;
	private int _health;
	private ActorData _actorData;
	public bool instant = true;
	public bool inAnimation = false;

	public float moveAnimationSpeed = .75f;

	public enum TurnState {
		INACTIVE,
		MOVE,
		WAIT,
		ATK
	}
	public TurnState State {
		get; set;
	}
	public GameObject ActorObject {
		get; set;
	}
	public ActorMove ActorAnim {
		get; set;
	}

	public List<Utility.Direction> ActorMoveList {
		get; set;
	}

	public ActorData ActorDataOrigin {
		get {
			return _actorData;
		}
		set {
			_actorData = value;
			if (_actorData != null) {
				ActorMoveList = new List<Utility.Direction>();
				ActorObject = new GameObject("Actor", typeof(SpriteRenderer), typeof(ActorMove));
				ActorAnim = ActorObject.GetComponent<ActorMove>();
				ActorAnim.actor = this;				
				SpriteRenderer temp = ActorObject.GetComponent<SpriteRenderer>();
				ActorAnim.sprite = temp;
				ActorAnim.renderedSprite = _actorData.lrSprite;
				temp.sprite = _actorData.lrSprite[ActorAnim.FeetPos];
				temp.sortingLayerName = "fg";
				if (TeamID == 2) {
					temp.flipX = true;
				}
				Health = _actorData.health;
				Attack = _actorData.attack;
				MoveRadius = _actorData.moveRange;
				AttackRadius = _actorData.attackRange;
				Speed = _actorData.turnSpeed;
				Size = _actorData.size;
			}
		}
	}
	public int Health {
		get {
			return _health;
		}
		set {
			_health = value;
			if (_health <= 0) {
				SpriteRenderer temp = ActorObject.GetComponent<SpriteRenderer>();
				temp.sprite = ActorDataOrigin.deadSprite;
				temp.sortingLayerName = "dead";
			}
		}
	}
	public int TeamID {
		get; set;
	}
	public int Idx {
		get; set;
	}
	public int Attack {
		get; set;
	}
	public int MoveRadius {
		get; set;
	}
	public int AttackRadius {
		get; set;
	}
	public int Size {
		get; set;
	}
	public int Speed {
		get; set;
	}
	public Vector3Int Position {
		get {
			return _position;
		}
		set {
			var oldPos = _position;
			_position = value;
			if (ActorObject != null) {
				if (instant) {
					ActorObject.transform.position = Main.main.grid.CellToWorld(Position) + new Vector3((float)Size / 2, (float)Size / 2);
					instant = false;
				}
				else
					ActorAnim?.AnimateMove(ActorMoveList, oldPos, new Vector3((float)Size / 2, (float)Size / 2), moveAnimationSpeed);
			}
		}
	}
	public abstract void Start();
	public abstract void Run();
	public abstract void Update();

	public void HideCorpse() {
		instant = true;
		Position = new Vector3Int(-1000, -1000, 0);
	}
}
