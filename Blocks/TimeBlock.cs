namespace Blocks;

public class TimeBlock : Block {
  public required string FormatString;

  public override void OnStart() {
    Result = DateTime.Now.ToString(FormatString);
  }
}
