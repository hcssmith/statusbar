namespace Blocks;

using System.Diagnostics;

public class CommandBlock : Block {
  public required string Command;
  public List<string>? Arguments;

  public override void OnStart()
  {
    string args = "";
    if (Arguments is not null) {
      args = string.Join(" ", Arguments);
    }
    ProcessStartInfo pi = new ProcessStartInfo
    {
      FileName = Command,
      Arguments = args,
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    Process? p = Process.Start(pi);
    if (p is null)
    {
      return;
    }
    string output = p.StandardOutput.ReadToEnd().Trim();
    p.WaitForExit();

    Result = output;
  }
}
