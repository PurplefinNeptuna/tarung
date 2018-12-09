using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorMove : MonoBehaviour {

	public Actor actor;
	public SpriteRenderer sprite;
	private bool inTransition = false;
	private bool inMove = false;
	private Queue<Utility.Direction> transitionQueue;
	private float transitionSpeed;
	private float timeToNextTransition;
	private Vector3Int nowPos;
	private Vector3Int targetPos;
	private Vector3 offset;
	private Grid grid;
	public List<Sprite> renderedSprite;
	private int feetperiod =0;
	private float maxStepTime = .125f;
	private float stepTime;
	public int FeetPos {
		get {
			if (feetperiod == 0||feetperiod == 2)
				return 1;
			else if (feetperiod == 1)
				return 0;
			else
				return 2;
		}
	}

	// Use this for initialization
	void Start() {
		grid = Main.main.grid;
		transitionQueue = new Queue<Utility.Direction>();
	}

	// Update is called once per frame
	void Update() {
		if (inMove) {
			stepTime -= Time.deltaTime;
			if (stepTime < 0f) {
				feetperiod = (feetperiod + 1) % 4;
				sprite.sprite = renderedSprite[FeetPos];
				stepTime = maxStepTime + stepTime;
			}
			if (inTransition) {
				Vector3 nowV3Pos = grid.CellToWorld(nowPos) + offset;
				Vector3 targetV3Pos = grid.CellToWorld(targetPos) + offset;
				timeToNextTransition -= Time.deltaTime;
				transform.position = Vector3.Lerp(nowV3Pos, targetV3Pos, (transitionSpeed - timeToNextTransition) / transitionSpeed);
				if (timeToNextTransition < 0f) {
					inTransition = false;
					nowPos = targetPos;
				}
			}
			else if (transitionQueue.Count == 0) {
				inMove = false;
				actor.inAnimation = false;
				feetperiod = 0;
				sprite.sprite = renderedSprite[FeetPos];
			}
			else {
				var nowDir = transitionQueue.Dequeue();
				targetPos = nowPos;
				if (nowDir == Utility.Direction.N) {
					targetPos += Vector3Int.up;
					renderedSprite = actor.ActorDataOrigin.upSprite;
					sprite.flipX = false;
				}
				else if (nowDir == Utility.Direction.E) {
					targetPos += Vector3Int.right;
					renderedSprite = actor.ActorDataOrigin.lrSprite;
					sprite.flipX = false;
				}
				else if (nowDir == Utility.Direction.S) {
					targetPos += Vector3Int.down;
					renderedSprite = actor.ActorDataOrigin.downSprite;
					sprite.flipX = false;
				}
				else if (nowDir == Utility.Direction.W) {
					targetPos += Vector3Int.left;
					renderedSprite = actor.ActorDataOrigin.lrSprite;
					sprite.flipX = true;
				}
				sprite.sprite = renderedSprite[FeetPos];
				timeToNextTransition = transitionSpeed;
				inTransition = true;
			}
		}
	}

	public void AnimateMove(List<Utility.Direction> dir, Vector3Int start, Vector3 offset, float speed) {
		if (dir.Count <= 0)
			return;
		inMove = true;
		inTransition = false;
		feetperiod = 1;
		stepTime = maxStepTime;
		transitionQueue = new Queue<Utility.Direction>(dir);
		nowPos = start;
		this.offset = offset;
		transitionSpeed = speed / dir.Count;
		timeToNextTransition = transitionSpeed;
		actor.inAnimation = true;
	}

	public void FaceAttackTarget(Vector3Int target) {
		//kartesian
		Vector3Int dist = target - actor.Position;
		if (Math.Abs(dist.x) > Math.Abs(dist.y)) {
			if (dist.x > 0) {
				renderedSprite = actor.ActorDataOrigin.lrSprite;
				sprite.flipX = false;
			}
			else {
				renderedSprite = actor.ActorDataOrigin.lrSprite;
				sprite.flipX = true;
			}
		}
		else {
			if (dist.y > 0) {
				renderedSprite = actor.ActorDataOrigin.upSprite;
				sprite.flipX = false;
			}
			else {
				renderedSprite = actor.ActorDataOrigin.downSprite;
				sprite.flipX = false;
			}
		}
		sprite.sprite = renderedSprite[FeetPos];
	}
}
