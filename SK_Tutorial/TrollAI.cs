using Godot;

using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;

using System;
using System.Threading.Tasks;
using System.IO;

[GlobalClass]
public partial class TrollAI : ChatAI
{
    // Signal for when the riddle is answered correctly
    [Signal]
    public delegate void RiddleAnsweredEventHandler();

    private OpenAIChatHistory _eval_limerick_chat;

    public override void _Ready()
    {
        _eval_limerick_chat = (OpenAIChatHistory)_chatGPT.CreateNewChat();
        var eval_template = File.ReadAllText("personalities/Evaluator.txt");
        var eval_context = _kernel.CreateNewContext();
        string eval_prompt = _promptRenderer.RenderAsync(eval_template, eval_context).GetAwaiter().GetResult();
        _eval_limerick_chat.AddSystemMessage(eval_prompt);

        base._Ready();
    }

    public override void ReceiveMsg(string msg)
    {
        // Call the async method but don't wait for it
        Task.Run(() => ReceiveMsgAsync(msg));
    }

    public async Task ReceiveMsgAsync(string msg)
    {
        var msg_template = File.ReadAllText("personalities/Message.txt");
        var context = _kernel.CreateNewContext();
        context["interlocutor"] = _inConvoWith.ChatName;
        context["message"] = msg;
        string fullMsg = _promptRenderer.RenderAsync(msg_template, context).GetAwaiter().GetResult();
        _chat.AddUserMessage(fullMsg);

        ChatRequestSettings settings = new();
        var riddle_chat = (OpenAIChatHistory)_chatGPT.CreateNewChat();
        var riddle_template = File.ReadAllText("personalities/RiddleGenerator.txt");
        var riddle_context = _kernel.CreateNewContext();
        riddle_context["subject"] = msg;
        string riddle_prompt = _promptRenderer.RenderAsync(riddle_template, riddle_context).GetAwaiter().GetResult();
        riddle_chat.AddSystemMessage(riddle_prompt);

        string riddle_reply = await _chatGPT.GenerateMessageAsync(riddle_chat, settings);
        string riddle_result = $"For the riddle bank: {riddle_reply}";
        GD.Print(riddle_result);
        _chat.AddSystemMessage(riddle_result);

        // Do not add riddle result to eval chat. Eval is just on result coming out of troll _chat.
        string full_reply = await _chatGPT.GenerateMessageAsync(_chat, settings);
        SendMsg(full_reply);
        _chat.AddAssistantMessage(full_reply);
        _eval_limerick_chat.AddUserMessage(full_reply);

        var eval_limerick_reply = await _chatGPT.GenerateMessageAsync(_eval_limerick_chat, new ChatRequestSettings());

        GD.Print($"EVAL REPLY {eval_limerick_reply}");
        if (eval_limerick_reply == "True")
        {
            GD.Print("TROLL IS HAPPY");
            CallDeferred("emit_signal", SignalName.RiddleAnswered);
        }

        _eval_limerick_chat.AddAssistantMessage(eval_limerick_reply);
    }
}