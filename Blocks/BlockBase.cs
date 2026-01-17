using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Blocks;

public class Settings {
  public int Interval {get; set;} = 1000;
  public string Id {get; set;} = "";
  public string Type {get; set;} = "";
  public int Order {get; set;} = 100;
  public string Icon {get; set;} = "";
  public string Background {get;set;} = "#2E2A44";
  public string Foreground {get; set;} = "#ffffff";
}

abstract public class BlockBase : BackgroundService {
  protected ILogger<BlockBase> _logger;

  private Settings _settings;

  public string Content;
  protected void UpdateInternalSettings(Settings s) => _settings = s;
  public int Order => _settings.Order;

  internal virtual string Background => _settings.Background;
  internal virtual string Foreground => _settings.Foreground;
  internal virtual string Icon => _settings.Icon;
  internal virtual string Id => _settings.Id;

  public abstract void UpdateSettings<T>(T s) where T:Settings;

  public BlockBase(ILogger<BlockBase> logger, Settings settings) {
    _logger = logger;
    _settings = settings;
    Content = "";
  }

  public abstract Task<string> UpdateContent(CancellationToken ct);

  protected override async Task ExecuteAsync(CancellationToken ct) {
    while (!ct.IsCancellationRequested) {
      string _content = await UpdateContent(ct);
      Content = $"^c{Background}^^t0,0,20,40,4^^f20^^c{Foreground}^^b{Background}^ {Icon}{_content} ^c{Background}^^t0,0,20,40,5^";
      await Task.Delay(_settings.Interval, ct);
    }
  }
} 

