using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

	public GameObject gameMenu;
	public GameObject multiMenu;

	public GameObject serverPrefab;
	public GameObject clientPrefab;

	public InputField address;
	public InputField playerName;

	public void RunGame() {
		SceneManager.LoadScene(2);
	}

	public void ShowMain() {
		gameMenu.SetActive(true);
		multiMenu.SetActive(false);
	}

	public void ShowMulti() {
		gameMenu.SetActive(false);
		multiMenu.SetActive(true);
	}

	public void HostButton() {
		try {
			GameObject gS = Instantiate(serverPrefab);
			gS.name = "ServerGO";
			Server s = gS.GetComponent<Server>();
			s.Init();

			GameObject gC = Instantiate(clientPrefab);
			gC.name = "ClientGO";
			Client c = gC.GetComponent<Client>();
			c.clientName = playerName.text == null || playerName.text == "" ? "Host" : playerName.text;
			c.isHost = true;

			c.ConnectToServer("127.0.0.1", 7777);
			SceneManager.LoadScene(3);
		}
		catch{
			Destroy(GameObject.Find("ServerGO"));
		}
	}

	public void JoinButton() {
		try {
			GameObject gC = Instantiate(clientPrefab);
			gC.name = "ClientGO";
			Client c = gC.GetComponent<Client>();
			c.clientName = playerName.text == null || playerName.text == "" ? "Client" : playerName.text;
			c.ConnectToServer(address.text == null || address.text == "" ? "127.0.0.1" : address.text, 7777);
			SceneManager.LoadScene(3);
		}
		catch{
			Destroy(GameObject.Find("ClientGO"));
		}
	}
}
