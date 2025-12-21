namespace Blocks;

public class BatteryBlock : Block {
  public required string BatteryLabel;
  public required int HighLevel;
  public required int LowLevel;
  public required string HighColour;
  public required string MediumColour;
  public required string LowColour;


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
    string status = File.ReadAllText(statusPath).Trim() switch {
      "Discharging" => "-",
      "Charging" => "+",
      _ => ""
    };
    if (capacity >= HighLevel) {
      Background = HighColour;
    } else if (capacity <=LowLevel) {
      Background = LowColour;
    } else {
      Background = MediumColour;
    }
    Result = $"{status}{capacity}%";
  }
}
