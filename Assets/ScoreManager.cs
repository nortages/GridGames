using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private const string pathToScores = "scores.js";
    private const string scoreFormat = "{0}. {1:00}:{2:00} - {3:MM/dd/yy HH:mm:ss}";
    private WindowsManager windowsManager;

    private void Awake()
    {
        windowsManager = FindObjectOfType<WindowsManager>();
    }

    public void OutputScores()
    {
        var jsonScores = File.ReadAllText(pathToScores);
        var dictScores = JsonConvert.DeserializeObject<Dictionary<DateTime, float>>(jsonScores);
        windowsManager.ShowScoresWindow(dictScores, scoreFormat);
    }

    public void SaveScore(float seconds)
    {
        // Save to file.
        if (!File.Exists(pathToScores))
        {
            File.WriteAllText(pathToScores, "{}");
        }

        var jsonScores = File.ReadAllText(pathToScores);
        var dictScores = JsonConvert.DeserializeObject<Dictionary<DateTime, float>>(jsonScores);
        dictScores.Add(DateTime.Now, seconds);
        var newJsonScores = JsonConvert.SerializeObject(dictScores, Formatting.Indented);
        File.WriteAllText(pathToScores, newJsonScores);
    }
    
}
