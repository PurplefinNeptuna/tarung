using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newLevel", menuName = "Create New Level")]
public class LevelData : ScriptableObject {
	public GameObject map;
	public List<Vector3Int> spawnpointTeam1;
	public List<Vector3Int> spawnpointTeam2;
}
