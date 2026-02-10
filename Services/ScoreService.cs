using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PacmanAvalonia.Services;

/// <summary>
/// Represents an immutable high score entry containing details about a completed game session.
/// </summary>
/// <param name="Date">The formatted date and time when the game was played.</param>
/// <param name="Name">The name or initials of the player.</param>
/// <param name="Level">The string representation of the level reached.</param>
/// <param name="Score">The numeric score value used for sorting.</param>
public record ScoreEntry(string Date, string Name, string Level, int Score);

/// <summary>
/// Provides static services for managing game high scores, including saving to and reading from a text file.
/// </summary>
public static class ScoreService
{
    /// <summary>
    /// The absolute path to the 'highscores.txt' file located in the application's base directory.
    /// </summary>
    private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highscores.txt");

    /// <summary>
    /// Appends a new score entry to the high scores file.
    /// </summary>
    /// <param name="playerName">The name entered by the player.</param>
    /// <param name="score">The final score achieved.</param>
    /// <param name="level">The level number the player was on.</param>
    public static void SaveScore(string playerName, int score, int level)
    {
        try
        {
            string line = $"{DateTime.Now:yyyy-MM-dd HH:mm}|{playerName.ToUpper()}|Level {level}|{score} PTS" + Environment.NewLine;
            
            File.AppendAllText(FilePath, line);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving score: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Retrieves the top 10 high scores from the file, ordered by score in descending order.
    /// </summary>
    /// <returns>A list of <see cref="ScoreEntry"/> objects.</returns>
    public static List<ScoreEntry> GetBestScores()
    {
        var list = new List<ScoreEntry>();
        
        if (!File.Exists(FilePath)) return list;

        try
        {
            var lines = File.ReadAllLines(FilePath);
            foreach (var line in lines)
            {
                var parts = line.Split('|');
                
                if (parts.Length == 4)
                {
                    string scorePart = parts[3].Replace("PTS", "").Trim();

                    if (int.TryParse(scorePart, out int scoreVal))
                    {
                        list.Add(new ScoreEntry(parts[0].Trim(), parts[1].Trim(), parts[2].Trim(), scoreVal));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing scores: {ex.Message}");
        }

        return list.OrderByDescending(x => x.Score).Take(10).ToList();
    }

    /// <summary>
    /// Deletes the high scores file, effectively resetting the leaderboard.
    /// </summary>
    public static void ClearScores()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}