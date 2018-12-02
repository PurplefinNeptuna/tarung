using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class WorldTiles {
	public Vector3Int LocalPlace {
		get; set;
	}

	public Vector3 WorldLocation {
		get; set;
	}

	public TileBase TileBase {
		get; set;
	}

	public Tilemap TilemapMember {
		get; set;
	}

	public string Name {
		get; set;
	}

	public int Cost {
		get; set;
	}
}
