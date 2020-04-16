using System.ComponentModel;
namespace LiveSplit.Yono {
    public enum SplitType {
        [Description("Manual Split")]
        ManualSplit,
        [Description("Area (Enter)")]
        AreaEnter,
        [Description("Area (Exit)")]
        AreaExit,
        [Description("Game Start")]
        GameStart,
        [Description("Game End")]
        GameEnd,
        [Description("Health Token")]
        HealthToken
    }
    public class Split {
        public string Name { get; set; }
        public SplitType Type { get; set; }
        public string Value { get; set; }

        public override string ToString() {
            return $"{Type}|{Value}";
        }
    }
}