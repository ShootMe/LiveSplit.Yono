using System;
namespace LiveSplit.Yono {
    public class LogicManager {
        public bool ShouldSplit { get; private set; }
        public bool ShouldReset { get; private set; }
        public int CurrentSplit { get; private set; }
        public bool Running { get; private set; }
        public bool Paused { get; private set; }
        public float GameTime { get; private set; }
        public MemoryManager Memory { get; private set; }
        public SplitterSettings Settings { get; private set; }
        private bool lastBoolValue;
        private int lastIntValue;
        private string lastStrValue;
        private DateTime splitLate;

        public LogicManager(SplitterSettings settings) {
            Memory = new MemoryManager();
            Settings = settings;
            splitLate = DateTime.MaxValue;
        }

        public void Reset() {
            splitLate = DateTime.MaxValue;
            Paused = false;
            Running = false;
            CurrentSplit = 0;
            InitializeSplit();
            ShouldSplit = false;
            ShouldReset = false;
        }
        public void Decrement() {
            CurrentSplit--;
            splitLate = DateTime.MaxValue;
            InitializeSplit();
        }
        public void Increment() {
            Running = true;
            splitLate = DateTime.MaxValue;
            CurrentSplit++;
            InitializeSplit();
        }
        private void InitializeSplit() {
            if (CurrentSplit < Settings.Autosplits.Count) {
                bool temp = ShouldSplit;
                CheckSplit(Settings.Autosplits[CurrentSplit], true);
                ShouldSplit = temp;
            }
        }
        public bool IsHooked() {
            bool hooked = Memory.HookProcess();
            Paused = !hooked;
            ShouldSplit = false;
            ShouldReset = false;
            GameTime = -1;
            return hooked;
        }
        public void Update(int currentSplit) {
            if (currentSplit != CurrentSplit) {
                CurrentSplit = currentSplit;
                Running = CurrentSplit > 0;
                InitializeSplit();
            }

            if (CurrentSplit < Settings.Autosplits.Count) {
                CheckSplit(Settings.Autosplits[CurrentSplit], !Running);
                if (!Running) {
                    Paused = true;
                    if (ShouldSplit) {
                        Running = true;
                    }
                }

                if (ShouldSplit) {
                    Increment();
                }
            }
        }
        private void CheckSplit(Split split, bool updateValues) {
            ShouldSplit = false;
            Paused = Memory.IsLoading();
            int saveCount = Memory.SaveDataCount();

            if (!updateValues && saveCount == 0) {
                return;
            }

            switch (split.Type) {
                case SplitType.ManualSplit:
                    break;
                case SplitType.GameStart:
                    CheckGameStart();
                    break;
                case SplitType.GameEnd:
                    CheckGameEnd();
                    break;
                case SplitType.AreaEnter:
                    CheckArea(split, true);
                    break;
                case SplitType.AreaExit:
                    CheckArea(split, false);
                    break;
                case SplitType.HealthToken:
                    CheckHealthTokens();
                    break;
            }

            if (Running && saveCount == 0) {
                ShouldSplit = false;
            } else if (DateTime.Now > splitLate) {
                ShouldSplit = true;
                splitLate = DateTime.MaxValue;
            }
        }
        private void CheckGameStart() {
            int saveDataCount = Memory.SaveDataCount();
            ShouldSplit = saveDataCount == 0 && lastIntValue > 0 && !string.IsNullOrEmpty(Memory.SceneName());
            lastIntValue = saveDataCount;
        }
        private void CheckGameEnd() {
            ShouldSplit = Paused && !lastBoolValue && "ElephantRealm".Equals(Memory.SceneName(), StringComparison.OrdinalIgnoreCase);
            lastBoolValue = Paused;
        }
        private void CheckHealthTokens() {
            int healthTokens = Memory.HealthTokens();
            ShouldSplit = healthTokens > 0 && healthTokens > lastIntValue;
            lastIntValue = healthTokens;
        }
        private void CheckArea(Split split, bool enter) {
            SplitArea area = Utility.GetEnumValue<SplitArea>(split.Value);
            switch (area) {
                case SplitArea.Windhill: CheckScene(enter, "Windhill"); break;
                case SplitArea.Hedgehod_Forest_Start: CheckScene(enter, "Hedgehod_Forest_Start"); break;
                case SplitArea.HedgeTunnel05: CheckScene(enter, "HedgeTunnel05"); break;
                case SplitArea.Knightingale_Square: CheckScene(enter, "Knightingale_Square"); break;
                case SplitArea.Sewers01: CheckScene(enter, "Sewers01"); break;
                case SplitArea.Trollmoss01: CheckScene(enter, "Trollmoss01"); break;
                case SplitArea.Trollmoss09: CheckScene(enter, "Trollmoss09"); break;
                case SplitArea.Sundergarden: CheckScene(enter, "Sundergarden"); break;
                case SplitArea.Crypt01: CheckScene(enter, "Crypt01"); break;
                case SplitArea.Crypt10: CheckScene(enter, "Crypt10"); break;
                case SplitArea.Acorn01: CheckScene(enter, "Acorn01"); break;
                case SplitArea.Freehaven: CheckScene(enter, "Freehaven"); break;
                case SplitArea.Factory01: CheckScene(enter, "Factory01"); break;
                case SplitArea.Factory06: CheckScene(enter, "Factory06"); break;
                case SplitArea.Woolly01: CheckScene(enter, "Woolly01"); break;
                case SplitArea.Woolly05: CheckScene(enter, "Woolly05"); break;
                case SplitArea.Dungeon01: CheckScene(enter, "Dungeon01", "Dungeon24"); break;
                case SplitArea.Dungeon06: CheckScene(enter, "Dungeon06"); break;
                case SplitArea.Dungeon09: CheckScene(enter, "Dungeon09"); break;
                case SplitArea.Dungeon19: CheckScene(enter, "Dungeon19"); break;
                case SplitArea.Dungeon23: CheckScene(enter, "Dungeon23"); break;
                case SplitArea.Knight_Prison: CheckScene(enter, "Knight_Prison"); break;
                case SplitArea.ElephantRealm: CheckScene(enter, "ElephantRealm"); break;
            }
        }
        private void CheckScene(bool onEnter, params string[] scenesToCheck) {
            string scene = Memory.SaveData("savedLocation", "global");
            if (string.IsNullOrEmpty(scene)) { return; }

            if (!scene.Equals(lastStrValue, StringComparison.OrdinalIgnoreCase)) {
                for (int i = 0; i < scenesToCheck.Length; i++) {
                    if (onEnter) {
                        if (scene.Equals(scenesToCheck[i], StringComparison.OrdinalIgnoreCase)) {
                            ShouldSplit = true;
                            break;
                        }
                    } else if (scenesToCheck[i].Equals(lastStrValue, StringComparison.OrdinalIgnoreCase)) {
                        ShouldSplit = true;
                        break;
                    }
                }
            }
            lastStrValue = scene;
        }
    }
}