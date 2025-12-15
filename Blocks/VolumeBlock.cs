namespace Blocks;

using System.Diagnostics;

public enum VolumeOutputType {
  Speakers,
  Headphones
}

public class VolumeBlock : Block {
  public string MuteIcon;
  public string SpeakerIcon;
  public string HeadphonesIcon;

  private string getOutputIcon() {
    ProcessStartInfo pi = new ProcessStartInfo {
      FileName = "pactl",
      Arguments = "list sinks",
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    Process p = Process.Start(pi);

    string output = p.StandardOutput.ReadToEnd();
    p.WaitForExit();

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
    Process p = Process.Start(pi);

    string output = p.StandardOutput.ReadToEnd();
    p.WaitForExit();

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
    Process p = Process.Start(pi);

    string output = p.StandardOutput.ReadToEnd();
    p.WaitForExit();

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
  }

  public override void OnEnd() {
    if (isMute())
    {
      Result = MuteIcon;
    }
  }
}
