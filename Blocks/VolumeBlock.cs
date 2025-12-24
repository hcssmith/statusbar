namespace Blocks;

using System.Diagnostics;

public class VolumeBlock : Block {
  private string? _speakerIcon;
  private string? _headphonesIcon;
  private TimeSpan? _timeout;
  public string MuteIcon {get => EmptyResponse ?? "?"; set => EmptyResponse = value;}
  public string SpeakerIcon {get => _speakerIcon ?? "?"; set => _speakerIcon = value;}
  public string HeadphonesIcon {get => _headphonesIcon ?? "?"; set => _headphonesIcon = value;}

  public TimeSpan Timeout {get => _timeout ?? TimeSpan.FromSeconds(2); set => _timeout = value;}

  private string getOutputIcon() {

    ProcessStartInfo pi = new ProcessStartInfo {
      FileName = "pactl",
      Arguments = "list sinks",
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    Process? p = Process.Start(pi);
    if (p is null)
    {
      return "?";
    }

    string output = p.StandardOutput.ReadToEnd();
    if (!p.WaitForExit(Timeout))
    {
      p.Kill();
    }

    int si = output.IndexOf("Active Port:", StringComparison.Ordinal);
    int ei = output.IndexOf("\n", si, StringComparison.Ordinal);

    int offset = 12;
    string sink = output.Substring(si + offset, ei - si - offset).Trim();
    
    return sink switch {
      "analog-output-headphones" => HeadphonesIcon,
      "analog-output-speaker" => SpeakerIcon,
      _ => Icon
    };
  }

  private bool isMute() {
    ProcessStartInfo pi = new ProcessStartInfo {
      FileName = "pactl",
      Arguments = "get-sink-mute @DEFAULT_SINK@",
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };
    Process? p = Process.Start(pi);
    if (p is null)
    {
      return false;
    }

    string output = p.StandardOutput.ReadToEnd();
    if (!p.WaitForExit(Timeout))
    {
      p.Kill();
    }

    int ci = output.IndexOf(":", StringComparison.Ordinal) +1;

    string muteStatus = output.Substring(ci, output.Length - ci ).Trim();

    return muteStatus switch {
      "yes" => true,
      "no" => false,
      _ => false
    };

  }

  

  private int getVolume()
  {
    ProcessStartInfo pi = new ProcessStartInfo {
      FileName = "pactl",
      Arguments = "get-sink-volume @DEFAULT_SINK@",
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };
    Process? p = Process.Start(pi);
    if (p is null)
    {
      return 0;
    }

    string output = p.StandardOutput.ReadToEnd();
    if (!p.WaitForExit(Timeout))
    {
      p.Kill();
    }

    int ci = output.IndexOf("%", StringComparison.Ordinal);

    int start = ci-1;
    while (start>=0 && char.IsDigit(output[start]))
    {
      start--;
    }
    string vol = output.Substring(start+1, ci - (start+1));
    return int.Parse(vol);

  }

  public override void OnStart() {
    Icon = getOutputIcon();
    Result = $"{getVolume()}";
    if (isMute())
    {
      Result = "";
    }
  }

}
