using UnityEngine;
using UnityEngine.Tilemaps;

// SpriteCook dual-grid tile (self-contained, needs only the built-in 2D Tilemap package).
//
// Paint cells on a single Tilemap as usual. Each painted cell renders the art that matches
// its 4-corner neighbourhood and is offset by half a tile, so terrain edges fall on the cell
// corners. That is the classic "dual grid" look our atlases are authored for.
//
// Corner mask bits: 1 = this cell (NW of the 2x2 it anchors), 2 = +X (NE),
// 4 = -Y (SW), 8 = +X -Y (SE). Sprites[mask] holds the art for each of the 16 combos.
[CreateAssetMenu(menuName = "SpriteCook/Dual Grid Tile")]
public class SpriteCookDualGridTile : TileBase
{
    public Sprite[] Sprites = new Sprite[16];

    static bool Filled(ITilemap tilemap, Vector3Int p)
    {
        return tilemap.GetTile(p) != null;
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        int mask = 0;
        if (Filled(tilemap, position)) mask |= 1;
        if (Filled(tilemap, position + new Vector3Int(1, 0, 0))) mask |= 2;
        if (Filled(tilemap, position + new Vector3Int(0, -1, 0))) mask |= 4;
        if (Filled(tilemap, position + new Vector3Int(1, -1, 0))) mask |= 8;

        tileData.sprite = (Sprites != null && mask < Sprites.Length) ? Sprites[mask] : null;
        tileData.color = Color.white;
        tileData.colliderType = Tile.ColliderType.None;
        tileData.transform = Matrix4x4.Translate(new Vector3(0.5f, -0.5f, 0f));
        tileData.flags = TileFlags.LockTransform | TileFlags.LockColor;
    }

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                tilemap.RefreshTile(position + new Vector3Int(dx, dy, 0));
    }
}
