using Godot;
using System;

public partial class Main : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Connect the UI up to the first ChatPlayer found
		ChatPlayer chatPlayer = GetNode("SubViewportContainer/SubViewport").FindChild("ChatPlayer") as ChatPlayer;
		UserInterface userInterface = GetNode<UserInterface>("MarginContainer/UserInterface");
		userInterface.MyChatPlayer = chatPlayer;
	}
}
