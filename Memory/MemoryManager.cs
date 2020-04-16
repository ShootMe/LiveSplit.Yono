using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace LiveSplit.Yono {
    public partial class MemoryManager {
        private static ProgramPointer SceneManager = new ProgramPointer("Yono and the Celestial Elephants.exe",
            new FindPointerSignature(PointerVersion.All, AutoDeref.Double, "8B0D????????568BF185C974088B018B106A00FFD26A5856E8", 0x2));
        private static ProgramPointer GlobalSaveData = new ProgramPointer(
            new FindPointerSignature(PointerVersion.All, AutoDeref.Double, "893883EC0C57E8????????83C41083EC0C50E8????????83C4108B4714", -0x4));
        public static PointerVersion Version { get; set; } = PointerVersion.All;
        public Process Program { get; set; }
        public bool IsHooked { get; set; }
        public DateTime LastHooked { get; set; }
        private Dictionary<string, SaveData> saveData = new Dictionary<string, SaveData>(StringComparer.OrdinalIgnoreCase);

        public MemoryManager() {
            LastHooked = DateTime.MinValue;
        }
        public string GamePointers() {
            return string.Concat(
                $"SM: {(uint)SceneManager.GetPointer(Program):X} ",
                $"GSD: {(uint)GlobalSaveData.GetPointer(Program):X} "
            );
        }
        public bool IsLoading() {
            return SceneManager.Read<int>(Program, 0xc) > 1 || SceneManager.Read<int>(Program, 0x20) > 0;
        }
        public string SceneName() {
            int length = SceneManager.Read<int>(Program, 0x14, 0x30);
            IntPtr name = SceneManager.Read<IntPtr>(Program, 0x14) + 0x20;
            if (length <= 16) {
                return Program.ReadAscii(name);
            }
            return Program.ReadAscii(name, 0x0, 0x0);
        }
        public int HealthTokens() {
            string tokens = SaveData("phantTokens", "global");
            int healthTokens = 0;
            int.TryParse(tokens, out healthTokens);
            return healthTokens;
        }
        public int SaveDataCount() {
            return GlobalSaveData.Read<int>(Program, 0x10, 0x8, 0xc);
        }
        public string SaveData(string keyName, string keyIdentity) {
            IntPtr saveEntries = GlobalSaveData.Read<IntPtr>(Program, 0x10, 0x8);
            int count = Program.Read<int>(saveEntries, 0xc);
            byte[] data = Program.Read(saveEntries + 0x10, count * 0x4);
            for (int i = 0; i < count; i++) {
                IntPtr item = (IntPtr)BitConverter.ToUInt32(data, i * 0x4);
                if (item == IntPtr.Zero) { continue; }

                string name = Program.ReadString(item, 0x8, 0x0);
                if (keyName.Equals(name, StringComparison.OrdinalIgnoreCase) && keyIdentity.Equals(Program.ReadString(item, 0xc, 0x0), StringComparison.OrdinalIgnoreCase)) {
                    return GetValue(item, name);
                }
            }
            return string.Empty;
        }
        public Dictionary<string, SaveData> SaveData() {
            IntPtr saveEntries = (IntPtr)GlobalSaveData.Read<uint>(Program, 0x10, 0x8);
            saveData.Clear();
            int count = Program.Read<int>(saveEntries, 0xc);
            byte[] data = Program.Read(saveEntries + 0x10, count * 0x4);
            for (int i = 0; i < count; i++) {
                IntPtr item = (IntPtr)BitConverter.ToUInt32(data, i * 0x4);
                if (item == IntPtr.Zero) { continue; }

                string name = Program.ReadString(item, 0x8, 0x0);
                string identity = Program.ReadString(item, 0xc, 0x0);
                string key = $"{name}({identity})";
                saveData.Add(key, new SaveData() {
                    Key = key,
                    Value = GetValue(item, name)
                });
            }
            return saveData;
        }
        private string GetValue(IntPtr item, string name) {
            switch (name) {
                case "giftGiven":
                case "fireOn":
                case "permalit":
                case "countDone":
                case "destroyed":
                case "defeated":
                case "leverIsOn":
                case "boxFilled":
                case "spawned":
                case "guardAwake":
                case "melted":
                case "keyUsed":
                case "chestOpen":
                case "unlocked":
                case "lootTaken":
                case "loremasterAwake":
                case "lotus":
                case "hasMelted":
                case "goAway":
                case "phantomQuest":
                case "robotDogDefeated":
                case "scarecrow":
                case "sunGrown":
                case "triggered":
                case "waterwheel":
                    return Program.Read<bool>(item, 0x10, 0x8).ToString();
                case "posx":
                case "posy":
                case "posz":
                    return Program.Read<float>(item, 0x10, 0x8).ToString("0.0");
                case "currentElephantColor":
                case "timestamp":
                case "savedLocation":
                case "currentArea":
                    return Program.ReadString(item, 0x10, 0x0);
                default:
                    return Program.Read<int>(item, 0x10, 0x8).ToString();
            }
        }
        public bool HookProcess() {
            IsHooked = Program != null && !Program.HasExited;
            if (!IsHooked && DateTime.Now > LastHooked.AddSeconds(1)) {
                LastHooked = DateTime.Now;

                Process[] processes = Process.GetProcessesByName("Yono and the Celestial Elephants");
                Program = processes != null && processes.Length > 0 ? processes[0] : null;

                if (Program != null && !Program.HasExited) {
                    MemoryReader.Update64Bit(Program);
                    MemoryManager.Version = PointerVersion.All;
                    IsHooked = true;
                }
            }

            return IsHooked;
        }
        public void Dispose() {
            if (Program != null) {
                Program.Dispose();
            }
        }
    }
}