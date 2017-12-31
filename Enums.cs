using System.ComponentModel;
namespace LiveSplit.Yono {
	public enum SplitName {
		[Description("Windhill (Enter)"), ToolTip("Splits when entering Windhill")]
		Windhill,
		[Description("Hedgehog Forest (Enter)"), ToolTip("Splits when entering Hedgehog Forest")]
		Hedgehod_Forest_Start,
		[Description("Knightingale City (Enter)"), ToolTip("Splits when entering Knightingale City")]
		Knightingale_Square,
		[Description("Trollmoss Forest (Enter)"), ToolTip("Splits when entering Trollmoss Forest")]
		Trollmoss01,
		[Description("The Sundergarden (Enter)"), ToolTip("Splits when entering The Sundergarden")]
		Sundergarden,
		[Description("Acorn Woods (Enter)"), ToolTip("Splits when entering Acorn Woods")]
		Acorn01,
		[Description("Woolly Mountain (Enter)"), ToolTip("Splits when entering Woolly Mountain")]
		Woolly01,
		[Description("Queen's Dungeons (Enter)"), ToolTip("Splits when entering Queen's Dungeons")]
		Dungeon01,
		[Description("Hangman's Arena (Enter)"), ToolTip("Splits when entering Hangman's Arena")]
		Dungeon23,
		[Description("Knightingale Prison (Enter)"), ToolTip("Splits when entering Knightingale Prison")]
		Knight_Prison,
		[Description("Elephant Realm (Enter)"), ToolTip("Splits when entering Elephant Realm")]
		ElephantRealm,
		[Description("End Game"), ToolTip("Splits when ending the game")]
		EndGame,
	}
	public class SaveData {
		public string Key, Value;
		public override string ToString() {
			return $"{Key}={Value}";
		}
	}
}