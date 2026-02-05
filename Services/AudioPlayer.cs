using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace PacmanAvalonia.Services;

public class AudioPlayer
{
    private readonly string _wakaPath;
    private readonly string _deathPath;
    private readonly string _introPath;
    private readonly string _winPath;

    public AudioPlayer()
    {
        _wakaPath = GetSoundPath("waka.wav");
        _deathPath = GetSoundPath("deathSound.wav");
        _introPath = GetSoundPath("titleMusic.wav");
        _winPath = GetSoundPath("levelWin.wav");
    }

    private string GetSoundPath(string fileName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "audio", fileName);
    }
    private void PlaySound(string path)
    {
        if (!File.Exists(path)) return;

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
                try { Process.Start("aplay", $"\"{path}\""); } catch { }
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                System.Media.SoundPlayer player = new System.Media.SoundPlayer(path);
                player.Play();
            }
            catch { }
        }
    }

    public void PlayWaka() => PlaySound(_wakaPath);

    public void PlayDeath() => PlaySound(_deathPath);
    
    public void PlayIntro() => PlaySound(_introPath);

    public void PlayWin() => PlaySound(_winPath);

    public void StopAll()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            try
            {
                Process.Start("pkill", "paplay");
            }
            catch { }
        }
    }
}


// private void PlaySound()
// {
//     var stream = AssetLoader.Open(
//         new Uri("avares://FlappyBird_Avalonia/Assets/Media/gameOver.wav")
//     );
// 
//     var reader = new WaveFileReader(stream);
//     var output = new WaveOutEvent();
// 
//     output.Init(reader);
//     output.Play();
// }