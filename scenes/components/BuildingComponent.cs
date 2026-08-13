using Godot;
using System;

namespace Game.Components;

public partial class BuildingComponent : Node2D
{
	[Export] public int BuildableRadius { get; private set; }
	
	public override void _Ready()
	{
		AddToGroup(nameof(BuildingComponent));
	}

	public Vector2I GetGridCellPosition()
	{
		//Vector2 mousePosition = this.GetGlobalMousePosition();
		Vector2 gridPosition = GlobalPosition / GlobalConstants.GridCellSize;
		gridPosition = gridPosition.Floor(); // Snap to nearest cell
		GD.Print(gridPosition);
		return new Vector2I((int)gridPosition.X, (int)gridPosition.Y) ;
	}
}
