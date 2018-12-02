using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class TurnQueue {
	List<Tuple<Actor, float, int>> queue;
	public bool active;
	public int Count {
		get {
			return queue.Count;
		}
	}

	public List<Actor> GetQueue {
		get {
			List<Actor> ans = new List<Actor>();
			foreach (var item in queue) {
				ans.Add(item.Item1);
			}
			return ans;
		}
	}

	public TurnQueue() {
		queue = new List<Tuple<Actor, float, int>>();
		active = false;
	}

	void Sort() {
		queue.Sort((x, y) => x.Item2 - y.Item2 < 0 ? -1 : 1);
	}

	public TurnQueue(List<Actor> actors) {
		queue = new List<Tuple<Actor, float, int>>();
		for (int i = 0; i < actors.Count; i++) {
			var actor = actors[i];
			//foreach (var actor in actors) {
			queue.Add(new Tuple<Actor, float, int>(actor, actor.Speed, i));
		}
		Sort();
		active = false;
	}

	public Actor Peek() {
		return queue.ElementAt(0)?.Item1;
	}

	public void Add(Actor item, int idx) {
		queue.Add(new Tuple<Actor, float, int>(item, item.Speed, idx));
		Sort();
	}

	public Tuple<Actor,int> Pop() {
		if (queue.Count == 0)
			return null;
		Tuple<Actor, float, int> temp = queue.ElementAt(0);
		queue.RemoveAt(0);
		for(int i = 0; i<queue.Count; i++) {
			queue[i] = new Tuple<Actor, float, int>(queue[i].Item1, queue[i].Item2 - temp.Item2, queue[i].Item3);
		}
		Sort();
		return new Tuple<Actor, int>(temp.Item1, temp.Item3);
	}
}