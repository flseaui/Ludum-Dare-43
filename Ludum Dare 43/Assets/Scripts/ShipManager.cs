using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class ShipManager : Singleton<ShipManager>
{

    [SerializeField] private Tilemap _grid;
    [SerializeField] private Tilemap _shipPartsMap;

    [SerializeField] private TileBase _crewWallTileVert;
    [SerializeField] private TileBase _crewWallTileHoriz;
    [SerializeField] private TileBase _brokenWallTileVert;
    [SerializeField] private TileBase _brokenWallTileHoriz;
    
    [SerializeField] private GameObject _selectCrewMenuPrefab;
    [SerializeField] private GameObject _weightPanelPrefab;

    struct TileStats
    {
        public int Weight;

        public TileStats(int weight)
        {
            Weight = weight;
        }
    }

    private Dictionary<Vector3Int, TileStats> TileToStatMap;
    
    private GameObject _canvas;

    public bool WaitingForCrewSelect;
    public bool CrewMovingToBreak;
    
    private Vector3Int _gridPos;

    public Vector3 MovePos;

    private Vector3Int _prevPanelPos;
    
    public GameObject SelectCrewMenu;
    public GameObject WeightMenu;
    
    private void Start()
    {
        _canvas = GameObject.Find("Canvas");
        TileToStatMap = new Dictionary<Vector3Int, TileStats>();
        _prevPanelPos = Vector3Int.zero;
    }

    private void Update()
    {

        bool destroyTooltip = true;

        if (Input.GetMouseButtonDown(0) && !WaitingForCrewSelect && !CrewMovingToBreak)
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var tempGridPos = new Vector3Int(Mathf.FloorToInt(pos.x / _grid.cellSize.x),
                Mathf.FloorToInt(pos.y / _grid.cellSize.y), 0);

            var tile = _shipPartsMap.GetTile(tempGridPos);

            if (tile != null)
            {
                if (tile.name == "square_part_broken" || tile.name == "square_part_broken_1")
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
        else
        {
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var tempGridPos = new Vector3Int(Mathf.FloorToInt(pos.x / _grid.cellSize.x),
                Mathf.FloorToInt(pos.y / _grid.cellSize.y), 0);
            
            var tile = _shipPartsMap.GetTile(tempGridPos);

            if (tile != null)
            {
                if (tile.name == "crew_wall_part" || tile.name == "crew_wall_part_1")
                {
                    if (tempGridPos == _prevPanelPos && MenuManager.Instance.WeightPanelOpened)
                        return;

                    if (tempGridPos != _prevPanelPos)
                    {
                        if (MenuManager.Instance.WeightPanelOpened)
                        {
                            MenuManager.Instance.WeightPanelOpened = false;
                            Destroy(MenuManager.Instance.OpenedPanel);
                        }
                    }
                    
                    _prevPanelPos = tempGridPos;
                    
                    destroyTooltip = false;

                    WeightMenu = Instantiate(_weightPanelPrefab, _canvas.transform);
                    WeightMenu.transform.Find("Text").GetComponent<TextMeshProUGUI>().text =
                        $"{TileToStatMap[tempGridPos].Weight}lbs";

                    MenuManager.Instance.WeightPanelOpened = true;

                    float offsetPosY = tempGridPos.y + 1.5f;

                    // Final position of marker above GO in world space
                    Vector3 offsetPos = new Vector3(tempGridPos.x + .5f, offsetPosY, tempGridPos.z);

                    // Calculate *screen* position (note, not a canvas/recttransform position)
                    Vector2 canvasPos;
                    Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);

                    // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.GetComponent<RectTransform>(),
                        screenPoint, null, out canvasPos);

                    // Set
                    WeightMenu.GetComponent<RectTransform>().localPosition = canvasPos;

                    MenuManager.Instance.OpenedPanel = WeightMenu;
                }
            }
        }
        

        if (destroyTooltip)
        {
            if (MenuManager.Instance.WeightPanelOpened)
            {
                MenuManager.Instance.WeightPanelOpened = false;
                Destroy(MenuManager.Instance.OpenedPanel);
            }
        }
    }

    public void CrewMemberSelected(CrewStats stats)
    {
        if (_shipPartsMap.GetTile(_gridPos).name == "square_part_broken_1")
            _shipPartsMap.SetTile(_gridPos, _crewWallTileHoriz);
        else
            _shipPartsMap.SetTile(_gridPos, _crewWallTileVert);

        --GameManager.Instance.Holes;
        
        TileToStatMap.Add(_gridPos, new TileStats(stats.Weight));
    }

    public void BreakRandomPartOfType(int numTilesToBreak)
    {
        var breakableTiles = new List<Vector3Int>();
        for (var i = _shipPartsMap.size.x / -2; i < _shipPartsMap.size.x / 2; i++)
        {
            for (var j = _shipPartsMap.size.y / -2; j < _shipPartsMap.size.y / 2; j++)
            {
                var tilePos = new Vector3Int(i, j, 0);
                if (_shipPartsMap.GetTile(tilePos) != null)
                {
                    if (_shipPartsMap.GetTile(tilePos).name == "square_part")
                    {
                        breakableTiles.Add(tilePos);
                    }
                }
            }
        }
        
        var random = new System.Random();

        if (numTilesToBreak > breakableTiles.Count)
            numTilesToBreak = breakableTiles.Count;
        
        GameManager.Instance.Holes += numTilesToBreak;
        
        for (var i = 0; i < numTilesToBreak; i++)
        {
            var index = random.Next(breakableTiles.Count);
            
            if (breakableTiles[index] == new Vector3Int(7, -1, 0) || breakableTiles[index] == new Vector3Int(7, 0, 0)
                                                                 || breakableTiles[index] == new Vector3Int(-7, -3, 0)
                                                                 || breakableTiles[index] == new Vector3Int(-8, -3, 0)
                                                                 || breakableTiles[index] == new Vector3Int(-7, 2, 0)
                                                                 || breakableTiles[index] == new Vector3Int(-8, 2, 0))
                _shipPartsMap.SetTile(breakableTiles[index], _brokenWallTileHoriz);
            else
                _shipPartsMap.SetTile(breakableTiles[index], _brokenWallTileVert);
            
            breakableTiles.RemoveAt(index);
        }
    }
}
