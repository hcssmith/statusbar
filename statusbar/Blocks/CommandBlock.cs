namespace Blocks;

using Microsoft.Extensions.Logging;
using System.Diagnostics;

public class CommandSettings : Settings {
  public string Command {get; set;} = "";
  public int Timeout {get;set;} = 1000;
}


public class CommandBlock : BlockBase {
  private CommandSettings _settings;

  public override void UpdateSettings<T>(T s)
  {
    _settings = (CommandSettings)(Settings)s;
    base.UpdateInternalSettings(s);
  }
  
  public string ExecuteCommand(string command, CancellationToken ct)
  {
    try
    {
      var processInfo = new ProcessStartInfo
      {
        FileName = command,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true
      };

      using var process = Process.Start(processInfo);
      if (process is null)
      {
        _logger.LogError("Failed to start process for command: {0}", command);
        return "";
      }

      var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
      var stderrTask = process.StandardError.ReadToEndAsync(ct);

      // Race between process exit and timeout
      var delayTask = Task.Delay(_settings.Timeout, ct);
      var exitTask = process.WaitForExitAsync();
      var completedTask = Task.WhenAny(exitTask, delayTask).GetAwaiter().GetResult();

      if (completedTask == delayTask)
      {
        process.Kill();
        _logger.LogError("Command timed out after {0}ms: {1}", _settings.Timeout, command);
        return "";
      }

      Task.WaitAll(stdoutTask, stderrTask);

      string stdout = stdoutTask.Result;
      string stderr = stderrTask.Result;

      if (!string.IsNullOrEmpty(stderr))
        _logger.LogError("Command stderr: {0}", stderr.Trim());

      return stdout.Trim();
    }
    catch (OperationCanceledException)
    {
      _logger.LogError("Command execution cancelled: {0}", command);
      return "";
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error executing command: {0}", command);
      return "";
    }
  }

  public CommandBlock(ILogger<CommandBlock> logger, CommandSettings settings) : base(logger, settings) {
    _settings = settings;
  }
  
  public override Task<string> UpdateContent(CancellationToken ct) => Task.FromResult(ExecuteCommand(_settings.Command, ct));
}
