using Game.Manager;
using Godot;

namespace Game;

public partial class Main : Node
{
    // References
    private GridManager _gridManager;
    
    
    // Constants for grid-based positioning
    private const int HighlightRadius = 3;        

    // Scene references
    private Sprite2D _cursor;                     // Visual cursor that follows the mouse
    private PackedScene _buildingScene;           // Preloaded building scene to instantiate
    private Button _placeBuildingButton;          // UI button to toggle placement mode
    
    // State
    private Vector2? _hoveredGridCellPosition;        // Last cached grid position for highlight updates (null if hidden)
    
    public override void _Ready()
    {
        // Load resources and get child nodes
        _buildingScene = GD.Load<PackedScene>("res://scenes/buildings/Building.tscn");
        _cursor = GetNode<Sprite2D>("Cursor");
        _placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
        //_highlightTileMapLayer = GetNode<TileMapLayer>("HighlightTileMapLayer");
        _gridManager = GetNode<GridManager>("GridManager");

        // Initially hide the cursor
        if (_cursor != null)
        {
            _cursor.Visible = false;
        }

        // Connect button pressed event if the button exists
        if (_placeBuildingButton != null)
        {
            _placeBuildingButton.Pressed += OnButtonPressed;
        }
    }
    
    public override void _UnhandledInput(InputEvent evt)
    {
        //GD.Print(_gridManager.IsTilePositionValid(_hoveredGridCellPosition.Value));
        // Guard: cursor must exist, be visible, and event must be a left‑click
        if (_cursor == null
            || !_cursor.Visible
            || !evt.IsActionPressed("left_click")
            || !_hoveredGridCellPosition.HasValue
            || !_gridManager.IsTilePositionValid(_hoveredGridCellPosition.Value)
           )
        {
            return;
        }
        //GD.Print("We are trying to place a building");
        PlaceBuildingAtHoveredCellPosition();
        _cursor.Visible = false; // Hide cursor after placing
    }
    
    public override void _Process(double delta)
    {
        if (_cursor == null || !_cursor.Visible)
        {
            return;
        }
        
        // Get the current grid cell under the mouse
        Vector2 currentGridPosition = _gridManager.GetMouseGridCellPosition();
        
        // Move the cursor sprite to that cell
        UpdateCursorPosition(currentGridPosition);
        
        _gridManager.HighlightValidTilesInRadius(currentGridPosition, HighlightRadius);
    }
    
   

   
    private void UpdateCursorPosition(Vector2 gridPosition)
    {
        _cursor.GlobalPosition = gridPosition * GlobalConstants.GridCellSize;
        _hoveredGridCellPosition = gridPosition;
    }

    
    private void PlaceBuildingAtHoveredCellPosition()
    {
        // Guard: building scene must be loaded, cursor exists, and it must be visible
        if (!_hoveredGridCellPosition.HasValue || _buildingScene == null || _cursor == null || !_cursor.Visible)
        {
            return;
        }

        // Spawn the building
        Node2D building = _buildingScene.Instantiate<Node2D>();
        AddChild(building);
        building.GlobalPosition = _hoveredGridCellPosition.Value * GlobalConstants.GridCellSize;
        _gridManager.MarkTileAsOccupied(_hoveredGridCellPosition.Value);

        // Clear the highlight after placement
        _hoveredGridCellPosition = null;
        _gridManager.ClearHighlightedTiles();

    }
    
    private void OnButtonPressed()
    {
        if (_cursor == null)
        {
            return;
        }
        _gridManager.ClearHighlightedTiles();
        _cursor.Visible = !_cursor.Visible;
    }
}