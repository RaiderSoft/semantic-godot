using Godot;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.TemplateEngine;
using Microsoft.Extensions.Configuration;

using System;
using System.Threading.Tasks;
using System.IO;

// The class for NPCs that will use semantic kernel.

[GlobalClass]
public partial class ChatAI : ChatEntity
{
	protected IChatCompletion _chatGPT;
	protected OpenAIChatHistory _chat;

	protected readonly PromptTemplateEngine _promptRenderer;
	protected readonly IKernel _kernel;

	public ChatAI()
	{
		// Set up chatGPT and chat
		string apiKey = GetApiKey();
		_promptRenderer = new PromptTemplateEngine();
		_kernel = new KernelBuilder()
						.WithOpenAIChatCompletionService("gpt-4", apiKey, "")
						.Build();
		_chatGPT = _kernel.GetService<IChatCompletion>();

	}

	public override void _Ready()
	{
		var npc_template = File.ReadAllText("personalities/NPC.txt");
		var context = _kernel.CreateNewContext();
		context["personality"] = ChatDescr;
		string npc_personality = _promptRenderer.RenderAsync(npc_template, context).GetAwaiter().GetResult();

		_chat = (OpenAIChatHistory)_chatGPT.CreateNewChat();
		_chat.AddSystemMessage(npc_personality);

		base._Ready();
	}

	// Called when ChatEntity _inConvoWith emits a MsgSent signal
	public override void ReceiveMsg(string msg)
	{
		// Call the async method but don't wait for it
		Task.Run(() => ReceiveMsgAsync(msg));
	}

	public async Task ReceiveMsgAsync(string msg)
	{
		try
		{
			var msg_template = await File.ReadAllTextAsync("personalities/Message.txt");
			var context = _kernel.CreateNewContext();
			context["interlocutor"] = _inConvoWith.ChatName;
			context["message"] = msg;

			string fullMsg = await _promptRenderer.RenderAsync(msg_template, context);
			_chat.AddUserMessage(fullMsg);

			ChatRequestSettings settings = new();
			string reply = await _chatGPT.GenerateMessageAsync(_chat, settings);

			SendMsg(reply);
			_chat.AddAssistantMessage(reply);
		}
		catch (Exception ex)
		{
			string errMsg = $"Could not get reply from {ChatName}: {ex.Message}";
			GD.PrintErr(errMsg);
		}
	}

	protected virtual void HandleReply(System.Threading.Tasks.Task<string> replyTask)
	{
		// If there are no errors for the reply, send the message
		if (replyTask.Exception == null)
		{
			SendMsg(replyTask.Result);
		}
		// If there were some errors for the reply, send a message about the errors
		else
		{
			string errMsg = "Could not get reply from " + ChatName;
			GD.PrintErr(errMsg + ": ", replyTask.Exception.Message);
			SendMsg(errMsg);
		}
	}

	// Notifies semantic kernel agent about some event or other important piece of information
	public void Notify(string message)
	{
		GD.Print($"NOTIFY: {message}");
		_chat.AddUserMessage(message);
	}

	// Get the OpenAI API key from user secrets
	private string GetApiKey()
	{
		var configuration = new ConfigurationBuilder()
		.AddUserSecrets<ChatAI>()
		.Build();

		var apiKeyVarName = "OPENAI_API_KEY";
		var apiKey = configuration[apiKeyVarName];

		if (apiKey == null)
		{
			throw new System.Exception($"Please set the {apiKeyVarName} user secret.");
		}

		return apiKey;
	}
}