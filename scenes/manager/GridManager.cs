using System.Collections.Generic;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{

    [Export] 
    private TileMapLayer _highlightTileMapLayer;
    
    [Export]
    private TileMapLayer _baseTerrainTileMapLayer;
    
    private HashSet<Vector2> OccupiedCells { get; } = new();
    
    public override void _Ready()
    {
        
    }

    public void HighlightValidTilesInRadius(Vector2 rootCell, int radius)
    {
        // Clear previous highlight
        ClearHighlightedTiles();

        // Fill a square area around the center cell
        for (int x = (int)rootCell.X - radius; x <= (int)rootCell.X + radius; x++)
        {
            for (int y = (int)rootCell.Y - radius; y <= (int)rootCell.Y + radius; y++)
            {
                // Set cell with source tile ID 0 at (0,0) in the tile atlas
                // Todo: Check the logic whether the placing building is valid or not
                if (!IsTilePositionValid(new Vector2(x, y))) continue;
                _highlightTileMapLayer.SetCell(new Vector2I(x, y), 0, Vector2I.Zero);
            }
        }
        
    }

    public bool IsTilePositionValid(Vector2 tilePosition)
    {
        return !OccupiedCells.Contains(tilePosition);
    }

    public void MarkTileAsOccupied(Vector2 tilePosition)
    {
        OccupiedCells.Add(tilePosition);
    }
    
    public Vector2 GetMouseGridCellPosition()
    {
        Vector2 mousePosition = _highlightTileMapLayer.GetGlobalMousePosition();
        Vector2 gridPosition = mousePosition / GlobalConstants.GridCellSize;
        gridPosition = gridPosition.Floor(); // Snap to nearest cell
        return gridPosition;
    }

    public void ClearHighlightedTiles()
    {
        _highlightTileMapLayer.Clear();
    }
}
