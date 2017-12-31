using System.ComponentModel;
namespace LiveSplit.Yono {
	public enum SplitName {
		[Description("Windhill"), ToolTip("Splits when entering Windhill")]
		Windhill,
		[Description("Hedgehog Forest"), ToolTip("Splits when entering Hedgehog Forest")]
		Hedgehod_Forest_Start,
		[Description("Knightingale City"), ToolTip("Splits when entering Knightingale City")]
		Knightingale_Square,
		[Description("Trollmoss Forest"), ToolTip("Splits when entering Trollmoss Forest")]
		Trollmoss01,
		[Description("The Sundergarden"), ToolTip("Splits when entering The Sundergarden")]
		Sundergarden,
		[Description("Acorn Woods"), ToolTip("Splits when entering Acorn Woods")]
		Acorn01,
		[Description("Woolly Mountain"), ToolTip("Splits when entering Woolly Mountain")]
		Woolly01,
		[Description("Queen's Dungeons"), ToolTip("Splits when entering Queen's Dungeons")]
		Dungeon01,
		[Description("Hangman's Arena"), ToolTip("Splits when entering Hangman's Arena")]
		Dungeon23,
		[Description("Knightingale Prison"), ToolTip("Splits when entering Knightingale Prison")]
		Knight_Prison,
		[Description("Elephant Realm"), ToolTip("Splits when entering Elephant Realm")]
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