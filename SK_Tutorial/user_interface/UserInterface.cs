using Godot;
using System;

public partial class UserInterface : Control
{
	// Properties

	// A reference to the ChatPlayer node (will be set by dependency injection via Main.cs)
	public ChatPlayer MyChatPlayer;

	// Various nodes that will need to be updated during gameplay
	private Label _controlInfo;
	private VBoxContainer _entityBox;
	private ScrollContainer _msgScroll;
	private VBoxContainer _msgBox;
	private TextEdit _newMsgEdit;
	private Button _newMsgSend;

	// Scenes that will need to be instanced during gameplay
	private PackedScene _dynamicLabelScene = GD.Load<PackedScene>("res://user_interface/dynamic_label.tscn");

	// Flag for when a message has been added to the message box and so we need to scroll to end
	private bool _justAddedNewMsg = false;



	// Godot Methods

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get all the various child nodes
		_controlInfo = GetNodeOrNull<Label>("ControlGroup/ControlInfo");
		_entityBox = GetNodeOrNull<VBoxContainer>("EntityGroup/EntityScroll/EntityBox");
		_msgScroll = GetNodeOrNull<ScrollContainer>("MsgGroup/MsgScroll");
		_msgBox = GetNodeOrNull<VBoxContainer>("MsgGroup/MsgScroll/MsgBox");
		_newMsgEdit = GetNodeOrNull<TextEdit>("MsgGroup/NewMsgBox/NewMsgEdit");
		_newMsgSend = GetNodeOrNull<Button>("MsgGroup/NewMsgBox/NewMsgSend");

		// Connect to the send message button (and propogate the signal)
		if (_newMsgSend != null)
			_newMsgSend.Pressed += OnNewMsgSendPressed;
		
		// Connect to the signals of the ChatPlayer
		CallDeferred("ConnectPlayerSignals");
	}

	// Connects to the various signals of the currently registered ChatPlayer
	private void ConnectPlayerSignals()
	{
		MyChatPlayer.ControlHintUpdated += SetControlHint;
		MyChatPlayer.ChatEntityAdded += AddEntity;
		MyChatPlayer.ChatEntityRemoved += RemoveEntity;
		MyChatPlayer.ConvoStarted += EnableNewMsg;
		MyChatPlayer.ConvoEnded += DisableNewMsg;
		MyChatPlayer.MsgAdded += AddMsg;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Check if a new message has been added to the message box and scroll if needed
		if (_justAddedNewMsg)
		{
			_msgScroll.ScrollVertical = Mathf.RoundToInt(_msgScroll.GetVScrollBar().MaxValue);
			_justAddedNewMsg = false;
		}
	}



	// Display Info Methods

	// Set the text of the control hint label
	public void SetControlHint(string controlHint)
	{
		_controlInfo.Text = controlHint;
	}

	// Add a ChatEntity to the entity box
	public void AddEntity(ChatEntity newEntity)
	{
		// Add the new entity to the box
		Label newEntityLabel = _dynamicLabelScene.Instantiate<Label>();
		newEntityLabel.Text = newEntity.ChatName;
		_entityBox.AddChild(newEntityLabel);
	}

	// Remove a ChatEntity from the entity box
	public void RemoveEntity(ChatEntity oldEntity)
	{
		string entityString = oldEntity.ChatName;

		// Look through all the entities in the box
		foreach (Node child in _entityBox.GetChildren())
		{
			// If we find the entity, remove it and exit
			if (child is Label entityLabel && entityLabel.Text == entityString)
			{
				entityLabel.QueueFree();
				break;
			}
		}
	}



	// Messaging Methods

	// Called when the send button is pressed
	public void OnNewMsgSendPressed()
	{
		MyChatPlayer.SendMsg(_newMsgEdit.Text);
		_newMsgEdit.Text = "";
	}

	// Enable the controls for typing and sending a new message
	public void EnableNewMsg()
	{
		// Enable controls
		_newMsgEdit.Editable = true;
		_newMsgSend.Disabled = false;

		// Put focus on the text editor
		_newMsgEdit.GrabFocus();
	}

	// Disable the controls for typing and sending a new message
	public void DisableNewMsg()
	{
		// Disable controls
		_newMsgEdit.Editable = false;
		_newMsgSend.Disabled = true;

		// Release focus
		_newMsgEdit.ReleaseFocus();
		_newMsgSend.ReleaseFocus();
	}

	// Add a ChatMsg to the message box
	public void AddMsg(ChatEntity sender, string msg)
	{
		// Add the new message to the box
		Label newMsgLabel = _dynamicLabelScene.Instantiate<Label>();
		newMsgLabel.Text = sender.ChatName + ": " + msg;
		_msgBox.AddChild(newMsgLabel);

		// Make sure we will scroll to the new message
		_justAddedNewMsg = true;
	}
}
