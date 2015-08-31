using UnityEngine;
using System.Collections;
using System;

public class BoardManager : MonoBehaviour {

	public GameObject Tile;
	public GameObject Wall;
	public GameObject Mud;

	public int numTiles = 10;
	public int difficulty = 0;

	private Cell[,] matrix;

	private Transform boardHolder;

	public void CreateBoard()
	{
		matrix = new Cell[this.numTiles, this.numTiles];
		boardHolder = new GameObject ("Board").transform;
		System.Random random = new System.Random();

		for (int x = 0; x < matrix.GetLength(0); x += 1) {
			for (int y = 0; y < matrix.GetLength(1); y += 1) {
			Array overfloortypes = Enum.GetValues(typeof(OverFloorType));
			OverFloorType randomCellType = (OverFloorType)overfloortypes.GetValue(random.Next(overfloortypes.Length));
			CreateTile(new Vector3(x,y),randomCellType);
		}
	}
	}

	private void CreateTile(Vector3 pos,OverFloorType over)
	{
		GameObject tile = (GameObject)GameObject.Instantiate (this.Tile,pos, Quaternion.identity);

		tile.transform.SetParent (boardHolder);
		GameObject toInstantiate = null;
		switch (over) {
		case OverFloorType.Mud:
				toInstantiate = this.Mud;
			break;
		case OverFloorType.Wall:
			toInstantiate = this.Wall;
			break;
		default:
			break;
		}

		if (toInstantiate != null) {
			GameObject overObject = (GameObject)GameObject.Instantiate (toInstantiate, pos, Quaternion.identity);
			overObject.transform.SetParent(tile.transform);
		}

	}

}

public class Cell
{
	public OverFloorType overFloor = OverFloorType.Floor;
}

public enum OverFloorType
{
	Floor = 0,
	Wall = 1,
	Mud = 2
}
