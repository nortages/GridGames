using System;
using System.Collections.Generic;

public interface IGame
{
    string GameTitle { get; }
    void StartNewGame();
    void CloseGame();
    float RawScore { get; }
    string FormatScore(float rawScore);
    bool SortScoresByDescending { get; }
}