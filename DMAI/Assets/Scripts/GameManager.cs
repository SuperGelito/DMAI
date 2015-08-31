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
	}

	// Update is called once per frame
	void Update () {
	
	}
}
