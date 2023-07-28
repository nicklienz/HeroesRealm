using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ManajerTiles : MonoBehaviour
{
    public static ManajerTiles instance;
    public static ManajerTiles Instance
    {
        get
        {
            // Jika instance belum ada, cari di scene
            if (instance == null)
            {
                instance = FindObjectOfType<ManajerTiles>();
            }
            
            return instance;
        }
    }
    public Grid grid;
    public Tilemap tilemap; // Tilemap yang akan digunakan untuk pathfinding
    public Tilemap tempTilemap;
    public TileBase walkableTile, protectedTile, temporaryTile, charTile; // Tile yang dianggap sebagai jalur yang bisa dilewati

    public void RemoveTile(Vector3Int position)
    {
        tempTilemap.SetTile(position, null);
    }

    // Method untuk menambahkan tile di posisi tertentu
    public void AddTile(Vector3Int position)
    {
        tempTilemap.SetTile(position, temporaryTile);
    }
}
