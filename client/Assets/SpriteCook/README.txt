SpriteCook tileset export (Unity)

The export bundles the autotile that matches this tileset's type. No extra packages
required (built-in 2D Tilemap only).

If it includes a *_BlobTile.asset (17-piece tilesets)
1. Add a Grid + Tilemap to your scene.
2. Open the Tile Palette, select the Tilemap, and paint with <name>_BlobTile.
   Each cell picks its art from its 8 neighbours, standard single-grid autotiling.

If it includes a *_DualGridTile.asset (15-piece tilesets)
1. Create an empty GameObject and add the "Sprite Cook Dual Grid" component.
   It adds a Grid + a hidden "Data" tilemap and a half-offset "Display" tilemap.
2. Drag the <name>_DualGridTile asset into the component's "Tile Set" field.
3. Select the "Data" tilemap in the Tile Palette and paint (any tile works); the
   Display grid fills in the seamless terrain live.

Tile size, padding and spacing are baked into the texture import settings.
