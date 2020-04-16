using System;
using System.Collections.Generic;
using System.IO;
namespace LiveSplit.Yono {
    public enum LogObject {
        CurrentSplit,
        Pointers,
        Version,
        Loading,
        SceneName,
        SaveData,
        SaveCount
    }
    public class LogManager {
        public const string LOG_FILE = "Yono.txt";
        private Dictionary<LogObject, string> currentValues = new Dictionary<LogObject, string>();
        private Dictionary<string, SaveData> currentSaveData = new Dictionary<string, SaveData>(StringComparer.OrdinalIgnoreCase);
        private bool enableLogging;
        public bool EnableLogging {
            get { return enableLogging; }
            set {
                if (value != enableLogging) {
                    enableLogging = value;
                    if (value) {
                        AddEntryUnlocked(new EventLogEntry("Initialized"));
                    }
                }
            }
        }

        public LogManager() {
            EnableLogging = false;
            Clear();
        }
        public void Clear(bool deleteFile = false) {
            lock (currentValues) {
                if (deleteFile) {
                    try {
                        File.Delete(LOG_FILE);
                    } catch { }
                }
                foreach (LogObject key in Enum.GetValues(typeof(LogObject))) {
                    currentValues[key] = null;
                }
            }
        }
        public void AddEntry(ILogEntry entry) {
            lock (currentValues) {
                AddEntryUnlocked(entry);
            }
        }
        private void AddEntryUnlocked(ILogEntry entry) {
            string logEntry = entry.ToString();
            if (EnableLogging) {
                try {
                    using (StreamWriter sw = new StreamWriter(LOG_FILE, true)) {
                        sw.WriteLine(logEntry);
                    }
                } catch { }
                Console.WriteLine(logEntry);
            }
        }
        public void Update(LogicManager logic, SplitterSettings settings) {
            if (!EnableLogging) { return; }

            lock (currentValues) {
                DateTime date = DateTime.Now;
                bool updateLog = true;

                foreach (LogObject key in Enum.GetValues(typeof(LogObject))) {
                    string previous = currentValues[key];
                    string current = null;

                    switch (key) {
                        case LogObject.CurrentSplit: current = $"{logic.CurrentSplit} ({GetCurrentSplit(logic, settings)})"; break;
                        case LogObject.Pointers: current = logic.Memory.GamePointers(); break;
                        case LogObject.Version: current = MemoryManager.Version.ToString(); break;
                        case LogObject.Loading: current = logic.Memory.IsLoading().ToString(); break;
                        case LogObject.SceneName: current = updateLog ? logic.Memory.SceneName() : previous; break;
                        case LogObject.SaveCount: current = updateLog ? logic.Memory.SaveDataCount().ToString() : previous; break;
                        case LogObject.SaveData: if (updateLog) { CheckItems<SaveData>(key, currentSaveData, logic.Memory.SaveData()); } break;
                    }

                    if (previous != current) {
                        AddEntryUnlocked(new ValueLogEntry(date, key, previous, current));
                        currentValues[key] = current;
                    }
                }
            }
        }
        private void CheckItems<T>(LogObject type, Dictionary<string, T> currentItems, Dictionary<string, T> newItems) {
            DateTime date = DateTime.Now;
            foreach (KeyValuePair<string, T> pair in newItems) {
                string key = pair.Key;
                T state = pair.Value;

                T oldState;
                if (!currentItems.TryGetValue(key, out oldState) || !state.Equals(oldState)) {
                    AddEntryUnlocked(new ValueLogEntry(date, type, oldState, state));
                    currentItems[key] = state;
                }
            }
            List<string> itemsToRemove = new List<string>();
            foreach (KeyValuePair<string, T> pair in currentItems) {
                string key = pair.Key;
                T state = pair.Value;

                if (!newItems.ContainsKey(key)) {
                    AddEntryUnlocked(new ValueLogEntry(date, type, state, null));
                    itemsToRemove.Add(key);
                }
            }
            for (int i = 0; i < itemsToRemove.Count; i++) {
                currentItems.Remove(itemsToRemove[i]);
            }
        }
        private string GetCurrentSplit(LogicManager logic, SplitterSettings settings) {
            if (logic.CurrentSplit >= settings.Autosplits.Count) { return "N/A"; }
            return settings.Autosplits[logic.CurrentSplit].ToString();
        }
    }
    public interface ILogEntry { }
    public class ValueLogEntry : ILogEntry {
        public DateTime Date;
        public LogObject Type;
        public object PreviousValue;
        public object CurrentValue;

        public ValueLogEntry(DateTime date, LogObject type, object previous, object current) {
            Date = date;
            Type = type;
            PreviousValue = previous;
            CurrentValue = current;
        }

        public override string ToString() {
            return string.Concat(
                Date.ToString(@"HH\:mm\:ss.fff"),
                ": (",
                Type.ToString(),
                ") ",
                PreviousValue,
                " -> ",
                CurrentValue
            );
        }
    }
    public class EventLogEntry : ILogEntry {
        public DateTime Date;
        public string Event;

        public EventLogEntry(string description) {
            Date = DateTime.Now;
            Event = description;
        }
        public EventLogEntry(DateTime date, string description) {
            Date = date;
            Event = description;
        }

        public override string ToString() {
            return string.Concat(
                Date.ToString(@"HH\:mm\:ss.fff"),
                ": ",
                Event
            );
        }
    }
}
