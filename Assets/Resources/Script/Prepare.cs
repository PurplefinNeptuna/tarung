using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Prepare : MonoBehaviour {

	public static Prepare main;
	public List<ActorData> roleAvailable;
	public Transform UICanvas;
	public GameObject UnitSelectPrefab;
	public GameObject ActiveUnitPrefab;
	public List<GameObject> ActiveUnits;
	public GameObject profilePanel;
	public Image profileImage;
	public Text profileName;
	public Text profileHPText;
	public Text profileAtk;
	public Text profileSpd;
	public Text profileMov;
	public int actorLimit;
	private bool isMulti;
	private int team;
	private bool team1Ready;
	private bool team2Ready;

	private void Awake() {
		if (main == null) {
			main = this;
		}
		else if (main != this) {
			Destroy(gameObject);
		}
		isMulti = (GameObject.Find("ClientGO") != null);
		team = !isMulti ? 1 : (GameObject.Find("ServerGO") ? 1 : 2);
		if (!isMulti) {
			team1Ready = false;
			team2Ready = true;
		}
		else {
			team1Ready = false;
			team2Ready = false;
		}
	}
	// Use this for initialization
	void Start() {
		roleAvailable = Resources.LoadAll<ActorData>("Actors").ToList();
		ScenarioData mapData = GameObject.FindGameObjectWithTag("Data").GetComponent<ScenarioData>();
		actorLimit = mapData.levelData.spawnpointTeam1.Count;
		float xPos = -150;
		foreach (var role in roleAvailable) {
			var roleSelect = Instantiate<GameObject>(UnitSelectPrefab, UICanvas);
			roleSelect.transform.localPosition = new Vector3(xPos, -100);
			roleSelect.GetComponent<SelectedRole>().Name = role.name;
			roleSelect.GetComponentsInChildren<Image>().Last().sprite = role.faceSprite;
			xPos += 100;
		}
		ActiveUnits = new List<GameObject>();
		xPos = -150;
		for (int i = 0; i < actorLimit; i++) {
			var activeRole = Instantiate<GameObject>(ActiveUnitPrefab, UICanvas);
			activeRole.transform.localPosition = new Vector3(xPos, 40);
			activeRole.GetComponent<ActiveRole>().Name = "Empty";
			activeRole.GetComponent<ActiveRole>().ID = i;
			activeRole.GetComponentsInChildren<Image>().Last().sprite = null;
			ActiveUnits.Add(activeRole);
			xPos += 100;
		}
		mapData.myTeam = team;
		mapData.isMulti = isMulti;
	}

	// Update is called once per frame
	void Update() {
		if (team1Ready && team2Ready)
			SceneManager.LoadScene(1);
	}
	public void RunGame() {
		ScenarioData mapData = GameObject.FindGameObjectWithTag("Data").GetComponent<ScenarioData>();
		string uData;
		switch (team) {
			case 1:
				uData = "CSU|1";
				mapData.ActorRole1 = new List<string>();
				foreach (var unit in ActiveUnits) {
					string uName = unit.GetComponent<ActiveRole>().Name;
					if (uName != "Empty") {
						mapData.ActorRole1.Add(uName);
						uData += "|" + uName;
					}
				}
				if (isMulti)
					Client.Instance.Send(uData);
				team1Ready = true;
				break;
			case 2:
				uData = "CSU|2";
				mapData.ActorRole2 = new List<string>();
				foreach (var unit in ActiveUnits) {
					string uName = unit.GetComponent<ActiveRole>().Name;
					if (uName != "Empty") {
						mapData.ActorRole2.Add(uName);
						uData += "|" + uName;
					}
				}
				if (isMulti)
					Client.Instance.Send(uData);
				team2Ready = true;
				break;
		}
	}

	public void SelectedUnit(string source) {
		Debug.Log(source + " Selected!");
		int IDTarget = ActiveUnits.Where(x => x.GetComponent<ActiveRole>().Name != "Empty").ToList().Count;
		Debug.Log("Next Slot: " + IDTarget);
		if (IDTarget >= ActiveUnits.Count)
			return;
		ActiveUnits[IDTarget].GetComponent<ActiveRole>().Name = source;
		ActiveUnits[IDTarget].GetComponentsInChildren<Image>().Last().sprite = roleAvailable.FirstOrDefault(x => x.name == source)?.faceSprite;
	}

	public void RemoveUnit(int source) {
		int totSelect = ActiveUnits.Where(x => x.GetComponent<ActiveRole>().Name != "Empty").ToList().Count;
		if (source >= totSelect)
			return;
		for (int i = source; i < totSelect - 1; i++) {
			Debug.Log(ActiveUnits[i].GetComponent<ActiveRole>().Name + " -> " + ActiveUnits[i + 1].GetComponent<ActiveRole>().Name);
			ActiveUnits[i].GetComponent<ActiveRole>().Name = ActiveUnits[i + 1].GetComponent<ActiveRole>().Name;
			ActiveUnits[i].GetComponentsInChildren<Image>().Last().sprite = ActiveUnits[i + 1].GetComponentsInChildren<Image>().Last().sprite;
		}
		ActiveUnits[totSelect - 1].GetComponent<ActiveRole>().Name = "Empty";
		ActiveUnits[totSelect - 1].GetComponentsInChildren<Image>().Last().sprite = null;
		Debug.Log(source + " Removed!");
	}

	public void ShowInfo(bool show, string source) {
		ActorData actorData = Resources.Load<ActorData>("Actors/" + source);
		profileImage.sprite = actorData.faceSprite;
		profileName.text = actorData.name;
		profileHPText.text = "HP   : " + actorData.health;
		profileAtk.text = "ATK : " + actorData.attack;
		profileSpd.text = "SPD : " + actorData.turnSpeed;
		profileMov.text = "MOV: " + actorData.moveRange;
		profilePanel.SetActive(true);
	}

	public void MultiAddUnit(int theteam, List<string> units) {
		if (theteam == team)
			return;

		ScenarioData mapData = GameObject.FindGameObjectWithTag("Data").GetComponent<ScenarioData>();
		switch (theteam) {
			case 1:
				mapData.ActorRole1 = units;
				team1Ready = true;
				break;
			case 2:
				mapData.ActorRole2 = units;
				team2Ready = true;
				break;
		}
	}
}
