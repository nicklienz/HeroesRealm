using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ImageToTilemap : MonoBehaviour {
    public Texture2D image;
    public Tile[] tiles;
    public Tilemap tilemap;

    void Start() {
        for (int y = 0; y < image.height; y++) {
            for (int x = 0; x < image.width; x++) {
                Color color = image.GetPixel(x, y);
                foreach (Tile tile in tiles) {
                    if (tile.sprite.texture.GetPixel(0, 0) == color) {
                        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                        break;
                    }
                }
            }
        }
    }
}
