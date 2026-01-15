using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Blocks;

public class VolumeSettings : Settings {
  public string MuteIcon {get; set;} = "M";
  public string SpeakerIcon {get; set;} = "S";
  public string HeadphonesIcon {get; set; } = "H";
  public string MuteColour {get; set;} = "#FFFFFF";
}

public class VolumeBlock : BlockBase {

  private VolumeSettings _settings;
  private string _bg;
  private string _icon;

  internal override string Background => _bg;
  internal override string Icon => _icon;

  public override void UpdateSettings<T>(T s)
  {
    _settings = (VolumeSettings)(Settings)s;
    base.UpdateInternalSettings(s);
  }

  public VolumeBlock(ILogger<VolumeBlock> logger, VolumeSettings settings) : base(logger, settings) {
    _settings = settings;
    _bg = "";
    _icon = "";
  }


  
  private async Task<string> PactlCommand(string arguments, CancellationToken cancellationToken) {
    var startInfo = new ProcessStartInfo {
      FileName = "pactl",
      Arguments = arguments,
      RedirectStandardOutput = true,
      RedirectStandardError = true
    };

    using var process = new Process {StartInfo = startInfo};

    process.Start();
    
    var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
    var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
    var exitTask = process.WaitForExitAsync(cancellationToken);
    var timeoutTask = Task.Delay(_settings.Delay);
    
    var completedTask = await Task.WhenAny(exitTask, timeoutTask);

    if (completedTask == timeoutTask && !process.HasExited) {
      process.Kill(entireProcessTree: true);
      _logger.LogError("pactl {arguments} timed out after {Timeout}", arguments, _settings.Delay);
      return "ERR";
    }

    if (cancellationToken.IsCancellationRequested) {
      if (!process.HasExited) {
        process.Kill(entireProcessTree: true);
      }
      return "ERR";
    }

    await Task.WhenAll(outputTask, errorTask, exitTask);

    var stdout = await outputTask;
    var stderr = await errorTask;
    var exitCode = process.ExitCode;

    if (exitCode != 0) {
      _logger.LogError("pactl {arguments} failed with ExitCode: {ExitCode} - Stderr: {Stderr}", arguments, exitCode, stderr.Trim());
      return "ERR";
    }

    return stdout.Trim();
  }

  private async Task<bool> IsMute(CancellationToken ct) {
    var mute_state = await PactlCommand("get-sink-mute @DEFAULT_SINK@", ct);

    if (mute_state is null) {
      _logger.LogWarning("Could not get mute state");
      return false;
    }

    int ci = mute_state.IndexOf(":", StringComparison.Ordinal);

    if (ci == -1) {
      _logger.LogError("Failed to parse mute state: no colon separator found");
      return false;
    }

    int startIndex = ci +1;

    string muteStatus = mute_state.Substring(startIndex, mute_state.Length - startIndex).Trim();

    return muteStatus switch {
      "yes" => true,
      "no" => false,
      _ => false
    };
  }
  
  private async Task<string> GetOutputIcon(CancellationToken cancellationToken) {
    var sinks = await PactlCommand("list sinks", cancellationToken);

    if (sinks is null) {
      _logger.LogWarning("No sinks found");
      return "ERR";
    }
    string ap = "Active Port:";

    int si = sinks.IndexOf(ap, StringComparison.Ordinal);
    int ei = sinks.IndexOf("\n", si, StringComparison.Ordinal);

    int offset = ap.Length;

    if (si == -1 || ei == -1 || ei <= si + offset) {
      _logger.LogError("Failed to parse active sink from output");
      return "?";
    }

    string sink = sinks.Substring(si + offset, ei - si - offset).Trim();

    return sink switch {
      "analog-output-headphones" => _settings.HeadphonesIcon,
      "analog-output-speaker" => _settings.SpeakerIcon,
      _ => Icon
    };
  }
  
  private async Task<int> GetVolume(CancellationToken cancellationToken) {
    var volume_state =await PactlCommand("get-sink-volume @DEFAULT_SINK@", cancellationToken);

    if (volume_state is null) {
      _logger.LogWarning("Could not get volume state");
      return 0;
    }

    int ci = volume_state.IndexOf("%", StringComparison.Ordinal);
    if (ci == -1) {
      _logger.LogError("Failed to parse volume state: no % separator found");
      return 0;
    }

    int start = ci - 1;
    while (start >= 0 && char.IsDigit(volume_state[start])) {
      start--;
    }
    string vol = volume_state.Substring(start + 1, ci - (start + 1));
    if (vol == "") {
      _logger.LogError("Failed to parse volume: empty volume string");
      return 0;
    }

    int volume;
    try {
      volume = int.Parse(vol);
    }
    catch (Exception ex) when (
      ex is FormatException ||
      ex is OverflowException
        ) {
      _logger.LogError("Failed to parse volume '{Vol}': {Ex}", vol, ex.Message);
      return 0;
    }
    return volume;
  }

  public async override Task<string> UpdateContent(CancellationToken ct) {
    if (await IsMute(ct)) {
      _icon = _settings.MuteIcon;
      _bg = _settings.MuteColour;
      return "";
    }
    _icon = await GetOutputIcon(ct);
    _bg = _settings.Background;
    return $"{await GetVolume(ct)}";
    
  }

}

