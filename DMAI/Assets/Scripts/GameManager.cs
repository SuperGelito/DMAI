using UnityEngine;
using System.Collections;

public  class GameManager : MonoBehaviour {

	private static GameManager instance;
	public GameObject boardManager;
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
		boardManager.GetComponent<BoardManager> ().CreateBoard ();
        CenterCamera(boardManager.GetComponent<BoardManager>().numTiles);
    }

    void CenterCamera(int size)
    {
        Camera mainCam = Camera.main;
        int centerScreen = (int)(size /2);
        Vector3 centerScreenVector = new Vector3(centerScreen, centerScreen, -10f);
        mainCam.transform.position = centerScreenVector;
        mainCam.orthographicSize = Mathf.RoundToInt(size / 2) ;
    }

	// Update is called once per frame
	void Update () {
	
	}
}
