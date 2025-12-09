namespace Blocks;

public class BatteryBlock : Block {
  public required string BatteryLabel;

  private string capacityPath;
  private string statusPath;

  public override void OnInit() {
    string batteryPath = $"/sys/class/power_supply/{BatteryLabel}";
    capacityPath = Path.Combine(batteryPath, "capacity");
    statusPath = Path.Combine(batteryPath, "status");
  }

  

  public override void OnStart() {
    string capacityText = File.ReadAllText(capacityPath).Trim();

    int capacity = int.Parse(capacityText);
    string status = File.ReadAllText(statusPath).Trim() switch {
      "Discharging" => "-",
      "Charging" => "+",
      _ => ""
    };
    Result = $"{status}{capacity}%";
  }
}
