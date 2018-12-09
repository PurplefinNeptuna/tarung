using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
SNJ = server ask new joined and give all connected user's name
CNJ = client answer with the name and host status (new joined only)
SCN = server broadcast new client's name
CSU = client send unit for the team
SSU = server broadcast team's unit
CSM = client send unit's movement
CSA = client send unit's attack
SSM = server broadcast movement
SSA = server broadcast attack
CHR = client (host) set run
SHR = broadcast of CHR
CRC = client (host) removing corpse
SRC = broadcast of CRC
CAB = Client abort game
SAB = broadcast CAB
*/


public class Client : MonoBehaviour {

	//meh
	public static Client Instance;

	public string clientName;
	public bool isHost;

	private bool socketReady = false;
	private TcpClient socket;
	private NetworkStream stream;
	private StreamWriter writer;
	private StreamReader reader;

	public List<GameClient> players = new List<GameClient>();

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else if (Instance != this) {
			Destroy(gameObject);
		}
	}

	// Use this for initialization
	void Start() {
		DontDestroyOnLoad(gameObject);
	}

	public bool ConnectToServer(string host, int port) {
		if (socketReady)
			return false;
		try {
			socket = new TcpClient(host, port);
			stream = socket.GetStream();
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);

			socketReady = true;
		}
		catch (Exception e) {
			WaitList.Instance.error = true;
			Debug.Log("Socket: " + e.Message);
		}
		return socketReady;
	}

	// Update is called once per frame
	void Update() {
		if (socketReady) {
			if (stream.DataAvailable) {
				string data = reader.ReadLine();
				if (data != null)
					IncomingData(data);
			}
		}
	}

	public void Send(string data) {
		if (!socketReady)
			return;

		writer.WriteLine(data);
		writer.Flush();
	}

	private void IncomingData(string data) {
		List<string> cdata = data.Split('|').ToList();
		Debug.Log("Client Receive: " + data + " Length " + cdata.Count);

		switch (cdata[0]) {
			case "SNJ":
				for (int i = 1; i < cdata.Count; i++) {
					string[] cl = cdata[i].Split('#');
					UserConnected(cl[0], cl[1] == "1" ? true : false);
				}
				Send("CNJ|" + clientName + "|" + (isHost ? 1 : 0).ToString());
				break;
			case "SCN":
				string[] tcl = cdata[1].Split('#');
				UserConnected(tcl[0], tcl[1] == "1" ? true : false);
				break;
			case "SSU":
				int team = int.Parse(cdata[1]);
				List<string> units = new List<string>();
				for (int i = 2; i < cdata.Count; i++) {
					units.Add(cdata[i]);
				}
				Prepare.main.MultiAddUnit(team, units);
				break;
			case "SSM": {
					int src = int.Parse(cdata[1]);
					if (src != -1) {
						string[] star = cdata[2].Split('#');
						Vector3Int vtar = new Vector3Int(int.Parse(star[0]), int.Parse(star[1]), 0);
						Main.main.MultiMove(src, vtar);
					}
					else if (isHost) {
						Main.main.TrySkip(true);
					}
					break;
				}
			case "SSA": {
					int src = int.Parse(cdata[1]);
					if (src != -1) {
						int tar = int.Parse(cdata[2]);
						Main.main.MultiAttack(src, tar);
					}
					else if (isHost) {
						Main.main.TrySkip(false);
					}
					break;
				}
			case "SHR": {
					int idx = int.Parse(cdata[1]);
					Main.main.TryRun(idx);
					break;
				}
			case "SRC": {
					int idx = int.Parse(cdata[1]);
					Main.main.TryRemoveCorpse(idx);
					break;
				}
			case "SAB":
				Main.main.LoadMenu();
				break;
		}
	}

	private void UserConnected(string data, bool host) {
		GameClient c = new GameClient() {
			name = data,
			isHost = host
		};

		players.Add(c);
	}

	private void OnApplicationQuit() {
		CloseSocket();
	}

	private void OnDisable() {
		CloseSocket();
	}

	private void CloseSocket() {
		if (!socketReady)
			return;
		writer.Close();
		reader.Close();
		socket.Close();
		socketReady = false;
	}
	public void Suicide() {
		CloseSocket();
		Destroy(gameObject);
	}
}
[Serializable]
public class GameClient {
	public string name;
	public bool isHost;
}