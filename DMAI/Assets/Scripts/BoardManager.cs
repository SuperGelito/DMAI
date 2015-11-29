using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using CSPNamespace;

public class BoardManager : MonoBehaviour {

	public GameObject Tile;
	public GameObject Wall;
	public GameObject Mud;

	public int numTiles = 10;
	public int difficulty = 0;
    public int toleranceFM = 2;
    public int toleranceFW = 2;
    public int randomGeneratedTilesRate = 30;

	private Cell[,] matrix;

	private Transform boardHolder;

	public void CreateBoard()
	{
		matrix = new Cell[this.numTiles, this.numTiles];
		boardHolder = new GameObject ("Board").transform;

        bool foundSolution = false;
        List<Assignment> assignments = new List<Assignment>();
        while (!foundSolution)
        {
            matrix = InitializeBoard(matrix);

            matrix = GenerateRandomTiles(matrix);

            //Create a CSP to fill the remaining data
            List<Variable> vars = ConvertMatrixToVars(matrix);
            List<OverFloorType> values = Enum.GetValues(typeof(OverFloorType)).OfType<OverFloorType>().Where(o => Convert.ToInt32(o) >= 0).ToList();
            CSP problem = new CSP(vars, values, toleranceFW, toleranceFM);
            //Use recursive backtracking to get a solution
            assignments = Search.RecursiveBacktrackingSearch(problem);
            if (assignments != null)
                foundSolution = true;
        }

        //Loop assignments
        foreach (var ass in assignments)
        {
            int x = (int)ass.variable.pos.x;
            int y = (int)ass.variable.pos.y;
            matrix[x,y].overFloor = ass.value;
            CreateTile(new Vector3(ass.variable.pos.x, ass.variable.pos.y), ass.value);
        }
        
	}

    private Cell[,] InitializeBoard(Cell[,] matrix)
    {
        //Initialize matrix
        for (int i = 0; i < matrix.GetLength(0); i++)
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = new Cell();
                matrix[i, j].overFloor = OverFloorType.NONE;
            }

        return matrix;
    }

    private Cell[,] GenerateRandomTiles(Cell[,] matrix)
    {
        //Initialize a new random generator
        System.Random random = new System.Random();

        //Get number of tiles
        var totalNumberOfTiles = this.numTiles * this.numTiles;
        var numberOfRandomTiles = totalNumberOfTiles * randomGeneratedTilesRate / 100;
        //List with all possible values
        List<OverFloorType> values = Enum.GetValues(typeof(OverFloorType)).OfType<OverFloorType>().Where(o => Convert.ToInt32(o) >= 0).ToList();
        
        for (int i = 0; i < numberOfRandomTiles; i++)
        {
            bool generatedValue = false;
            //Loop until a valid random generated value has been created for this turn
            while (!generatedValue)
            {
                //Generate a random number for x and y
                var x = random.Next(numTiles);
                var y = random.Next(numTiles);
                //Only assign value to unassigned 
                if (matrix[x, y].overFloor == OverFloorType.NONE)
                {
                    //Generate a random value
                    Array overfloortypes = Enum.GetValues(typeof(OverFloorType));
                    OverFloorType randomType = OverFloorType.NONE;
                    while (randomType == OverFloorType.NONE)
                        randomType = (OverFloorType)overfloortypes.GetValue(random.Next(0, overfloortypes.Length));
                    //Assign the value
                    matrix[x, y].overFloor = randomType;
                    List<Variable> vars = ConvertMatrixToVars(matrix);
                    CSP problem = new CSP(vars, values, toleranceFW, toleranceFM);
                    if (problem.ValidateConstraints())
                    {
                        generatedValue = true;
                    }
                    else
                        matrix[x, y].overFloor = OverFloorType.NONE;
                }
            }
        }

        return matrix;
    }

    private List<Variable> ConvertMatrixToVars(Cell[,] matrix)
    {
        List<Variable> vars = new List<Variable>();
        for (int i = 0; i < matrix.GetLength(0); i++)
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                Cell cell = matrix[i, j];
                var pos = new Vector2(i, j);
                Variable var = cell.overFloor == OverFloorType.NONE ? new Variable(pos) : new Variable(pos, cell.overFloor);
                vars.Add(var);
            }
        return vars;
    }

	private void CreateTile(Vector3 pos,OverFloorType over)
	{
        int quantityToCenter = -5;

        Vector3 centeredPos = new Vector3(pos.x + quantityToCenter, pos.y + quantityToCenter);

        GameObject tile = (GameObject)GameObject.Instantiate (this.Tile, centeredPos, Quaternion.identity);
        
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
			GameObject overObject = (GameObject)GameObject.Instantiate (toInstantiate, centeredPos, Quaternion.identity);
			overObject.transform.SetParent(tile.transform);
		}

	}



}

public class Cell
{
    public Cell()
    {

    }
	public OverFloorType overFloor = OverFloorType.Floor;
}

public enum OverFloorType
{
    NONE = -1,
	Floor = 0,
	Wall = 1,
	Mud = 2
}
