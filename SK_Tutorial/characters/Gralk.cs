using Godot;

public partial class Gralk : CharacterBody2D
{
	// Properties

	// The AI node for Gralk
	public TrollAI MyTrollAI;

	// Where Gralk should move to allow player to pass
	private Vector2 _allowToPassPosition;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Set up MyTrollAI
		MyTrollAI = GetNode<TrollAI>("TrollAI");
		if (MyTrollAI == null)
			GD.PrintErr(Name + " cannot find TrollAI");
		else
			MyTrollAI.RiddleAnswered += AllowToPass;
		
		// Designate where Gralk should move to allow player to pass
		_allowToPassPosition = Position;
		_allowToPassPosition.X -= 300;
	}

	// Moves Gralk to allow the player to pass
	public void AllowToPass()
	{
		Tween moveTween = CreateTween();
		moveTween.TweenProperty(this, "position", _allowToPassPosition, 2);
	}
}

