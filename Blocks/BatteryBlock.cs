namespace Blocks;

public class BatteryBlock : Block {
  public required string BatteryLabel;
  public required int HighLevel;
  public required int LowLevel;
  public required string HighColour;
  public required string MediumColour;
  public required string LowColour;
  public required string HighIconDischarging;
  public required string MediumIconDischarging;
  public required string LowIconDischarging;
  public required string HighIconCharging;
  public required string MediumIconCharging;
  public required string LowIconCharging;


  private string? capacityPath;
  private string? statusPath;

  public override void OnInit() {
    string batteryPath = $"/sys/class/power_supply/{BatteryLabel}";
    capacityPath = Path.Combine(batteryPath, "capacity");
    statusPath = Path.Combine(batteryPath, "status");
  }

  

  public override void OnStart() {
    if (capacityPath is null || statusPath is null)
    {
      return;
    }
    string capacityText = File.ReadAllText(capacityPath).Trim();

    int capacity = int.Parse(capacityText);
    bool charging = File.ReadAllText(statusPath).Trim() switch {
      "Discharging" => false,
      "Charging" => true,
      _ => false
    };
    if (capacity >= HighLevel) {
      Background = HighColour;
      if (charging) {
        Icon = HighIconCharging;
      } else {
        Icon = HighIconDischarging;
      }
    } else if (capacity <=LowLevel) {
      Background = LowColour;
      if (charging) {
        Icon = LowIconCharging;
      } else {
        Icon = LowIconDischarging;
      }
    } else {
      Background = MediumColour;
      if (charging) {
        Icon = MediumIconCharging;
      } else {
        Icon = MediumIconDischarging;
      }
    }
    Result = capacityText;
  }
}
