using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShipManager : Singleton<ShipManager>
{

    public List<ShipPart> ShipParts;

    [SerializeField] private Tilemap _grid;
    [SerializeField] private Tilemap _shipPartsMap;

    [SerializeField] private TileBase _crewWallTile;

    [SerializeField] private GameObject _selectCrewMenuPrefab;

    private GameObject _canvas;

    public bool WaitingForCrewSelect;
    
    private Vector3Int _gridPos;

    public Vector3 MovePos;
    
    public GameObject SelectCrewMenu;
    
    private void Start()
    {
        _canvas = GameObject.Find("Canvas");
    }

    private void Update()
    {
        if (WaitingForCrewSelect) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var tempGridPos = new Vector3Int(Mathf.FloorToInt(pos.x / _grid.cellSize.x),
                Mathf.FloorToInt(pos.y / _grid.cellSize.y), 0);

            var tile = _shipPartsMap.GetTile(tempGridPos);

            if (tile == null) return;
            
            if (tile.name == "square_part_broken")
            {
                _gridPos = tempGridPos;
                MovePos = new Vector3(_gridPos.x + _grid.cellSize.x / 2, _gridPos.y + _grid.cellSize.y / 2);
                if (MenuManager.Instance.MenuOpened)
                {
                    MenuManager.Instance.OpenedMenu.GetComponent<CrewMemberMenu>().RoleDropdown.Hide();
                    Destroy(MenuManager.Instance.OpenedMenu);
                    MenuManager.Instance.MenuOpened = false;
                }
                SelectCrewMenu = Instantiate(_selectCrewMenuPrefab, _canvas.transform);
                WaitingForCrewSelect = true;
            }
        }
    }

    public void CrewMemberSelected()
    {                                     
        _shipPartsMap.SetTile(_gridPos, _crewWallTile);
    }
}
