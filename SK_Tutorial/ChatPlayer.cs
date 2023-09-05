using Godot;

// The class for players that will interact with semantic kernel.
[GlobalClass]
public partial class ChatPlayer : ChatEntity
{
	// Properties

	// The current control hint
	private string _controlHint;

	// Signals for connecting to the UI
	[Signal]
	public delegate void ControlHintUpdatedEventHandler(string controlHint);
	[Signal]
	public delegate void ChatEntityAddedEventHandler(ChatEntity chatEntity);
	[Signal]
	public delegate void ChatEntityRemovedEventHandler(ChatEntity chatEntity);
	[Signal]
	public delegate void ConvoStartedEventHandler();
	[Signal]
	public delegate void ConvoEndedEventHandler();
	[Signal]
	public delegate void MsgAddedEventHandler(ChatEntity sender, string msg);



	// Godot Methods

	// Called every tick
	public override void _Process(double delta)
	{
		// Get the current control hint
		string newControlHint = "Use the arrow keys to move";
		if (InConvo())
		{
			newControlHint = "Press ESC to end the conversation";
		}
		else
		{
			ChatEntity nearestChatEntity = NearestChatEntity();
			if (nearestChatEntity != null)
				newControlHint = "Press SHIFT to talk with " + nearestChatEntity.ChatName;
		}
		// If the control hint has changed, update it and emit a signal
		if (newControlHint != _controlHint)
		{
			_controlHint = newControlHint;
			EmitSignal(SignalName.ControlHintUpdated, _controlHint);
		}
	}

	// Handle user input
	public override void _Input(InputEvent @event)
	{
		// In a conversation, we can end the conversation
		if (InConvo())
		{
			if (@event.IsActionPressed("end_convo"))
				EndConvo(_inConvoWith);
		}
		// Not in conversation, we can start a conversation
		else
		{
			if (@event.IsActionPressed("start_convo"))
			{
				ChatEntity nearestChatEntity = NearestChatEntity();
				if (nearestChatEntity != null)
					StartConvo(nearestChatEntity);
			}
		}
	}



	// Nearby ChatEntity Methods

	// Called when another ChatEntity enters the collision area of this ChatPlayer
	protected override void OnChatEntityEntered(ChatEntity enteringChatEntity)
	{
		base.OnChatEntityEntered(enteringChatEntity);

		// Emit a signal that a new ChatEntity was added (for UI)
		EmitSignal(SignalName.ChatEntityAdded, enteringChatEntity);
	}

	// Called when another ChatEntity exits the collision area of this ChatEntity
	protected override void OnChatEntityExited(ChatEntity exitingChatEntity)
	{
		base.OnChatEntityExited(exitingChatEntity);

		// Emit a signal that a new ChatEntity was removed (for UI)
		EmitSignal(SignalName.ChatEntityRemoved, exitingChatEntity);
	}



	// Messaging/Conversation Methods

	// Attempts to start a conversation with another ChatEntity.
	// Will fail and return false if otherChatEntity is already in a conversation.
	public override bool StartConvo(ChatEntity otherChatEntity)
	{
		// Do the basic stuff
		if (!base.StartConvo(otherChatEntity))
			return false;

		// If otherChatEntity is a ChatAI, notify it of the start of the conversation
		if (otherChatEntity is ChatAI otherChatAI)
			otherChatAI.Notify(ChatName + " has started a conversation with you.");

		// Emit a signal that a conversation was started (for UI)
		EmitSignal(SignalName.ConvoStarted);

		// Return success
		return true;
	}

	// Attempts to end a conversation with another ChatEntity
	// Will fail and return false if otherChatEntity is not in a conversation with this ChatEntity
	public override bool EndConvo(ChatEntity otherChatEntity)
	{
		// Do the basic stuff
		if (!base.EndConvo(otherChatEntity))
			return false;

		// If otherChatEntity is a ChatAI, notify it of the end of the conversation
		if (otherChatEntity is ChatAI otherChatAI)
			otherChatAI.Notify(ChatName + " has ended their conversation with you.");

		// Emit a signal that a conversation was ended (for UI)
		EmitSignal(SignalName.ConvoEnded);

		// Return success
		return true;
	}

	// A useful shorthand for sending a message
	// Called when the MsgSent signal is emitted by UI
	public override void SendMsg(string msg)
	{
		// Send the message to ChatEntity _inConvoWith
		base.SendMsg(msg);

		// Emit a signal that there is a new message (for UI)
		EmitSignal(SignalName.MsgAdded, this, msg);
	}

	// Called when ChatEntity _inConvoWith emits a MsgSent signal
	public override void ReceiveMsg(string msg)
	{
		// Emit a signal that there is a new message (for UI)
		EmitSignal(SignalName.MsgAdded, _inConvoWith, msg);
	}
}
