using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Utility {
	public enum Direction {
		N,
		E,
		S,
		W
	}

	static List<Vector3Int> dir = new List<Vector3Int>() {
			new Vector3Int(0,1,0),
			new Vector3Int(1,0,0),
			new Vector3Int(0,-1,0),
			new Vector3Int(-1,0,0)
	};

	public static List<Tuple<Vector3Int, List<Direction>>> SelectableArea(Vector3Int position, Dictionary<Vector3Int, WorldTiles> source, int radius, int team, bool searchForActor = false) {
		if (radius == 0 || (source.ContainsKey(position) && source[position].TilemapMember.tag == "object"))
			return null;
		List<Vector3Int> visited = new List<Vector3Int>();
		List<Tuple<Vector3Int, List<Direction>>> ans = new List<Tuple<Vector3Int, List<Direction>>>();
		Queue<Tuple<Vector3Int, List<Direction>>> wait = new Queue<Tuple<Vector3Int, List<Direction>>>();
		visited.Add(position);
		wait.Enqueue(new Tuple<Vector3Int, List<Direction>>(position, new List<Direction>()));
		while (wait.Count != 0) {
			Vector3Int nowpos = wait.Peek().Item1;
			List<Direction> nowdir = wait.Peek().Item2;
			wait.Dequeue();
			for (int i = 0; i < 4; i++) {
				bool hasActor = Main.main.actors.FirstOrDefault(x => x.Position == nowpos + dir[i] && x.Health > 0) != null;
				bool enemyInside = Main.main.actors.FirstOrDefault(x => x.Position == nowpos + dir[i] && x.Health > 0 && x.TeamID != team) != null;
				if ((source.ContainsKey(nowpos + dir[i]) && source[nowpos + dir[i]].TilemapMember.tag == "ground") && !visited.Contains(nowpos + dir[i]) && nowdir.Count < radius && (!hasActor || (hasActor && searchForActor))) {
					visited.Add(nowpos + dir[i]);
					List<Direction> tempdir = new List<Direction>(nowdir) {
						(Direction)i
					};
					Tuple<Vector3Int, List<Direction>> tempans = new Tuple<Vector3Int, List<Direction>>(nowpos + dir[i], tempdir);
					if ((!hasActor && !searchForActor) || (enemyInside && searchForActor))
						ans.Add(tempans);
					wait.Enqueue(tempans);
				}
			}
		}
		return ans;
	}

	public static Vector3Int NearestEnemy(Vector3Int position, Dictionary<Vector3Int, WorldTiles> source, int radius, int team = 2, bool seeThroughActors = false) {
		if (radius == 0 || (source.ContainsKey(position) && source[position].TilemapMember.tag == "object"))
			return position;
		if (Main.main.actors.Count(x => x.TeamID != team) <= 0)
			return position;
		Tuple<Vector3Int, int, Vector3Int> ans = new Tuple<Vector3Int, int, Vector3Int>(position, 0, position);
		List<Vector3Int> visited = new List<Vector3Int>();
		Queue<Tuple<Vector3Int, int, Vector3Int>> wait = new Queue<Tuple<Vector3Int, int, Vector3Int>>();
		visited.Add(position);
		wait.Enqueue(new Tuple<Vector3Int, int, Vector3Int>(position, 0, position));
		while (wait.Count != 0) {
			Tuple<Vector3Int, int, Vector3Int> oldPos = wait.Dequeue();
			for (int i = 0; i < 4; i++) {
				Vector3Int thisPos = oldPos.Item1 + dir[i];
				bool hasActor = Main.main.actors.FirstOrDefault(x => x.Position == thisPos && x.Health > 0) != null;
				bool enemyInside = Main.main.actors.FirstOrDefault(x => x.Position == thisPos && x.Health > 0 && x.TeamID != team) != null;
				if (enemyInside) {
					ans = new Tuple<Vector3Int, int, Vector3Int>(thisPos, oldPos.Item2 + 1, oldPos.Item3);
					Debug.Log("Searching from " + position + " to " + ans.Item1 + " via " + ans.Item3 + ".Distance: " + ans.Item2);
					return ans.Item3;
				}
				else if ((source.ContainsKey(thisPos) && source[thisPos].TilemapMember.tag == "ground") && !visited.Contains(thisPos) && (!hasActor || seeThroughActors)) {
					visited.Add(thisPos);
					Tuple<Vector3Int, int, Vector3Int> tempans = new Tuple<Vector3Int, int, Vector3Int>(thisPos, oldPos.Item2 + 1, (oldPos.Item2 < radius && !hasActor) ? thisPos : oldPos.Item3);
					wait.Enqueue(tempans);
				}
			}
		}
		return position;
	}
}

public static class CanvasPositioningExtensions {
	public static Vector3 WorldToCanvasPosition(this Canvas canvas, Vector3 worldPosition, Camera camera = null) {
		if (camera == null) {
			camera = Camera.main;
		}
		var viewportPosition = camera.WorldToViewportPoint(worldPosition);
		return canvas.ViewportToCanvasPosition(viewportPosition);
	}

	public static Vector3 ScreenToCanvasPosition(this Canvas canvas, Vector3 screenPosition) {
		var viewportPosition = new Vector3(screenPosition.x / Screen.width,
										   screenPosition.y / Screen.height,
										   0);
		return canvas.ViewportToCanvasPosition(viewportPosition);
	}

	public static Vector3 ViewportToCanvasPosition(this Canvas canvas, Vector3 viewportPosition) {
		var centerBasedViewPortPosition = viewportPosition - new Vector3(0.5f, 0.5f, 0);
		var canvasRect = canvas.GetComponent<RectTransform>();
		var scale = canvasRect.sizeDelta;
		return Vector3.Scale(centerBasedViewPortPosition, scale);
	}
}