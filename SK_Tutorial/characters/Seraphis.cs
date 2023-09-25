using Godot;

public partial class Seraphis : CharacterBody2D
{
	// Properties

	// How fast the player will move (pixels/sec)
	[Export]
	public int Speed = 300;

	// The ChatPlayer node
	public ChatPlayer MyChatPlayer;



	// Godot Methods

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Set up MyChatPlayer
		MyChatPlayer = FindChild("ChatPlayer") as ChatPlayer;
		if (MyChatPlayer == null)
			GD.PrintErr(Name + " cannot find ChatPlayer");
	}

	// Called once per physics tick
	public override void _PhysicsProcess(double delta)
	{
		// Not in a conversation, can move
		if (!MyChatPlayer.InConvo())
		{
			// Get input vector
			var velocity = Input.GetVector("move_left", "move_right", "move_up", "move_down");

			// Set correct magnitude
			if (velocity.Length() > 0)
				velocity = velocity.Normalized() * Speed;

			// Set Velocity property of this CharacterBody2D
			Velocity = velocity;
		}
		// In a conversation, cannot move
		else
		{
			Velocity = Vector2.Zero;
		}

		// Call Godot's built-in function for physics-based movement
		MoveAndSlide();
	}
}

