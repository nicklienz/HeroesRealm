using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ManajerTiles : MonoBehaviour
{
    public Grid grid;
    public Tilemap tilemap; // Tilemap yang akan digunakan untuk pathfinding
    public TileBase walkableTile, protectedTile, temporaryTile; // Tile yang dianggap sebagai jalur yang bisa dilewati
}
