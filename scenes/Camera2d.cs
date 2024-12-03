using Godot;
using System;

public partial class Camera2d : Camera2D
{
    [Export]
    private float ZoomLevel = 0.5f;
    
    [Export]
    private float SmoothingSpeed = 10.0f;
    
    [Export]
    private NodePath CharacterBody2D;  
    
    private Node2D _target;

    public override void _Ready()
    {
        // Set initial zoom
        Zoom = new Vector2(1.0f / ZoomLevel, 1.0f / ZoomLevel);
        
        if (CharacterBody2D != null)
        {
            _target = GetNode<Node2D>(CharacterBody2D);
        }
        
        PositionSmoothingEnabled = true;
        PositionSmoothingSpeed = SmoothingSpeed;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target != null)
        {

            GlobalPosition = _target.GlobalPosition;
        }
    }
}