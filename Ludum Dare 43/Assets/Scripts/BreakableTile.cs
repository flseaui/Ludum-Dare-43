using System;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
[CreateAssetMenu(fileName = "New Breakable Tile", menuName = "ShipTiles/BreakableTile")]
public class BreakableTile : TileBase
{
    public Sprite Sprite;
}