using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
public partial class HUD : CanvasLayer
{
	[Signal]
	public delegate void StartGameEventHandler();
	public string playerName = "";
	public class Score
	{
		public Score(string a, int b)
		{
			name = a;
			score = b;
		}
		public string name { get; set; }
		public int score { get; set; }
	}
	public List<Score> scores = new List<Score>();
	public void ShowMessage(string text)
	{
		var message = GetNode<Label>("Message");
		message.Text = text;
		message.Show();
	
		GetNode<Timer>("MessageTimer").Start();
	}
	async public void ShowGameOver()
	{
		ShowMessage("Game Over \n Your score: " + GetNode<Label>("ScoreLabel").Text);
		if (playerName.Equals(""))
		{
			playerName = "player";
		}
		SaveGame(playerName,int.Parse(GetNode<Label>("ScoreLabel").Text));
		var messageTimer = GetNode<Timer>("MessageTimer");
		await ToSignal(messageTimer, Timer.SignalName.Timeout);
		var message = GetNode<Label>("Message");
		message.Text = "Dodge the Creeps!";
		message.Show();
		await ToSignal(GetTree().CreateTimer(1.0), SceneTreeTimer.SignalName.Timeout);
		GetNode<Button>("StartButton").Show();
		GetNode<TextEdit>("Input").Show();
		GetNode<HSlider>("Difficulty").Show();
		GetNode<Button>("ShowLeaderboard").Show();
	}
	public void UpdateScore(int score)
	{
		GetNode<Label>("ScoreLabel").Text = score.ToString();
	}
	private void OnStartButtonPressed()
	{
		if (GetNode<TextEdit>("Input").Text is null || GetNode<TextEdit>("Input").Text.Equals(""))
		{
			playerName = "player";
		}
		else
		{
			playerName = GetNode<TextEdit>("Input").Text;
		}
		GetNode<Button>("StartButton").Hide();
		EmitSignal(SignalName.StartGame);
		GetNode<TextEdit>("Input").Hide();
		GetNode<HSlider>("Difficulty").Hide();
		GetNode<Button>("ShowLeaderboard").Hide();
	}
	
	private void OnMessageTimerTimeout()
	{
		GetNode<Label>("Message").Hide();
	}
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
	}

	public void SaveGame(string playerName, int scored)
	{
		using var saveGame = FileAccess.Open("user://savegame.json", FileAccess.ModeFlags.ReadWrite);
		Score currentScore = new Score(playerName, scored);
		scores.Add(currentScore);
		var jsonString = JsonSerializer.Serialize(currentScore);
		saveGame.StoreLine(jsonString);
	}
	public void LoadSaves()
	{
		using var saveGame = FileAccess.Open("user://savegame.save", FileAccess.ModeFlags.Read);
		if (!FileAccess.FileExists("user://savegame.save"))
    	{
    	    return;
    	}
		while (saveGame.GetPosition() < saveGame.GetLength())
	    {
	        var jsonString = saveGame.GetLine();
	        var json = new Json();
	        var parseResult = json.Parse(jsonString);
	        if (parseResult != Error.Ok)
	        {
	            GD.Print($"JSON Parse Error: {json.GetErrorMessage()} in {jsonString} at line {json.GetErrorLine()}");
	            continue;
	        }
			else
			{
				scores.Add(JsonSerializer.Deserialize<Score>(jsonString));
			}
		}
	}
	public void ShowOrHideLeaderboard()
	{
		Vector2 position = new Vector2(0, 0);
		var row = GD.Load<PackedScene>("res://RowOfLeaderboard.tscn");
		var leaderboard = GetNode<Panel>("Leaderboard");
		if (leaderboard.Visible)
		{
			foreach (var node in GetNode<Panel>("Leaderboard").GetChildren())
			{
				node.QueueFree();
			}
			leaderboard.Hide();
			return;
		}
		else
		{
			leaderboard.Show();
			foreach (var item in scores)
			{
				Node2D instance = row.Instantiate<Node2D>();
				GetNode<Panel>("Leaderboard").AddChild(instance);
				instance.Position = position;
				instance.GetNode<Label>("PlayerName").Text = item.name;
				instance.GetNode<Label>("PlayerScore").Text = item.score.ToString();
				position.Y += 30;
			}
		}
	}
	public void SetDifficulty(float diff)
	{
		var node = GetNode("../.");
		switch (diff)
		{
			case 0:
				node.GetNode<Timer>("MobTimer").WaitTime = 2;
				break;
			case 1:
				node.GetNode<Timer>("MobTimer").WaitTime = 1;
				break;
			case 2:
				node.GetNode<Timer>("MobTimer").WaitTime = 0.5f;
				break;
			case 3:
				node.GetNode<Timer>("MobTimer").WaitTime = 0.3f;
				break;
		}
	}
}