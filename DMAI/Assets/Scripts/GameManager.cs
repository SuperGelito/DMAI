using UnityEngine;
using System.Collections;

public  class GameManager : MonoBehaviour {

	private static GameManager instance;
	public GameObject boardManager;
    public GameObject boardManagerInstance;

    
    private bool actionMode = false;

	public GameManager GetInstance()
	{
		if (instance == null)
			instance = this;
		else if (instance != null && instance != this)
			Destroy (gameObject);


		return instance;
	}

	void Awake()
	{
		instance = GetInstance ();
		DontDestroyOnLoad (gameObject);
        boardManagerInstance = (GameObject)GameObject.Instantiate(this.boardManager, Vector3.zero, Quaternion.identity);
        boardManagerInstance.GetComponent<BoardManager> ().CreateBoard ();
        //Calculate variables of position and size
        int size = boardManagerInstance.GetComponent<BoardManager>().numTiles;
        //Calculate center of the screen
        var centerScreen = boardManagerInstance.GetComponent<BoardManager>().BoardCenter;
        CenterCamera(centerScreen, size);
        actionMode = true;
    }

    void CenterCamera(Vector3 centerScreenVector, int size)
    {
        Camera mainCam = Camera.main;
        mainCam.transform.position = centerScreenVector;
        mainCam.orthographicSize = Mathf.RoundToInt(size / 2.1f) ;
    }

    

	// Update is called once per frame
	void FixedUpdate () {
        if (actionMode)
        {
            if (Input.GetMouseButton(0))
            {
                var v3 = Input.mousePosition;
                v3.z = 10.0f;
                v3 = Camera.main.ScreenToWorldPoint(v3);
                var x = Mathf.RoundToInt(v3.x);
                var y = Mathf.RoundToInt(v3.y);
                var selectedPosition = new Vector3(x, y);
                if (boardManagerInstance.GetComponent<BoardManager>().ValidSelectCell(selectedPosition))
                {
                    boardManagerInstance.GetComponent<BoardManager>().SelectCell(selectedPosition);
                }
            }
        }
    }
}
