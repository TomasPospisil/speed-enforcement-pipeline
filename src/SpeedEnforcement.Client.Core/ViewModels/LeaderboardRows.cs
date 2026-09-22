namespace SpeedEnforcement.Client.Core.ViewModels;

/// <summary>Row types bound by the grids. Immutable: the view model swaps rows, it never mutates them.</summary>
public sealed record GlobalLeaderboardRow(int Rank, string NumberPlate, float AverageSpeed, int CamerasPassed);

public sealed record CameraLeaderboardRow(int Rank, string NumberPlate, float Speed);
