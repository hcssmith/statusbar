namespace Blocks;

using System.Text;
using Bar;

public class Block
{

  private string? _emptyResponse;
  private string? _emptyColour;
  private string? _icon;
  private string? _fg;
  private string? _bg;
  private bool? _emptyIcon;

  private static string? _lastBackground;


  public TimeSpan Interval {init; get;}
  public string EmptyResponse {get => _emptyResponse ?? ""; set => _emptyResponse = value;}
  public string EmptyColour {get => _emptyColour ?? "#D90429"; set => _emptyColour = value;}
  public int LengthLimit {init; get;}
  public string Icon {get => _icon ?? ""; set => _icon = value;}
  public int Counter { get => _counter; }
  public string Foreground {get => _fg ?? "#ffffff"; set => _fg = value;}
  public string Background {get => _bg ?? "#000000"; set => _bg = value;}
  public bool EmptyIcon {get => _emptyIcon ?? false; set => _emptyIcon = value;}

  public string Result;

  public delegate void OnStartEvent();
  public delegate void OnEndEvent();
  public delegate void OnInitEvent();
  
  private OnStartEvent? StartActions;
  private OnEndEvent? EndActions;
  private OnInitEvent? InitActions;

  private long? lastRun;
  private int _counter;
  private bool cacheRun;

  static Block()
  {
    _lastBackground = null;
  }

  public Block() {
    _counter = 0;
    lastRun = null;
    Result =  "";
    cacheRun = false;
  }

  private string drawTriangle(string bg) {
    StringBuilder sb = new();
    int wstep = 1;
    int width = 20;
    int height = 50;

    int cw = 0;

    sb.Append($"^c{bg}^");
    for (int y=0; y<=height;y=y+2)
    {
      
      sb.Append($"^r{cw},{y},{width-cw},2^");
      cw+=wstep;
    }
    sb.Append($"^c{Block._lastBackground}^");
    for (int y=height;y>=0;y=y-2)
    {
      
      sb.Append($"^r{cw},{y},{width-cw},2^");
      cw+=wstep;
    }
    

    return sb.ToString();
  }


  public void Execute()
  {
    cacheRun = false;
    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    StartActions += Start;
    StartActions += OnStart;
    EndActions += BeforeEnd;
    EndActions += End;
    EndActions += OnEnd;
    InitActions += OnInit;

    if (lastRun is null) {
      foreach (Delegate initEvent in InitActions.GetInvocationList())
      {
        initEvent.DynamicInvoke();
      }
    }
    if (Interval == TimeSpan.Zero) {
      return;
    }
    if (lastRun is null  || now - lastRun >= Interval.TotalMilliseconds)
    {
      foreach (Delegate startEvent in StartActions.GetInvocationList())
      {
        startEvent.DynamicInvoke();
      }
      lastRun = now;
    } else {
      cacheRun = true;
    }
    foreach (Delegate endEvent in EndActions.GetInvocationList())
    {
      endEvent.DynamicInvoke();
    }
}

  private void Start()
  {
    _counter++;
  }

  private void End()
  {
    StartActions = null;
    EndActions = null;
    InitActions = null;

    if (cacheRun)
    {
      return;
    }

    string bg = Background;
    string results = Result;
    if (LengthLimit > 0 && Result.Length > LengthLimit) {
      string r = Result.Substring(0, LengthLimit);
      results = $"{r}…";
    }

    string o = "";

    if (results == "") {
      if (EmptyIcon) {
        o = $"{Icon} {EmptyResponse}";
      } else {
        o = EmptyResponse;
      }
      bg = EmptyColour;
    } else { 
      o = $"{Icon} {results}";
    }


    Block._lastBackground = bg;
    Result = $"{drawTriangle(bg)}^f20^^c{Foreground}^^b{bg}^ {o} ";
  }

  public virtual void OnStart() {}
  public virtual void OnEnd() {}
  public virtual void BeforeEnd() {}
  public virtual void OnInit() {}
}
