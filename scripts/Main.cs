using System.Collections.Generic;
using Godot;

namespace Game;

/// <summary>
/// Main game controller handling building placement, cursor tracking,
/// and grid highlighting.
/// </summary>
public partial class Main : Node
{
    // Constants for grid-based positioning
    private const int GridCellSize = 64;          // Size of each grid cell in pixels
    private const int HighlightRadius = 3;        // Radius of the highlight area around the cursor

    // Scene references
    private Sprite2D _cursor;                     // Visual cursor that follows the mouse
    private PackedScene _buildingScene;           // Preloaded building scene to instantiate
    private Button _placeBuildingButton;          // UI button to toggle placement mode
    private TileMapLayer _highlightTileMapLayer;  // Layer used to show buildable area highlight
    
    

    // State
    private Vector2? _hoverGridCellPosition;          // Last cached grid position for highlight updates (null if hidden)
    private HashSet<Vector2> _occupiedCells = new(); // For checking whether cells are occupied with buildings or not (avoiding building stacks)
    
    public override void _Ready()
    {
        // Load resources and get child nodes
        _buildingScene = GD.Load<PackedScene>("res://scenes/buildings/Building.tscn");
        _cursor = GetNode<Sprite2D>("Cursor");
        _placeBuildingButton = GetNode<Button>("PlaceBuildingButton");
        _highlightTileMapLayer = GetNode<TileMapLayer>("HighlightTileMapLayer");

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
        // Guard: cursor must exist, be visible, and event must be a left‑click
        if (_cursor == null || !_cursor.Visible || !evt.IsActionPressed("left_click") || !_hoverGridCellPosition.HasValue || _occupiedCells.Contains(_hoverGridCellPosition.Value))
        {
            return;
        }

        PlaceBuildingAtHoveredCellPosition();
        _cursor.Visible = false; // Hide cursor after placing
    }
    
    public override void _Process(double delta)
    {
        if (_cursor == null)
        {
            return;
        }

        // Get the current grid cell under the mouse
        Vector2 currentGridPosition = GetMouseGridCellPosition();

        // Move the cursor sprite to that cell
        UpdateCursorPosition(currentGridPosition);

        // Update the highlight if the cursor visibility or hover position changed
        UpdateHighlightIfNeeded(currentGridPosition);
    }

    /// <summary>
    /// Converts the global mouse position to a grid cell coordinate.
    /// </summary>
    /// <returns>Grid cell position as a Vector2 (x and y are integers).</returns>
    private Vector2 GetMouseGridCellPosition()
    {
        Vector2 mousePosition = _highlightTileMapLayer.GetGlobalMousePosition();
        Vector2 gridPosition = mousePosition / GridCellSize;
        gridPosition = gridPosition.Floor(); // Snap to nearest cell
        return gridPosition;
    }

    /// <summary>
    /// Updates the cursor's global position to the center of the given grid cell.
    /// </summary>
    private void UpdateCursorPosition(Vector2 gridPosition)
    {
        _cursor.GlobalPosition = gridPosition * GridCellSize;
    }

    /// <summary>
    /// Updates the highlight tilemap layer only when necessary (cursor visible and grid changed,
    /// or cursor hidden and highlight was previously shown).
    /// </summary>
    private void UpdateHighlightIfNeeded(Vector2 gridPosition)
    {
        if (_highlightTileMapLayer == null)
        {
            return; // No highlight layer available
        }

        // Case 1: cursor is visible and the hovered cell has changed (or was null)
        if (_cursor.Visible && (!_hoverGridCellPosition.HasValue || _hoverGridCellPosition.Value != gridPosition))
        {
            _hoverGridCellPosition = gridPosition;
            UpdateHighlightTileMapLayer();
        }
        // Case 2: cursor is hidden, but we still have a stale highlight – clear it
        else if (!_cursor.Visible && _hoverGridCellPosition.HasValue)
        {
            _hoverGridCellPosition = null;
            UpdateHighlightTileMapLayer();
        }
    }

    /// <summary>
    /// Instantiates a building at the current mouse grid position and clears the highlight.
    /// </summary>
    private void PlaceBuildingAtHoveredCellPosition()
    {
        // Guard: building scene must be loaded, cursor exists, and it must be visible
        if (!_hoverGridCellPosition.HasValue ||_buildingScene == null || _cursor == null || !_cursor.Visible)
        {
            return;
        }

        // Spawn the building
        Node2D building = _buildingScene.Instantiate<Node2D>();
        AddChild(building);
        building.GlobalPosition = _hoverGridCellPosition.Value * GridCellSize;
        _occupiedCells.Add(_hoverGridCellPosition.Value);

        // Clear the highlight after placement
        _hoverGridCellPosition = null;
        UpdateHighlightTileMapLayer();
    }

    /// <summary>
    /// Redraws the highlight tilemap layer based on the current hovered grid position.
    /// Highlights a square area of size (2*HighlightRadius+1) x (2*HighlightRadius+1).
    /// </summary>
    private void UpdateHighlightTileMapLayer()
    {
        if (_highlightTileMapLayer == null)
        {
            return;
        }

        // Clear previous highlight
        _highlightTileMapLayer.Clear();

        // If no hover position, we're done (nothing to highlight)
        if (!_hoverGridCellPosition.HasValue)
        {
            return;
        }

        Vector2 center = _hoverGridCellPosition.Value;

        // Fill a square area around the center cell
        for (int x = (int)center.X - HighlightRadius; x <= (int)center.X + HighlightRadius; x++)
        {
            for (int y = (int)center.Y - HighlightRadius; y <= (int)center.Y + HighlightRadius; y++)
            {
                // Set cell with source tile ID 0 at (0,0) in the tile atlas
                _highlightTileMapLayer.SetCell(new Vector2I(x, y), 0, Vector2I.Zero);
            }
        }
    }

    /// <summary>
    /// Event handler for the "Place Building" button press.
    /// Toggles the cursor visibility.
    /// </summary>
    private void OnButtonPressed()
    {
        if (_cursor == null)
        {
            return;
        }

        // Toggle: if visible, hide; if hidden, show
        _cursor.Visible = !_cursor.Visible;
    }
}