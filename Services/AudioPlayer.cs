using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace PacmanAvalonia.Services;

/// <summary>
/// Manages audio playback for the game, handling cross-platform differences between Linux and Windows.
/// </summary>
public class AudioPlayer
{
    private readonly string _wakaPath;
    private readonly string _deathPath;
    private readonly string _introPath;
    private readonly string _winPath;
    private readonly string _eatGhostPath;   
    private readonly string _powerPelletPath;

    /// <summary>
    /// Initializes a new instance of the AudioPlayer class and preloads file paths.
    /// </summary>
    public AudioPlayer()
    {
        _wakaPath = GetSoundPath("eat_dot_0.wav"); 
        _deathPath = GetSoundPath("death_1.wav");
        _introPath = GetSoundPath("start.wav");
        _winPath = GetSoundPath("intermission.wav"); 
        _eatGhostPath = GetSoundPath("eat_ghost.wav");
        _powerPelletPath = GetSoundPath("eat_fruit.wav");
    }

    /// <summary>
    /// Plays the characteristic "waka-waka" sound effect.
    /// </summary>
    public void PlayWaka()
    {
        PlaySound(_wakaPath);
    }

    /// <summary>
    /// Plays the sound effect triggered when Pacman loses a life.
    /// </summary>
    public void PlayDeath()
    {
        PlaySound(_deathPath);
    }
    
    /// <summary>
    /// Plays the game introduction music.
    /// </summary>
    public void PlayIntro()
    {
        PlaySound(_introPath);
    }

    /// <summary>
    /// Plays the victory sound effect when a level is completed.
    /// </summary>
    public void PlayWin()
    {
        PlaySound(_winPath);
    }
    /// <summary>
    /// Plays when pacman eats a Ghost
    /// </summary>
    public void PlayEatGhost() => PlaySound(_eatGhostPath);

    /// <summary>
    /// Plays when Pacman can eat ghosts
    /// </summary>
    public void PlayPowerPellet() => PlaySound(_powerPelletPath);
    
    /// <summary>
    /// Attempts to stop all currently playing sounds. 
    /// On Linux, this terminates the audio process.
    /// </summary>
    public void StopAll()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                Process.Start("pkill", "paplay");
            }
            catch 
            {
            }
        }
    }

    private string GetSoundPath(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "audio", fileName);
    }

    private void PlaySound(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "paplay",
                    Arguments = $"\"{path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch 
            {
                try 
                { 
                    Process.Start("aplay", $"\"{path}\""); 
                } 
                catch 
                { 
                }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(path);
                player.Play();
            }
            catch 
            { 
            }
        }
    }
}