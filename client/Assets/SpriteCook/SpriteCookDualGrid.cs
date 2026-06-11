using UnityEngine;
using UnityEngine.Tilemaps;

// SpriteCook dual-grid manager (self-contained, needs only the built-in 2D Tilemap package).
//
// Add this to an empty GameObject and drag a <name>_DualGridTile asset into "Tile Set".
// It creates two child tilemaps: a hidden "Data" grid you paint on, and a "Display" grid
// offset by half a cell that shows the seamless terrain. Painting updates the display live.
[ExecuteAlways]
[RequireComponent(typeof(Grid))]
public class SpriteCookDualGrid : MonoBehaviour
{
    [Tooltip("An exported <name>_DualGridTile asset; supplies the 16 corner sprites.")]
    public SpriteCookDualGridTile tileSet;

    Tilemap dataTilemap;
    Tilemap displayTilemap;
    Tile[] displayTiles;

    void OnEnable()
    {
        EnsureChildren();
        BuildDisplayTiles();
        Tilemap.tilemapTileChanged += OnTilemapChanged;
        RebuildAll();
    }

    void OnDisable()
    {
        Tilemap.tilemapTileChanged -= OnTilemapChanged;
    }

    //void OnValidate()
    //{
    //    if (!isActiveAndEnabled) return;
    //    BuildDisplayTiles();
    //    RebuildAll();
    //}

    void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        BuildDisplayTiles();

        // 🌟 유니티 에디터 타이밍 충돌 에러(SendMessage) 방지 처리
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) RebuildAll();
        };
#endif
    }

    void EnsureChildren()
    {
        if (dataTilemap == null)
        {
            var go = FindOrCreateChild("Data");
            dataTilemap = GetOrAdd<Tilemap>(go);
            GetOrAdd<TilemapRenderer>(go).enabled = false; // the data grid is logical only
        }
        if (displayTilemap == null)
        {
            var go = FindOrCreateChild("Display");
            displayTilemap = GetOrAdd<Tilemap>(go);
            GetOrAdd<TilemapRenderer>(go);
            go.transform.localPosition = new Vector3(-0.5f, -0.5f, 0f); // half-cell offset
        }
    }

    GameObject FindOrCreateChild(string childName)
    {
        var existing = transform.Find(childName);
        if (existing != null) return existing.gameObject;
        var go = new GameObject(childName);
        go.transform.SetParent(transform, false);
        return go;
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        return c != null ? c : go.AddComponent<T>();
    }

    void BuildDisplayTiles()
    {
        displayTiles = new Tile[16];
        if (tileSet == null || tileSet.Sprites == null) return;
        for (int m = 0; m < 16; m++)
        {
            var sprite = (m < tileSet.Sprites.Length) ? tileSet.Sprites[m] : null;
            if (sprite == null) continue;
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.None;
            displayTiles[m] = tile;
        }
    }

    void OnTilemapChanged(Tilemap tilemap, Tilemap.SyncTile[] changed)
    {
        if (tilemap != dataTilemap || displayTilemap == null) return;
        foreach (var sync in changed)
        {
            var p = sync.position;
            RefreshDisplay(p.x, p.y);
            RefreshDisplay(p.x + 1, p.y);
            RefreshDisplay(p.x, p.y + 1);
            RefreshDisplay(p.x + 1, p.y + 1);
        }
    }

    void RebuildAll()
    {
        if (dataTilemap == null || displayTilemap == null || displayTiles == null) return;
        displayTilemap.ClearAllTiles();
        var b = dataTilemap.cellBounds;
        for (int x = b.xMin; x <= b.xMax + 1; x++)
            for (int y = b.yMin; y <= b.yMax + 1; y++)
                RefreshDisplay(x, y);
    }

    bool Filled(int x, int y)
    {
        return dataTilemap.HasTile(new Vector3Int(x, y, 0));
    }

    void RefreshDisplay(int x, int y)
    {
        if (displayTiles == null) return;
        int mask = 0;
        if (Filled(x - 1, y)) mask |= 1;     // NW
        if (Filled(x, y)) mask |= 2;          // NE
        if (Filled(x - 1, y - 1)) mask |= 4;  // SW
        if (Filled(x, y - 1)) mask |= 8;      // SE
        displayTilemap.SetTile(new Vector3Int(x, y, 0), mask == 0 ? null : displayTiles[mask]);
    }
}
