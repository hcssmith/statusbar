namespace Bar;

using System.Text;
using Blocks;
using X11;

public class StatusBar
{
  private List<Block> blocks;
  private string cbar;
  XWindow root;
  public string? OuterPadding;
  public int Heartbeat;

  public StatusBar()
  {
    blocks = new();
    cbar = "";
    root = new();
    root.ConnectRootWindow();
    Heartbeat = 1000;
  }

  public void Add(Block b)
  {
    blocks.Add(b);
  }

  public void Render()
  {
    while (true)
    {
      
      StringBuilder sb = new();
      if (OuterPadding != default(string))
      {
        sb.Append(OuterPadding);
      }
      foreach (Block b in blocks)
      {
        b.Execute();
        sb.Append(b.Result);
      }
      if (OuterPadding != default(string))
      {
        sb.Append(OuterPadding);
        sb.Append("");
      }
      string nbar = sb.ToString();
      if (nbar != cbar) {
        root.SetWMName(nbar);
        cbar = nbar;
      }
      sb.Clear();
      Thread.Sleep(Heartbeat);
    }
  }
}
