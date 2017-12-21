using System.ComponentModel;
namespace LiveSplit.Yono {
	public enum SplitName {
		[Description("Main Menu"), ToolTip("Splits when entering Main Menu")]
		MainMenu,
		[Description("Starfall Cave"), ToolTip("Splits when entering Starfall Cave")]
		StarfallCave,
		[Description("Tutorial Trail"), ToolTip("Splits when entering Tutorial Trail")]
		TutorialTrail,
		[Description("Windhill"), ToolTip("Splits when entering Windhill")]
		Windhill,
		[Description("End Game"), ToolTip("Splits when ending the game")]
		EndGame,
	}
}