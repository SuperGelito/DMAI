using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using CSPNamespace;
using Assets.Scripts.Board;
using Assets.Scripts.Char;

public class BoardManager : MonoBehaviour {

	public GameObject Tile;
	public GameObject Wall;
	public GameObject Mud;
    public GameObject[] heroTypes;

	public int numTiles = 10;
	public int difficulty = 0;
    public int toleranceFM = 2;
    public int toleranceFW = 2;
    public int randomGeneratedTilesRate = 30;

    public Vector2 SelectedCell = new Vector2();

	private Cell[,] matrix;
    private List<Hero> heroList;
    private Dictionary<Guid, GameObject> cellInstances;
    private Dictionary<Guid, GameObject> charInstances;
	private Transform boardHolder;

    public GameObject guiManager;
    private GameObject guiManagerInstance;

    public Vector3 BoardCenter { get
        {
            
var x = guiManagerInstance.transform.position.x;
var y = guiManagerInstance.transform.position.y;
            var lateralMenuSize = guiManagerInstance.GetComponent<GuiManager>().HeroLateralMenu.GetComponent<RectTransform>().rect.width;
            return new Vector3(x + lateralMenuSize / 2, y,-10f);
        } }

    /// <summary>
    /// Override to start gui manager
    /// </summary>
    void Awake()
    {
        int centerScreen = numTiles / 2;
        Vector3 centerScreenVector = new Vector3(centerScreen, centerScreen, -10f);
        guiManagerInstance = (GameObject)GameObject.Instantiate(this.guiManager, centerScreenVector, Quaternion.identity);
        guiManagerInstance.GetComponent<GuiManager>().FitSize();
        RectTransform transform = guiManagerInstance.GetComponent<RectTransform>();
        transform.sizeDelta = new Vector2(numTiles, numTiles);
        guiManagerInstance.GetComponent<GuiManager>().SetHeroLatealMenu();

    }

    #region Board control
    /// <summary>
    /// Generates a board to play with
    /// </summary>
    public void CreateBoard()
	{
		matrix = new Cell[this.numTiles, this.numTiles];
        heroList = new List<Hero>();
        charInstances = new Dictionary<Guid, GameObject>();
        cellInstances = new Dictionary<Guid, GameObject>();
		boardHolder = this.transform;


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

        //Initialize heroes
        matrix = GenerateRandomHeroes(matrix);
        //Loop heroes to render
        foreach (var hero in heroList)
        {
            CreateHero(hero.Position, hero);
        }

        
    }

    /// <summary>
    /// Method to select a selectable cell
    /// </summary>
    /// <param name="cellPos"></param>
    /// <returns></returns>
    public bool ValidSelectCell(Vector3 cellPos)
    {
        //bool ret = false;
        
        if (cellPos.x >= 0 && cellPos.x < this.numTiles && cellPos.y >= 0 && cellPos.y < this.numTiles)
        {
            var cell = matrix[(int)cellPos.x, (int)cellPos.y];

            if (cell.overFloor == OverFloorType.Wall || cell.CellOwner != null)
            {
                return true;
            }
        }
        return false;
    }

    public void SelectCell(Vector3 cellPos)
    {
        guiManagerInstance.GetComponent<GuiManager>().SelectCell(cellPos);
        var cell = matrix[(int)cellPos.x, (int)cellPos.y];
        SelectedCell = cellPos;
        switch (cell.CellOwner.charType)
        {
            case CharType.Hero:

                break;
            case CharType.Enemy:

                break;
            case CharType.NonPlayerCharacter:

                break;
            default:
                break;             
        }
    }

    #endregion


    #region Board model management
    /// <summary>
    /// Creates a empty board
    /// </summary>
    /// <param name="matrix">Cell matrix to be filled</param>
    /// <returns>Cell matrix initialized</returns>
    private Cell[,] InitializeBoard(Cell[,] matrix)
    {
        //Initialize matrix
        for (int i = 0; i < matrix.GetLength(0); i++)
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                matrix[i, j] = new Cell();
                matrix[i, j].overFloor = OverFloorType.NONE;
                matrix[i, j].CellOwner = null;
            }

        return matrix;
    }

    /// <summary>
    /// Generates a random number of tiles using CSP constraint validaton
    /// </summary>
    /// <param name="matrix">Cell matrix to be filled</param>
    /// <returns>Cell matrix with random values generated</returns>
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
                        Debug.Log(string.Format("Position assigned:[{0},{1}] Value: {2}",x.ToString(),y.ToString(),randomType.ToString()));
                    }
                    else
                        matrix[x, y].overFloor = OverFloorType.NONE;
                }
            }
        }

        return matrix;
    }

    /// <summary>
    /// Generates a matrix with some discrete values (USED FOR TESTING)
    /// </summary>
    /// <param name="matrix">Cell matrix to be filled</param>
    /// <returns>Cell matrix with discrete values generated</returns>
    private Cell[,] GenerateDiscreteTiles(Cell[,] matrix)
    {
        

        return matrix;
    }

    /// <summary>
    /// Generates a random number of heroes to play
    /// </summary>
    /// <param name="matrix">Cell matrix used to set heroes</param>
    /// <returns>Cell matrix with heroes already set</returns>
    private Cell[,] GenerateRandomHeroes(Cell[,] matrix)
    {
        //Initialize a new random generator
        System.Random random = new System.Random();

        for (int i = 0; i < heroTypes.Count(); i++)
        {
            bool generatedValue = false;
            //Loop until a valid random generated value has been created for this turn
            while (!generatedValue)
            {
                //Generate a random number for x and y
                var x = random.Next(numTiles);
                var y = random.Next(numTiles);
                //Only assign value to unassigned
                Cell cell = matrix[x, y];
                if (cell.overFloor != OverFloorType.Wall && cell.IsFree)
                {
                    HeroType charType = (HeroType)Enum.Parse(typeof(HeroType),heroTypes[i].name);
                    Hero hero;
                    switch (charType)
                    {
                        case HeroType.Fighter:
                            hero = new Fighter(new Vector2(x, y));
                            cell.CellOwner = hero;
                            heroList.Add(hero);
                            break;
                    }
                    generatedValue = true;
                }
            }
        }

        return matrix;
    }
    #endregion

    #region Render board
    /// <summary>
    /// Instantiate a tile with data provided 
    /// </summary>
    /// <param name="pos">Position to instantiate the object</param>
    /// <param name="over">Type of object to be instantiated</param>
    private void CreateTile(Vector3 pos,OverFloorType over)
	{
        var cell = matrix[(int)pos.x, (int)pos.y];
        GameObject tile = (GameObject)GameObject.Instantiate (this.Tile, pos, Quaternion.identity);
        cellInstances[cell.Id] = tile;

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

    /// <summary>
    /// Instantiate a hero
    /// </summary>
    /// <param name="pos">Position to instantiate a hero</param>
    /// <param name="type">Type of hero</param>
    private void CreateHero(Vector3 pos, Hero hero)
    {
        //Get prefab resource
        GameObject heroPrefab = (GameObject)Resources.Load(hero.heroType.ToString());

        GameObject heroInstance = (GameObject)GameObject.Instantiate(heroPrefab, pos, Quaternion.identity);

        heroInstance.transform.SetParent(boardHolder);

        charInstances.Add(hero.Id, heroInstance);
    }
    #endregion

    #region CSP interface
    /// <summary>
    /// Generates a list of variable to be used in a CSP
    /// </summary>
    /// <param name="matrix">Cell matrix used as source</param>
    /// <returns>List of variables used with value assigned</returns>
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
    #endregion
}




