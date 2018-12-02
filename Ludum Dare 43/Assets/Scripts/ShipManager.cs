using System;
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
    [SerializeField] private GameObject _oxygenTooltipPrefab;

    public interface ITileStats
    {
    }
    

    public struct WeightTileStats : ITileStats
    {
        public int Weight;

        public WeightTileStats(int weight)
        {
            Weight = weight;
        }
    }

    public struct OxygenTileStats : ITileStats
    {
        public int Oxygen;
        public bool Status;

        public OxygenTileStats(int oxygen, bool status)
        {
            Oxygen = oxygen;
            Status = status;
        }
    }

    
    private Dictionary<Vector3Int, ITileStats> TileToStatMap;
    
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
        TileToStatMap = new Dictionary<Vector3Int, ITileStats>();
        _prevPanelPos = Vector3Int.zero;
        SetupSpecialTiles();
    }

    private void Update()
    {
        if (GameManager.Instance.InShop) return;

        bool destroyTooltip = true;
        bool success = false;

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
            /********* TOOLTIPS *********/
            success = UIAtMousePosition(new[] {"crew_wall_part", "crew_wall_part_1"}, new Vector2(.5f, 1.5f),
                           ref destroyTooltip, delegate(Vector3Int gridPos)
                           {
                               var menu = Instantiate(_weightPanelPrefab, _canvas.transform);
                               menu.transform.Find("Text").GetComponent<TextMeshProUGUI>().text =
                                   $"{((WeightTileStats) TileToStatMap[gridPos]).Weight}lbs";
                               return menu;
                           }) 
                      && UIAtMousePosition(new[] {"oxygen_tank"}, new Vector2(1.5f, .5f), ref destroyTooltip,
                           delegate(Vector3Int gridPos)
                           {
                               var menu = Instantiate(_oxygenTooltipPrefab, _canvas.transform);
                               menu.transform.Find("Text").GetComponent<TextMeshProUGUI>().text =
                                   $"O2: {((OxygenTileStats) TileToStatMap[gridPos]).Oxygen}";
                               menu.transform.Find("Text 2").GetComponent<TextMeshProUGUI>().text =
                                   $"Status: {((OxygenTileStats) TileToStatMap[gridPos]).Status}";
                               return menu;
                           });
        }
        

        if (destroyTooltip && success)
        {
            if (MenuManager.Instance.WeightPanelOpened)
            {
                MenuManager.Instance.WeightPanelOpened = false;
                Destroy(MenuManager.Instance.OpenedPanel);
            }
        }
    }

    private bool UIAtMousePosition(string[] tileNames, Vector2 offsets, ref bool destTooltip, Func<Vector3Int, GameObject> ui)
    {
        var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var tempGridPos = new Vector3Int(Mathf.FloorToInt(pos.x / _grid.cellSize.x),
            Mathf.FloorToInt(pos.y / _grid.cellSize.y), 0);
        
        var tile = _shipPartsMap.GetTile(tempGridPos);

        if (tile != null)
        {
            if (tileNames.Contains(tile.name))
            {
                if (tempGridPos == _prevPanelPos && MenuManager.Instance.WeightPanelOpened)
                    return false;

                if (tempGridPos != _prevPanelPos)
                {
                    if (MenuManager.Instance.WeightPanelOpened)
                    {
                        MenuManager.Instance.WeightPanelOpened = false;
                        Destroy(MenuManager.Instance.OpenedPanel);
                    }
                }
                
                _prevPanelPos = tempGridPos;
                
                destTooltip = false;

                var uiGo = ui.Invoke(tempGridPos);

                MenuManager.Instance.WeightPanelOpened = true;

                float offsetPosX = tempGridPos.x + offsets.x;
                float offsetPosY = tempGridPos.y + offsets.y;

                // Final position of marker above GO in world space
                Vector3 offsetPos = new Vector3(offsetPosX, offsetPosY, tempGridPos.z);

                // Calculate *screen* position (note, not a canvas/recttransform position)
                Vector2 canvasPos;
                Vector2 screenPoint = Camera.main.WorldToScreenPoint(offsetPos);

                // Convert screen position to Canvas / RectTransform space <- leave camera null if Screen Space Overlay
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.GetComponent<RectTransform>(),
                    screenPoint, null, out canvasPos);

                // Set
                uiGo.GetComponent<RectTransform>().localPosition = canvasPos;

                MenuManager.Instance.OpenedPanel = uiGo;
            }
        }
        return true;
    }    
    
    public void CrewMemberSelected(CrewStats stats)
    {
        if (_shipPartsMap.GetTile(_gridPos).name == "square_part_broken_1")
            _shipPartsMap.SetTile(_gridPos, _crewWallTileHoriz);
        else
            _shipPartsMap.SetTile(_gridPos, _crewWallTileVert);

        --GameManager.Instance.Holes;
        
        TileToStatMap.Add(_gridPos, new WeightTileStats(stats.Weight));
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

    public void SetupSpecialTiles()
    {
        for (var i = _shipPartsMap.size.x / -2; i < _shipPartsMap.size.x / 2; i++)
        {
            for (var j = _shipPartsMap.size.y / -2; j < _shipPartsMap.size.y / 2; j++)
            {
                var tilePos = new Vector3Int(i, j, 0);
                if (_shipPartsMap.GetTile(tilePos) != null)
                {
                    switch (_shipPartsMap.GetTile(tilePos).name)
                    {
                        case "oxygen_tank":
                            TileToStatMap[tilePos] = new OxygenTileStats(1, true);
                            break;
                    }
                }
            }
        }
    }

    public void SetTileStats<T>(string tileName, T tileStats) where T : ITileStats
    {
        for (var i = _shipPartsMap.size.x / -2; i < _shipPartsMap.size.x / 2; i++)
        {
            for (var j = _shipPartsMap.size.y / -2; j < _shipPartsMap.size.y / 2; j++)
            {
                var tilePos = new Vector3Int(i, j, 0);
                if (_shipPartsMap.GetTile(tilePos) != null)
                {
                    if (_shipPartsMap.GetTile(tilePos).name == tileName)
                    {
                        TileToStatMap[tilePos] = tileStats;
                    }
                }
            }
        }
    }
    
}
