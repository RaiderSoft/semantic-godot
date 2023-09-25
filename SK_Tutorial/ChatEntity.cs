using Godot;
using System.Collections.Generic;

// The base class for nodes that will interact with semantic kernel.
// It handles seeing nearby ChatEntities and some basic messaging.
[GlobalClass]
public partial class ChatEntity : Area2D
{
	// Properties

	// The name for this ChatEntity instance (can be read by ChatEntity instances)
	[Export]
	public string ChatName = "";

	// The description for this ChatEntity instance (can be read by ChatEntity instances)
	[Export(PropertyHint.MultilineText)]
	public string ChatDescr = "";

	// List of ChatEntities currently in range
	protected List<ChatEntity> _nearbyChatEntities = new List<ChatEntity>();

	// Holds the value of the other ChatEntity that this ChatEntity is currently in conversation with
	protected ChatEntity _inConvoWith = null;

	// A Godot signal for sending a message
	[Signal]
	public delegate void MsgSentEventHandler(string msg);



	// Nearby ChatEntity Methods

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Connect the signals for when another ChatEntity enters or exits the area
		AreaEntered += OnAreaEntered;
		AreaExited += OnAreaExited;
	}

	// Called when another Area2D enters the collision area of this ChatEntity
	private void OnAreaEntered(Area2D enteringArea2D)
	{
		if (enteringArea2D is ChatEntity enteringChatEntity)
			OnChatEntityEntered(enteringChatEntity);
	}

	// Called when another Area2D exits the collision area of this ChatEntity
	private void OnAreaExited(Area2D exitingArea2D)
	{
		if (exitingArea2D is ChatEntity exitingChatEntity)
			OnChatEntityExited(exitingChatEntity);
	}

	// Called when another ChatEntity enters the collision area of this ChatEntity
	protected virtual void OnChatEntityEntered(ChatEntity enteringChatEntity)
	{
		_nearbyChatEntities.Add(enteringChatEntity);
	}

	// Called when another ChatEntity enters the collision area of this ChatEntity
	protected virtual void OnChatEntityExited(ChatEntity exitingChatEntity)
	{
		_nearbyChatEntities.Remove(exitingChatEntity);
	}

	// Returns the nearest ChatEntity in _nearbyChatEntities
	public ChatEntity NearestChatEntity()
	{
		// No nearby entities, return null
		if (_nearbyChatEntities.Count == 0)
		{
			return null;
		}
		// Otherwise, search through and find the nearest ChatEntity
		else
		{
			ChatEntity nearestChatEntity = _nearbyChatEntities[0];

			foreach (ChatEntity currentChatEntity in _nearbyChatEntities)
			{
				float nearestDistance = GlobalPosition.DistanceTo(nearestChatEntity.GlobalPosition);
				float currentDistance = GlobalPosition.DistanceTo(currentChatEntity.GlobalPosition);

				if (currentDistance < nearestDistance)
					nearestChatEntity = currentChatEntity;
			}

			return nearestChatEntity;
		}
	}



	// Messaging/Conversation Methods

	// Returns true if currently in a conversation, false otherwise
	public virtual bool InConvo()
	{
		return _inConvoWith != null;
	}

	// Attempts to start a conversation with another ChatEntity.
	// Will fail and return false if otherChatEntity is already in a conversation.
	public virtual bool StartConvo(ChatEntity otherChatEntity)
	{
		// Check to make sure neither ChatEntity is already in a conversation
		if (_inConvoWith != null || otherChatEntity._inConvoWith != null)
			return false;

		// Put both ChatEntities in conversation mode
		_inConvoWith = otherChatEntity;
		otherChatEntity._inConvoWith = this;

		// Connect up the MsgSent signals
		MsgSent += otherChatEntity.ReceiveMsg;
		otherChatEntity.MsgSent += ReceiveMsg;

		// Return success
		return true;
	}

	// Attempts to end a conversation with another ChatEntity
	// Will fail and return false if otherChatEntity is not in a conversation with this ChatEntity
	public virtual bool EndConvo(ChatEntity otherChatEntity)
	{
		// Check to make sure both ChatEntities are in conversation with each other
		if (_inConvoWith != otherChatEntity || otherChatEntity._inConvoWith != this)
			return false;

		// Take both ChatEntities out of conversation mode
		_inConvoWith = null;
		otherChatEntity._inConvoWith = null;

		// Disconnect the MsgSent signals
		MsgSent -= otherChatEntity.ReceiveMsg;
		otherChatEntity.MsgSent -= ReceiveMsg;

		// Return success
		return true;
	}

	// A useful shorthand for sending a message
	public virtual void SendMsg(string msg)
	{
		CallDeferred("emit_signal", SignalName.MsgSent, msg);
	}

	// Called when ChatEntity _inConvoWith emits a MsgSent signal
	public virtual void ReceiveMsg(string msg)
	{
		// Definition should be filled in by inheriting class
	}
}
