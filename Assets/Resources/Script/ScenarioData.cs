using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioData : MonoBehaviour {

	public GameObject map;
	public LevelData levelData;
	public List<string> ActorRole1;
	public List<string> ActorRole2;
	public int myTeam;
	public bool isMulti;
	
	// Use this for initialization
	void Awake () {
		DontDestroyOnLoad(gameObject);
		var levels = Resources.LoadAll<LevelData>("Maps");
		levelData = levels[Random.Range(0, levels.Length)];
		map = levelData.map;
		ActorRole1 = new List<string>(){
			"Ninja","Knight","Mage","Ninja"
		};
		ActorRole2 = new List<string>(){
			"Ninja","Knight","Mage","Ninja"
		};
	}
	
	// Update is called once per frame
	public void Suicide() {
		Destroy(gameObject);
	}
}
