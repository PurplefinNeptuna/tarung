using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="newActor",menuName ="Create New Actor")]
public class ActorData : ScriptableObject {
	public Sprite deadSprite;
	public Sprite faceSprite;
	public List<Sprite> upSprite;
	public List<Sprite> downSprite;
	public List<Sprite> lrSprite;
	public int health;
	public int attack;
	public int moveRange;
	public int attackRange;
	public int turnSpeed;
	public int size;
}
