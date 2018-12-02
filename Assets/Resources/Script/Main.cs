using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour {

	public static Main main;
	public Grid grid;
	public Tilemap ground;
	public List<Actor> actors;
	private Vector3Int cellpos;
	private Vector3 worldpos;
	private Vector3 firstpos;
	private bool clicked = false;
	public Dictionary<Vector3Int, WorldTiles> tiles;
	public delegate void Updater();
	public event Updater ActorUpdate;
	public int team;
	public bool isMulti;
	public MPlayer waiting;
	public string[] teamName;
	private ScenarioData mapData;

	private Tuple<Actor, int> thisTurn;
	public TurnQueue turnQueue;
	private bool playing = true;

	[Header("Turn Tile Color")]
	public Color moveColor = new Color(.5f, 1f, .5f);
	public Color activeColor = new Color(.5f, .5f, 1f);
	public Color attackColor = new Color(1f, .5f, .5f);

	[Header("UI Objects")]
	public GameObject winPanel;
	public GameObject profilePanel;
	public Image profileImage;
	public Text profileName;
	public Slider profileHPBar;
	public Image profileHPFill;
	public Text profileHPText;
	public Text profileAtk;
	public Text profileSpd;
	public Text profileMov;

	private void Awake() {
		if (main == null) {
			main = this;
		}
		else if (main != this) {
			Destroy(gameObject);
		}

		mapData = GameObject.FindGameObjectWithTag("Data").GetComponent<ScenarioData>();
		mapData.map = Instantiate<GameObject>(mapData.map);
		isMulti = mapData.isMulti;
		team = mapData.myTeam;
		grid = mapData.map.GetComponent<Grid>();

		actors = new List<Actor>();

		GetWorldTiles();

		for (int i = 0; i < Math.Min(mapData.levelData.spawnpointTeam1.Count, mapData.ActorRole1.Count); i++) {
			if (!isMulti || team == 1) {
				actors.Add(new Player(mapData.ActorRole1[i], mapData.levelData.spawnpointTeam1[i], 1));
			}
			else {
				actors.Add(new MPlayer(mapData.ActorRole1[i], mapData.levelData.spawnpointTeam1[i], 1));
			}
		}
		for (int i = 0; i < Math.Min(mapData.levelData.spawnpointTeam2.Count, mapData.ActorRole2.Count); i++) {
			if (!isMulti) {
				actors.Add(new AIPlayer(mapData.ActorRole2[i], mapData.levelData.spawnpointTeam2[i], 2));
			}
			else if (isMulti && team == 2) {
				actors.Add(new Player(mapData.ActorRole2[i], mapData.levelData.spawnpointTeam2[i], 2));
			}
			else {
				actors.Add(new MPlayer(mapData.ActorRole2[i], mapData.levelData.spawnpointTeam2[i], 2));
			}
		}

		for (int i = 0; i < actors.Count; i++) {
			actors[i].Idx = i;
		}

		if (team == 1)
			turnQueue = new TurnQueue(actors);

		if (isMulti) {
			teamName = new string[]{
				Client.Instance.players.Where(x => x.isHost).ToList()[0].name,
				Client.Instance.players.Where(x => !x.isHost).ToList()[0].name
			};
		}

		waiting = null;
	}

	// Use this for initialization
	void Start() {
		ground = grid.GetComponentsInChildren<Tilemap>()[0];
	}

	// Update is called once per frame
	void Update() {
		ClickDebug();
		if (playing) {
			if (team == 1 && turnQueue.Count > 0) {
				if (!turnQueue.active) {
					thisTurn = turnQueue.Pop();
					thisTurn.Item1.Run();
					Debug.Log("this turn: Team " + (thisTurn.Item1.TeamID) + " " + thisTurn.Item1.Position);
					if (isMulti && thisTurn.Item1.GetType() == typeof(MPlayer)) {
						Client.Instance.Send("CHR|" + thisTurn.Item2);
					}
					turnQueue.active = true;
				}
				else if (thisTurn.Item1.State == Actor.TurnState.INACTIVE) {
					if (thisTurn.Item1.Health > 0)
						turnQueue.Add(thisTurn.Item1, thisTurn.Item2);
					turnQueue.active = false;
				}
			}
			worldpos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			cellpos = grid.WorldToCell(worldpos);
			Actor hover = actors.SingleOrDefault(x => x.Position == cellpos && x.Health > 0);
			if (hover != null) {
				ShowProfile(true, hover);
			}
			else {
				ShowProfile(false);
			}
		}

		int Group1 = 0;
		int Group2 = 0;
		foreach (var iActor in actors) {
			if (iActor.Health > 0) {
				if (iActor.TeamID == 1) {
					Group1++;
				}
				else {
					Group2++;
				}
			}
		}
		if (Group1 == 0 || Group2 == 0) {
			playing = false;
			winPanel.SetActive(true);
			if (isMulti)
				winPanel.GetComponentInChildren<Text>().text = (Group2 == 0 ? teamName[0] : teamName[1]) + " Win";
			else
				winPanel.GetComponentInChildren<Text>().text = (Group2 == 0 ? "Blue" : "Red") + " Win";
		}

		ActorUpdate?.Invoke();
	}

	private void GetWorldTiles() {
		tiles = new Dictionary<Vector3Int, WorldTiles>();
		foreach (Tilemap map in grid.GetComponentsInChildren<Tilemap>()) {
			foreach (Vector3Int pos in map.cellBounds.allPositionsWithin) {
				Vector3Int localplace = new Vector3Int(pos.x, pos.y, pos.z);

				if (!map.HasTile(localplace))
					continue;
				if (tiles.ContainsKey(localplace))
					tiles.Remove(localplace);
				WorldTiles tile = new WorldTiles() {
					LocalPlace = localplace,
					WorldLocation = map.CellToWorld(localplace),
					TileBase = map.GetTile(localplace),
					TilemapMember = map,
					//nambah nanti
					Name = localplace.x + "," + localplace.y,
					Cost = 1
				};
				tiles.Add(tile.LocalPlace, tile);
			}
		}
	}

	private void ClickDebug() {
		if (Input.GetMouseButtonDown(0) && !clicked) {
			Vector3 temp1 = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int temp2 = grid.WorldToCell(temp1);
			Debug.Log(temp2);

			clicked = true;
		}
		else
			clicked = false;
	}

	private void ShowProfile(bool show, Actor actor = null) {
		if (!show) {
			profilePanel.SetActive(false);
		}
		else {
			profileImage.sprite = actor.ActorDataOrigin.faceSprite;
			profileName.text = actor.ActorDataOrigin.name;
			profileHPBar.value = (float)actor.Health / (float)actor.ActorDataOrigin.health;
			profileHPText.text = "HP: " + actor.Health;
			profileAtk.text = "ATK: " + actor.Attack;
			profileSpd.text = "SPD: " + actor.Speed;
			profileMov.text = "MOV: " + actor.MoveRadius;
			Image temp = profilePanel.GetComponent<Image>();
			temp.color = actor.TeamID == 1 ? new Color(0.25f, 0.5f, 1, 0.8f) : new Color(1, 0.5f, 0.25f, 0.8f);
			if (worldpos.y < 0) {
				RectTransform panel = profilePanel.GetComponent<RectTransform>();
				panel.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 550, panel.rect.height);
				panel.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, panel.rect.height);
			}
			else {
				RectTransform panel = profilePanel.GetComponent<RectTransform>();
				panel.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0, panel.rect.height);
				panel.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 550, panel.rect.height);
			}
			profilePanel.SetActive(true);
		}
	}

	public void LoadMenu() {
		Client.Instance?.Suicide();
		Server.Instance?.Suicide();
		mapData.Suicide();
		Client.Instance = null;
		Server.Instance = null;
		SceneManager.LoadScene(0);
	}

	public void MultiMove(int idx, Vector3Int tar) {
		if (team == actors[idx].TeamID)
			return;
		if (team == 1 && waiting == null)
			return;
		if (team == 1 && waiting.Idx != idx)
			return;
		if (team == 1 && waiting.Health > 0)
			waiting.TryMove(tar);
		else {
			if (actors[idx].GetType() == typeof(MPlayer) && actors[idx].Health > 0)
				((MPlayer)actors[idx]).TryMove(tar);
		}
	}

	public void MultiAttack(int src, int tar) {
		if (team == actors[src].TeamID)
			return;
		if (team == 1 && waiting == null)
			return;
		if (team == 1 && waiting.Idx != src)
			return;
		Actor target = actors[tar];
		if (target.Health <= 0)
			return;
		if (team == 1 && waiting.Health > 0)
			waiting.TryAttack(target);
		else {
			if (actors[src].GetType() == typeof(MPlayer) && actors[src].Health > 0)
				((MPlayer)actors[src]).TryAttack(target);
		}
	}
	public void TryRun(int idx) {
		if (team == 1)
			return;

		Debug.Log("Trying to move: " + idx + " " + actors[idx].GetType() + " " + typeof(Player));
		Debug.Log("Target Pos: " + actors[idx].Position);
		if (actors[idx].GetType() == typeof(Player))
			actors[idx].Run();
	}
	public void TryRemoveCorpse(int idx) {
		if (team == 1)
			return;
		if (actors[idx].Health <= 0) {
			actors[idx].HideCorpse();
		}
	}

	public void TrySkip(bool s) {
		if (thisTurn.Item1.TeamID == team)
			return;
		if (s) {
			thisTurn.Item1.State = Actor.TurnState.ATK;
		}
		else {
			thisTurn.Item1.State = Actor.TurnState.INACTIVE;
		}
	}
}