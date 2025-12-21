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
      EmptyColour = "#D90429",
      Icon = " ",
      LengthLimit = 12,
      Background = "#0D3B66",
      EmptyIcon = true
        });

    b.Add(new VolumeBlock() {
      Interval = TimeSpan.FromSeconds(1),
      Icon = " ",
      SpeakerIcon = " ",
      HeadphonesIcon = " ",
      MuteIcon = " ",
      Background = "#4DA8DA",
      EmptyColour = "#8A8F98",
        });

    b.Add(new BatteryBlock() {
      Interval = TimeSpan.FromSeconds(5),
      BatteryLabel = "BAT0",
      Icon = "󰁹",
      LowLevel = 25,
      HighLevel = 70,
      HighColour = "#3AA655",
      MediumColour = "#C9A227",
      LowColour = "#EE6C4D",
        });

    b.Add(new TimeBlock() {
      Interval = TimeSpan.FromSeconds(15),
      FormatString = "HH:mm",
      Icon = " ",
      Background = "#6A4C93",
        });

    b.Render();
  }
}

