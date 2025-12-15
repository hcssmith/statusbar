namespace Blocks;

public class Block
{
  public TimeSpan Interval {init; get;}
  public string EmptyResponse {init; get;}
  public int LengthLimit {init; get;}
  public string Icon {set; get;}
  public int Counter { get => _counter; }

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

  public Block() {
    _counter = 0;
    lastRun = null;
    Result =  "";
    cacheRun = false;
  }


  public void Execute()
  {
    cacheRun = false;
    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    StartActions += Start;
    StartActions += OnStart;
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

    string results = Result;
    if (LengthLimit > 0 && Result.Length > LengthLimit) {
      string r = Result.Substring(0, LengthLimit);
      results = $"{r}...";
    }

    if (results == "" && EmptyResponse != default(string)) {
      results = EmptyResponse;
    }

    string final_result = results;
    if (Icon != default(string))
    {
      final_result = $"{Icon} {results}";
    }

    Result = final_result;
  }

  public virtual void OnStart() {}
  public virtual void OnEnd() {}
  public virtual void OnInit() {}
}
