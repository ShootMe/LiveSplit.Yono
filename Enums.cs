using System.ComponentModel;
namespace LiveSplit.Yono {
	public enum SplitName {
		[Description("Windhill (Enter)"), ToolTip("Splits when entering Windhill")]
		Windhill,
		[Description("Hedgehog Forest (Enter)"), ToolTip("Splits when entering Hedgehog Forest")]
		Hedgehod_Forest_Start,
		[Description("Hedgehog Halfway (Enter)"), ToolTip("Splits when entering Hedgehog Halfway Point")]
		HedgeTunnel05,
		[Description("Knightingale City (Enter)"), ToolTip("Splits when entering Knightingale City")]
		Knightingale_Square,
		[Description("Knightingale Sewers (Enter)"), ToolTip("Splits when entering Knightingale Sewers")]
		Sewers01,
		[Description("Trollmoss Forest (Enter)"), ToolTip("Splits when entering Trollmoss Forest")]
		Trollmoss01,
		[Description("Trollmoss Halfway (Enter)"), ToolTip("Splits when entering Trollmoss Halfway Point")]
		Trollmoss09,
		[Description("The Sundergarden (Enter)"), ToolTip("Splits when entering The Sundergarden")]
		Sundergarden,
		[Description("Maggot's Keep (Enter)"), ToolTip("Splits when entering Maggot's Keep")]
		Crypt01,
		[Description("Maggot's Keep Boat Room (Enter)"), ToolTip("Splits when entering Maggot's Keep Boat Room")]
		Crypt10,
		[Description("Acorn Woods (Enter)"), ToolTip("Splits when entering Acorn Woods")]
		Acorn01,
		[Description("Freehaven (Enter)"), ToolTip("Splits when entering Freehaven")]
		Freehaven,
		[Description("Manufactoria (Enter)"), ToolTip("Splits when entering Manufactoria")]
		Factory01,
		[Description("Manufactoria Hub (Enter)"), ToolTip("Splits when entering Manufactoria Hub Area")]
		Factory06,
		[Description("Woolly Mountain (Enter)"), ToolTip("Splits when entering Woolly Mountain")]
		Woolly01,
		[Description("Woolly Halfway (Enter)"), ToolTip("Splits when entering Woolly Halfway Point")]
		Woolly05,
		[Description("Queen's Dungeon (Enter)"), ToolTip("Splits when entering Queen's Dungeon")]
		Dungeon01,
		[Description("Queen's Room 6 (Enter)"), ToolTip("Splits when entering Queen's Dungeon Room 6")]
		Dungeon06,
		[Description("Queen's Robot Prison (Enter)"), ToolTip("Splits when entering Queen's Dungeon Robot Prison Room")]
		Dungeon09,
		[Description("Queen's Room 19 (Enter)"), ToolTip("Splits when entering Queen's Dungeon Room 19")]
		Dungeon19,
		[Description("Hangman's Arena (Enter)"), ToolTip("Splits when entering Hangman's Arena")]
		Dungeon23,
		[Description("Knightingale Prison (Enter)"), ToolTip("Splits when entering Knightingale Prison")]
		Knight_Prison,
		[Description("Elephant Realm (Enter)"), ToolTip("Splits when entering Elephant Realm")]
		ElephantRealm,
		[Description("End Game"), ToolTip("Splits when ending the game")]
		EndGame,
		[Description("Health Token (Pickup)"), ToolTip("Splits when picking up a new Health Token")]
		HealthToken,
	}
	public enum EyeType {
		Normal,
		Closed,
		Happy,
		Dead
	}
	public class SaveData {
		public string Key, Value;
		public override string ToString() {
			return $"{Key}={Value}";
		}
	}
}