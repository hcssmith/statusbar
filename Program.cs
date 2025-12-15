using Blocks;
using Bar;


public class Program {
  public static void Main()
  {
    StatusBar b = new();
    b.OuterPadding = " ";

    b.Add(new CommandBlock() {
      Interval = TimeSpan.FromSeconds(5),
      Command = "ssid.sh",
      EmptyResponse = "disconnected",
      Icon = " ",
      LengthLimit = 12
        });

    b.Add(new SeparatorBlock() {
      SeparatorChar = "",
      LeftPadding = 1,
      RightPadding = 1
        });

    b.Add(new VolumeBlock() {
      Interval = TimeSpan.FromSeconds(1),
      Icon = " ",
      SpeakerIcon = " ",
      HeadphonesIcon = " ",
      MuteIcon = " "
        });

    b.Add(new SeparatorBlock() {
      SeparatorChar = "",
      LeftPadding = 1,
      RightPadding = 1
        });

    b.Add(new BatteryBlock() {
      Interval = TimeSpan.FromSeconds(5),
      BatteryLabel = "BAT0",
      Icon = "󰁹"
        });

    b.Add(new SeparatorBlock() {
      SeparatorChar = "",
      LeftPadding = 1,
      RightPadding = 1
        });
    
    b.Add(new TimeBlock() {
      Interval = TimeSpan.FromSeconds(15),
      FormatString = "HH:mm",
      Icon = " "
        });

    b.Render();
  }
}

