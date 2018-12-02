using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class WaitList : MonoBehaviour {

	public static WaitList Instance;
	
	public GameObject playerListPrefab;
	public Transform UICanvas;
	private List<GameObject> playerList;
	private List<GameClient> players;
	public bool error;

	private void Awake() {
		if (Instance == null)
			Instance = this;
		else if (Instance != this) {
			Destroy(gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		playerList = new List<GameObject>();
		players = Client.Instance.players;
		foreach (var p in players) {
			GameObject pList = Instantiate(playerListPrefab, UICanvas, false);
			pList.GetComponent<Text>().text = p.name + (p.isHost ? " (★)" : "");
			pList.transform.localPosition -= new Vector3(0, 60 * playerList.Count, 0);
			playerList.Add(pList);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (error)
			TurnBack();	
		players = Client.Instance.players;
		if (players.Count > playerList.Count) {
			for (int i = playerList.Count; i < players.Count; i++) {
				GameObject pList = Instantiate(playerListPrefab, UICanvas, false);
				pList.GetComponent<Text>().text = players[i].name + (players[i].isHost ? " (★)" : "");
				pList.transform.localPosition -= new Vector3(0, 60 * playerList.Count, 0);
				playerList.Add(pList);
			}
		}
		if (playerList.Count >= 2) {
			StartCoroutine(Ready());
		}
	}

	public void TurnBack() {
		Destroy(GameObject.Find("ClientGO"));
		Destroy(GameObject.Find("ServerGO"));
		SceneManager.LoadScene(0);
	}

	public IEnumerator Ready() {
		yield return new WaitForSeconds(3);
		SceneManager.LoadScene(2);
	}
}
