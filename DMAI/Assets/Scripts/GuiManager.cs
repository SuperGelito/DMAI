using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GuiManager : MonoBehaviour {

    public GameObject ValidTile;
    public GameObject InvalidTile;
    public GameObject SelectTile;
    public GameObject HeroLateralMenu;
    private GameObject heroLateralMenuInstance;
    public float rateToCenter = 0.5f;
    private GameObject selectedTile;
    private List<GameObject> markedTiles;
	// Use this for initialization
	void Awake () {
        markedTiles = new List<GameObject>();
    }
	
	// Update is called once per frame
	void Update () {
	    
	}

    public void FitSize()
    {
        Vector3 position = GetComponent<RectTransform>().position;
        GetComponent<RectTransform>().position = ConvertToGuiPosition(position);
    }

    public void SetHeroLatealMenu()
    {
        var rect = GetComponent<RectTransform>();
        var x = rect.rect.width;
        var y = 0;//rect.position.x + rect.rect.height;
        var lateralMenuPosition = ConvertToGuiPosition(new Vector3(x, y));
        heroLateralMenuInstance = (GameObject)GameObject.Instantiate(this.HeroLateralMenu,lateralMenuPosition, Quaternion.identity);
        heroLateralMenuInstance.transform.SetParent(this.transform);
    }

    public Vector3 ConvertToGuiPosition(Vector3 position)
    {
        return new Vector3(position.x - rateToCenter, position.y - rateToCenter, position.z);
    }


    public void SelectCell(Vector3 coordinates)
    {
        ClearSelectedTiles();
        ClearMarkedTiles();
        GameObject selection = (GameObject)GameObject.Instantiate(this.SelectTile, ConvertToGuiPosition(coordinates), Quaternion.identity);
        selection.transform.SetParent(this.GetComponent<Canvas>().transform);
        selectedTile = selection;
    }

    public void MarkMoveCell(Vector3 coordinates)
    {
        ClearMarkedTiles();
        GameObject selection = (GameObject)GameObject.Instantiate(this.ValidTile, ConvertToGuiPosition(coordinates), Quaternion.identity);
        selection.transform.SetParent(this.GetComponent<Canvas>().transform);
        markedTiles.Add(selection);
    }

    private void ClearSelectedTiles()
    {
        Destroy(selectedTile);
    }

    private void ClearMarkedTiles()
    {
        for (int i = 0; i < markedTiles.Count(); i++)
        {
            Destroy(markedTiles[i]);
        }
    }
}
