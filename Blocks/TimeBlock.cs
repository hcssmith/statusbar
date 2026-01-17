using Microsoft.Extensions.Logging;

namespace Blocks;

public class TimeSettings : Settings {
  public string Format {get; set;} = "HH:mm:ss";
}

public class TimeBlock : BlockBase {

  private TimeSettings _settings;
  public override void UpdateSettings<T>(T s)
  {
    _settings = (TimeSettings)(Settings)s;
    base.UpdateInternalSettings(s);
  }

  public TimeBlock(ILogger<TimeBlock> logger, TimeSettings settings) : base(logger, settings) {
    _settings = settings;
  }
  
  public override Task<string> UpdateContent(CancellationToken ct) => 
    Task.FromResult(DateTime.Now.ToString(_settings.Format));

}

