namespace Blocks;

using System.Text;

public class SeparatorBlock: Block {
  public required string SeparatorChar;
  public int? LeftPadding;
  public int? RightPadding;

  public override void OnInit() {
    StringBuilder sb = new();
    if (LeftPadding is not null) {
      for(int i = 0; i < LeftPadding; i++)
      {
        sb.Append(" ");
      }
    }
    sb.Append(SeparatorChar);
    if (RightPadding is not null) {
      for(int i = 0; i < RightPadding; i++)
      {
        sb.Append(" ");
      }
    }
    Result = sb.ToString();
  }
}
