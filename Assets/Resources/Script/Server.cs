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
*/
public class Server : MonoBehaviour {

	//meh
	public static Server Instance;

	public int port = 7777;
	private List<ServerClient> clients;
	private List<ServerClient> disconnectList;
	private TcpListener server;
	private bool serverStarted;

	private void Awake() {
		if (Instance == null) {
			Instance = this;
		}
		else if (Instance != this) {
			Destroy(gameObject);
		}
	}

	public void Init() {
		DontDestroyOnLoad(gameObject);
		clients = new List<ServerClient>();
		disconnectList = new List<ServerClient>();
		try {
			server = new TcpListener(IPAddress.Any, port);
			server.Start();

			StartListening();
			serverStarted = true;
		}
		catch(Exception e){
			WaitList.Instance.error = true;
			Debug.Log("Socket: " + e.Message);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!serverStarted)
			return;
		foreach (var c in clients) {
			if (!IsConnected(c.tcp)) {
				c.tcp.Close();
				disconnectList.Add(c);
			}
			else {
				NetworkStream s = c.tcp.GetStream();
				if (s.DataAvailable) {
					StreamReader reader = new StreamReader(s, true);
					string data = reader.ReadLine();

					if (data != null) {
						IncomingData(c, data);
					}
				}
			}
		}
		for (int i = 0; i < disconnectList.Count; i++) {
			clients.Remove(disconnectList[i]);
			disconnectList.RemoveAt(i);
		}
	}

	private void StartListening() {
		server.BeginAcceptTcpClient(ClientConnect, server);
	}

	private void ClientConnect(IAsyncResult ar) {
		Debug.Log("New Client Connected!");
		TcpListener listener = (TcpListener)ar.AsyncState;
		string allUsers = "";
		foreach (var i in clients) {
			allUsers += "|" + i.clientName + "#" + (i.isHost ? 1 : 0);
		}
		ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
		clients.Add(sc);

		StartListening();

		Broadcast("SNJ" + allUsers, clients[clients.Count - 1]);
	}

	private bool IsConnected(TcpClient c) {
		try {
			if (c != null && c.Client != null && c.Client.Connected) {
				if (c.Client.Poll(0, SelectMode.SelectRead))
					return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
				return true;
			}
			else
				return false;
		}
		catch{
			return false;
		}
	}

	private void Broadcast(string data, List<ServerClient> cl) {
		foreach (var sc in cl) {
			try {
				StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
				writer.WriteLine(data);
				writer.Flush();
			}
			catch(Exception e){
				Debug.Log("Write: " + e.Message);
			}
		}
	}

	private void Broadcast(string data, ServerClient c) {
		List<ServerClient> sc = new List<ServerClient>() { c };
		Broadcast(data, sc);
	}

	private void IncomingData(ServerClient c, string data) {
		Debug.Log("Server Receive: " + data);
		List<string> cData = data.Split('|').ToList();

		string uData;

		switch (cData[0]) {
			case "CNJ":
				c.clientName = cData[1];
				c.isHost = cData[2] == "0" ? false : true;
				Broadcast("SCN|" + c.clientName + "#" + (c.isHost ? 1 : 0), clients);
				break;
			case "CSU":
				uData = "SSU";
				for (int i = 1; i < cData.Count; i++) {
					uData += "|" + cData[i];
				}
				Broadcast(uData, clients);
				break;
			case "CSM":
				uData = "SSM";
				for (int i = 1; i < cData.Count; i++) {
					uData += "|" + cData[i];
				}
				Broadcast(uData, clients);
				break;
			case "CSA":
				uData = "SSA";
				for (int i = 1; i < cData.Count; i++) {
					uData += "|" + cData[i];
				}
				Broadcast(uData, clients);
				break;
			case "CHR":
				uData = "SHR|";
				uData += cData[1];
				Broadcast(uData, clients);
				break;
			case "CRC":
				uData = "SRC|";
				uData += cData[1];
				Broadcast(uData, clients);
				break;
		}
	}
	public void Suicide() {
		server.Stop();
		Destroy(gameObject);
	}

}

[Serializable]
public class ServerClient {
	public string clientName;
	public TcpClient tcp;
	public bool isHost;

	public ServerClient(TcpClient tcp) {
		this.tcp = tcp;
	}
}