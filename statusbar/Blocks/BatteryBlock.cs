
using Microsoft.Extensions.Logging;

namespace Blocks;

public class BatterySettings : Settings {
  public string BatteryLabel {get; set;} = "BAT0";
  public int High {get;set;} = 90;
  public int Low {get;set;} = 20;
  public string HighIconDischarging {get;set;}= "F ";
  public string MediumIconDischarging {get;set;}= "M ";
  public string LowIconDischarging {get;set;}= "L ";
  public string HighIconCharging {get;set;}= "F+";
  public string MediumIconCharging {get;set;}= "M+";
  public string LowIconCharging {get;set;}= "L+";
  public string HighColour {get;set;}= "#3AA655";
  public string MediumColour {get;set;}= "#C9A227";
  public string LowColour {get;set;}= "#EE6C4D";
}

public class BatteryBlock : BlockBase {

  private BatterySettings _settings;

  private string _bg;
  private string _icon;

  internal override string Icon => _icon;
  internal override string Background => _bg;

  public override void UpdateSettings<T>(T s)
  {
    _settings = (BatterySettings)(Settings)s;
    base.UpdateInternalSettings(s);
  }

  public BatteryBlock(ILogger<BatteryBlock> logger, BatterySettings settings) : base(logger, settings) {
    _settings = settings;
    _bg = "";
    _icon = "";
  }
  
  public override Task<string> UpdateContent(CancellationToken ct) {
    string batteryPath = $"/sys/class/power_supply/{_settings.BatteryLabel}";
    string capacityPath = Path.Combine(batteryPath, "capacity");
    string statusPath = Path.Combine(batteryPath, "status");

    string capacityText;
    try {
      capacityText = File.ReadAllText(capacityPath).Trim();
    }
    catch (Exception ex) {
      _logger.LogError($"Could not read: {capacityPath}: {ex.Message}");
      return Task.FromResult("ERR");
    }

    int capacity;
    try {
      capacity = int.Parse(capacityText);
    }
    catch (Exception ex) {
      _logger.LogError($"Could not parse {capacityText} as a number: {ex.Message}");
      return Task.FromResult("ERR");
    }

    bool charging;
    try {
      charging = File.ReadAllText(statusPath).Trim() switch {
        "Discharging" => false,
        "Charging" => true,
        _ => false
      };
    } catch (Exception) {
      _logger.LogError("Could not get charging state, assuming discharging.");
      charging = false;
    }

    if (capacity >= _settings.High) {
      _bg = _settings.HighColour;
      _icon = charging ? _settings.HighIconCharging : _settings.HighIconDischarging;
    } else if (capacity <= _settings.Low) {
      _bg = _settings.LowColour;
      _icon = charging ? _settings.LowIconCharging : _settings.LowIconDischarging;
    } else {
      _bg = _settings.MediumColour;
      _icon = charging ? _settings.MediumIconCharging : _settings.MediumIconDischarging;
    }

    return Task.FromResult(capacityText);

  }

}

