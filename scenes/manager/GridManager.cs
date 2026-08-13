using System.Collections.Generic;
using System.Linq;
using Game.Components;
using Godot;

namespace Game.Manager;

public partial class GridManager : Node
{

    [Export] 
    private TileMapLayer _highlightTileMapLayer;
    
    [Export]
    private TileMapLayer _baseTerrainTileMapLayer;
    
    private HashSet<Vector2> OccupiedCells { get; } = new();
    
    
    
    public bool IsTilePositionValid(Vector2I tilePosition)
    {
        var tilePositionInt = new Vector2I((int)tilePosition.X, (int)tilePosition.Y);
        var customData = _baseTerrainTileMapLayer.GetCellTileData(tilePositionInt);
        
        if(customData == null) return false;
        if(!(bool)customData.GetCustomData("buildable")) return false;
        
        return !OccupiedCells.Contains(tilePosition);
    }
    
    public void MarkTileAsOccupied(Vector2 tilePosition)
    {
        OccupiedCells.Add(tilePosition);
    }

    public void HighlightBuildableTiles()
    {
        ClearHighlightedTiles();

        // Use OfType to safely filter only BuildingComponent nodes
        var buildingComponents = GetTree()
            .GetNodesInGroup(nameof(BuildingComponent))
            .OfType<BuildingComponent>()
            .ToList();  // optional: materialize to avoid multiple enumeration

        GD.Print($"Found {buildingComponents.Count} building components to highlight.");

        foreach (var buildingComponent in buildingComponents)
        {
            // Optional: you might want to null-check here, but OfType already excludes nulls
            HighlightValidTilesInRadius(
                buildingComponent.GetGridCellPosition(),
                buildingComponent.BuildableRadius
            );
        }
    }
    
    public Vector2I GetMouseGridCellPosition()
    {
        Vector2 mousePosition = _highlightTileMapLayer.GetGlobalMousePosition();
        Vector2 gridPosition = mousePosition / GlobalConstants.GridCellSize;
        gridPosition = gridPosition.Floor(); // Snap to nearest cell
        return new Vector2I((int)gridPosition.X, (int)gridPosition.Y) ;
    }

    public void ClearHighlightedTiles()
    {
        _highlightTileMapLayer.Clear();
    }
    
    private void HighlightValidTilesInRadius(Vector2I rootCell, int radius)
    {

        // Fill a square area around the center cell
        for (var x = rootCell.X - radius; x <= rootCell.X + radius; x++)
        {
            for (var y = rootCell.Y - radius; y <= rootCell.Y + radius; y++)
            {
                // Set cell with source tile ID 0 at (0,0) in the tile atlas
                // Todo: Check the logic whether the placing building is valid or not
                if (!IsTilePositionValid(new Vector2I(x, y))) continue;
                _highlightTileMapLayer.SetCell(new Vector2I(x, y), 0, Vector2I.Zero);
            }
        }
        
    }
}
