using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private const string pathToScores = "scores.js";
    private const string scoreFormat = "{0}. {1} - {2:MM/dd/yy HH:mm:ss}";
    private WindowsManager windowsManager;
    private GameManager gameManager;

    private void Awake()
    {
        windowsManager = FindObjectOfType<WindowsManager>();
        gameManager = FindObjectOfType<GameManager>();
    }

    public void OutputScores()
    {
        if (!File.Exists(pathToScores))
        {
            File.WriteAllText(pathToScores, "{}");
        }
        var jsonScores = File.ReadAllText(pathToScores);
        var dictScores = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<DateTime, float>>>(jsonScores);
        var gameTitle = gameManager.currentGame.GameTitle;
        if (!dictScores.ContainsKey(gameTitle)) dictScores.Add(gameTitle, new Dictionary<DateTime, float>());
        var formattedScores = new List<string>(dictScores[gameTitle].Count);
        int i = 0;
        IOrderedEnumerable<KeyValuePair<DateTime, float>> sortedDict;
        if (gameManager.currentGame.SortScoresByDescending)
        {
            sortedDict = dictScores[gameTitle].OrderByDescending(pair => pair.Value);
        }
        else
        {
            sortedDict = dictScores[gameTitle].OrderBy(pair => pair.Value);
        }
        foreach (var item in sortedDict)
        {
            i++;
            var formattedValue = gameManager.currentGame.FormatScore(item.Value);
            formattedScores.Add(string.Format(scoreFormat, i, formattedValue, item.Key));
        }
        windowsManager.ShowScoresWindow(formattedScores);
    }

    public void SaveScore(float score)
    {
        // Saves to a file.

        if (!File.Exists(pathToScores))
        {
            File.WriteAllText(pathToScores, "{}");
        }
        var jsonScores = File.ReadAllText(pathToScores);
        var dictScores = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<DateTime, float>>>(jsonScores);
        var gameTitle = gameManager.currentGame.GameTitle;
        if (!dictScores.ContainsKey(gameTitle)) dictScores.Add(gameTitle, new Dictionary<DateTime, float>());

        dictScores[gameTitle].Add(DateTime.Now, score);

        var newJsonScores = JsonConvert.SerializeObject(dictScores, Formatting.Indented);
        File.WriteAllText(pathToScores, newJsonScores);
    }    
}
