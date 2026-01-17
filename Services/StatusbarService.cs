using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Blocks;
using X11;

namespace Services;


class StatusbarSettings {
  public int PollInterval {get; set; } = 1000;
  public IEnumerable<IConfigurationSection> Blocks {get; set;} = Enumerable.Empty<IConfigurationSection>();
}

class StatusbarService : BackgroundService {
  private ILogger<StatusbarService> _logger;
  private IServiceProvider _serviceProvider;
  private IOptionsMonitor<StatusbarSettings> _settings;
  private string _lastSettingsSnapshot;
  private List<BlockBase> _blocks;


  public int PollInterval => _settings.CurrentValue.PollInterval;

  public StatusbarService(ILogger<StatusbarService> logger, IServiceProvider sp, IOptionsMonitor<StatusbarSettings> settings) {
    _logger = logger;
    _serviceProvider = sp;
    _settings = settings;
    _lastSettingsSnapshot = CreateSettingsSnapshot(settings.CurrentValue);
    _blocks = new();
    _settings.OnChange(OnSettingsChanged);
  }
  
  private void CreateOrUpdate<TBlock, TSettings>(IConfigurationSection config)
    where TBlock : BlockBase
    where TSettings : Settings, new()
  {
    var settings = config.Get<TSettings>() ?? new TSettings () {};
    var block = settings.Id != "" ?
      _blocks.Where(b => b  is TBlock && b.Id == settings.Id).FirstOrDefault() as TBlock
      :_blocks.Where(b => b is TBlock).FirstOrDefault() as TBlock;
    if (block is null) {
      _logger.LogInformation("Creating block {0}", typeof(TBlock) );
      block = ActivatorUtilities.CreateInstance<TBlock>(
          _serviceProvider,
        settings
          );
      _blocks.Add(block);
    } else {
      _logger.LogInformation("Setting new config for block {0}", block.Id != "" ? block.Id : typeof(TBlock));
      ((TBlock)block).UpdateSettings(settings);
    }
  }

  private void OnSettingsChanged(StatusbarSettings newSettings) {
    var newSnapshot = CreateSettingsSnapshot(newSettings);
    if (AreSettingsEqual(_lastSettingsSnapshot, newSnapshot)) {
      _logger.LogDebug("Settings unchanged, skipping update");
      return;
    }

    _lastSettingsSnapshot = newSnapshot;

    foreach (var block in newSettings.Blocks) {
      string type = block["Type"] ?? "";
      switch (type) {
        case "TimeBlock":
          CreateOrUpdate<TimeBlock, TimeSettings>(block);
          break;
        case "BatteryBlock":
          CreateOrUpdate<BatteryBlock, BatterySettings>(block);
          break;
        case "VolumeBlock":
          CreateOrUpdate<VolumeBlock, VolumeSettings>(block);
          break;
        case "CommandBlock":
          CreateOrUpdate<CommandBlock, CommandSettings>(block);
          break;
        default: 
          _logger.LogError("Unknown type");
          break;
      }
    }
  }

  private bool AreSettingsEqual(string a, string b) {
    if (a == b) return true;
    if (a == null || b == null) return false;
    var equal = a == b;
    _logger.LogDebug("Settings equal: {0}", equal);
    return equal;
  }

  private string CreateSettingsSnapshot(StatusbarSettings settings) {
    var blocks = settings.Blocks ?? Enumerable.Empty<IConfigurationSection>();
    var blockStrings = blocks.Select(BlockToString).OrderBy(x => x).ToArray();
    return $"{settings.PollInterval}|{string.Join(";", blockStrings)}";
  }

  private string BlockToString(IConfigurationSection section) {
    if (section == null) return "";

    var pairs = section.AsEnumerable()
      .Where(pair => pair.Value != null)
      .Select(pair => $"{pair.Key}={pair.Value}")
      .OrderBy(x => x);

    return string.Join(",", pairs);
  }
  

  protected override async Task ExecuteAsync(CancellationToken ct) {
    _logger.LogInformation("Main scope started");

    foreach (var block in _settings.CurrentValue.Blocks) {
      string type = block["Type"] ?? "";
      switch (type) {
        case "TimeBlock":
          CreateOrUpdate<TimeBlock, TimeSettings>(block);
          break;
        case "BatteryBlock":
          CreateOrUpdate<BatteryBlock, BatterySettings>(block);
          break;
        case "VolumeBlock":
          CreateOrUpdate<VolumeBlock, VolumeSettings>(block);
          break;
        case "CommandBlock":
          CreateOrUpdate<CommandBlock, CommandSettings>(block);
          break;
        default:
          _logger.LogError("Unknown block type detected.");
          break;
      }
    }

    var blockTasks = _blocks.Select(b => b.StartAsync(ct)).ToArray();
    var pollTask = PollBlocks(ct);

    await Task.WhenAll(blockTasks.Concat(new[] { pollTask }));
    
    // Stop blocks on cancellation
    var stopTasks = _blocks.Select(b => b.StopAsync(ct));
    await Task.WhenAll(stopTasks);

  }

  private async Task PollBlocks(CancellationToken ct) {
    string bar = "";
    using XRootWindow root = new();

    while (!ct.IsCancellationRequested) {
      string new_bar = string.Join("", _blocks
          .OrderBy(b => { 
            return b.Order;
            })
          .Select(b => b.Content));
      if (new_bar != bar) {
        _logger.LogDebug(new_bar);
        root.SetWindowName(new_bar);
        bar = new_bar;
      }
      await Task.Delay(PollInterval);
    }
  }
}
