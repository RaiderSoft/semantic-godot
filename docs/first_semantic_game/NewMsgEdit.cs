using Godot;
using System;

public partial class NewMsgEdit : TextEdit
{
	// Flag indicating a message has just been sent
	private bool _justSentMsg = false;

	// Some code so that the enter key will send the message (and do nothing else)
	public override void _Input(InputEvent @event)
	{
		// Only do something if we have focus and the input is the enter key
		if (HasFocus() && @event is InputEventKey @eventKey && @eventKey.AsTextKeyLabel() == "Enter")
		{
			// Pressing the enter key
			if (@eventKey.IsPressed())
			{
				Button newMsgSend = GetNode<Button>("../NewMsgSend");
				if (newMsgSend != null)
				{
					newMsgSend.EmitSignal("pressed");
					Editable = false;
					_justSentMsg = true;
				}
			}
			// Releasing the enter key
			else if (@eventKey.IsReleased() && _justSentMsg)
			{
				Editable = true;
				_justSentMsg = false;
			}
		}
	}
}
