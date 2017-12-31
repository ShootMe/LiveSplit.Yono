#if !Info
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
#endif
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.Yono {
#if !Info
	public class SplitterComponent : UI.Components.IComponent {
		public TimerModel Model { get; set; }
#else
	public class SplitterComponent {
#endif
		public string ComponentName { get { return "Yono and the Celestial Elephants Autosplitter"; } }
		public IDictionary<string, Action> ContextMenuControls { get { return null; } }
		private static string LOGFILE = "_Yono.log";
		internal static string[] keys = { "CurrentSplit", "Loading", "SceneName", "SaveData" };
		private SplitterMemory mem;
		private int currentSplit = -1, lastLogCheck = 0;
		private bool hasLog = false;
		private Dictionary<string, string> currentValues = new Dictionary<string, string>();
		private SplitterSettings settings;
		private string lastSavedLocation;
		private bool isAutoSplit;
#if !Info
		public SplitterComponent(LiveSplitState state) {
#else
		public SplitterComponent() {
#endif
			mem = new SplitterMemory();
			settings = new SplitterSettings();
			foreach (string key in keys) {
				currentValues[key] = "";
			}

#if !Info
			if (state != null) {
				Model = new TimerModel() { CurrentState = state };
				Model.InitializeGameTime();
				Model.CurrentState.IsGameTimePaused = true;
				state.OnReset += OnReset;
				state.OnPause += OnPause;
				state.OnResume += OnResume;
				state.OnStart += OnStart;
				state.OnSplit += OnSplit;
				state.OnUndoSplit += OnUndoSplit;
				state.OnSkipSplit += OnSkipSplit;
			}
#endif
		}

		public void GetValues() {
			if (!mem.HookProcess()) {
				Model.CurrentState.IsGameTimePaused = true;
				return;
			}

#if !Info
			if (Model != null) {
				HandleSplits();
			}
#endif

			LogValues();
		}
#if !Info
		private void HandleSplits() {
			bool shouldSplit = false;

			if (currentSplit == -1) {
				shouldSplit = mem.SceneName().Equals("MainMenu", StringComparison.OrdinalIgnoreCase) && mem.SaveDataCount() == 0;
			} else if (Model.CurrentState.CurrentPhase == TimerPhase.Running) {
				SplitName split = currentSplit + 1 < settings.Splits.Count ? settings.Splits[currentSplit + 1] : SplitName.EndGame;
				string savedLocation = mem.SaveData("savedLocation(global)");
				switch (split) {
					case SplitName.Windhill:
					case SplitName.Hedgehod_Forest_Start:
					case SplitName.Knightingale_Square:
					case SplitName.Trollmoss01:
					case SplitName.Sundergarden:
					case SplitName.Acorn01:
					case SplitName.Woolly01:
					case SplitName.Dungeon01:
					case SplitName.Dungeon23:
					case SplitName.Knight_Prison:
					case SplitName.ElephantRealm:
						shouldSplit = !savedLocation.Equals(lastSavedLocation, StringComparison.OrdinalIgnoreCase) && savedLocation.Equals(split.ToString(), StringComparison.OrdinalIgnoreCase);
						break;
					case SplitName.EndGame:
						shouldSplit = mem.SceneName().Equals("ElephantRealm", StringComparison.OrdinalIgnoreCase) && !Model.CurrentState.IsGameTimePaused && mem.IsLoading();
						if (shouldSplit) {
							isAutoSplit = true;
						}
						break;
				}
				lastSavedLocation = savedLocation;
			}

			Model.CurrentState.IsGameTimePaused = mem.IsLoading();
			HandleSplit(shouldSplit, false);
		}
		private void HandleSplit(bool shouldSplit, bool shouldReset = false) {
			if (shouldReset) {
				if (currentSplit >= 0) {
					Model.Reset();
				}
			} else if (shouldSplit) {
				if (currentSplit < 0) {
					Model.Start();
				} else {
					Model.Split();
				}
			}
		}
#endif
		private void LogValues() {
			if (lastLogCheck == 0) {
				hasLog = File.Exists(LOGFILE);
				lastLogCheck = 300;
			}
			lastLogCheck--;

			if (hasLog || !Console.IsOutputRedirected) {
				string prev = string.Empty, curr = string.Empty;
				foreach (string key in keys) {
					prev = currentValues[key];

					switch (key) {
						case "CurrentSplit": curr = currentSplit.ToString(); break;
						case "Loading": curr = mem.IsLoading().ToString(); break;
						case "SceneName": curr = mem.SceneName(); break;
						case "SaveData":
							List<SaveData> data = mem.SaveData();
							for (int i = 0; i < data.Count; i++) {
								SaveData rs = data[i];
								string temp;
								if (!currentValues.TryGetValue(rs.Key, out temp)) {
									temp = string.Empty;
								}
								CompareAndLog(rs.Key, rs.Value, temp);
							}
							curr = data.Count.ToString();
							break;
						default: curr = string.Empty; break;
					}

					CompareAndLog(key, curr, prev);
				}
			}
		}
		private void CompareAndLog(string key, string current, string previous) {
			if (previous == null) { previous = string.Empty; }
			if (current == null) { current = string.Empty; }
			if (!previous.Equals(current)) {
				WriteLogWithTime(key + ": ".PadRight(Math.Max(0, 16 - key.Length), ' ') + previous.PadLeft(25, ' ') + " -> " + current);

				currentValues[key] = current;
			}
		}
		private void WriteLog(string data) {
			if (hasLog || !Console.IsOutputRedirected) {
				if (!Console.IsOutputRedirected) {
					Console.WriteLine(data);
				}
				if (hasLog) {
					using (StreamWriter wr = new StreamWriter(LOGFILE, true)) {
						wr.WriteLine(data);
					}
				}
			}
		}
		private void WriteLogWithTime(string data) {
#if !Info
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + (Model != null && Model.CurrentState.CurrentTime.RealTime.HasValue ? " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) : "") + ": " + data);
#else
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + ": " + data);
#endif
		}

#if !Info
		public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
			//Remove duplicate autosplitter componenets
			IList<ILayoutComponent> components = lvstate.Layout.LayoutComponents;
			bool hasAutosplitter = false;
			for (int i = components.Count - 1; i >= 0; i--) {
				ILayoutComponent component = components[i];
				if (component.Component is SplitterComponent) {
					if ((invalidator == null && width == 0 && height == 0) || hasAutosplitter) {
						components.Remove(component);
					}
					hasAutosplitter = true;
				}
			}

			GetValues();
		}

		public void OnReset(object sender, TimerPhase e) {
			currentSplit = -1;
			isAutoSplit = false;
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog("---------Reset----------------------------------");
		}
		public void OnResume(object sender, EventArgs e) {
			WriteLog("---------Resumed--------------------------------");
		}
		public void OnPause(object sender, EventArgs e) {
			WriteLog("---------Paused---------------------------------");
		}
		public void OnStart(object sender, EventArgs e) {
			currentSplit = 0;
			isAutoSplit = false;
			Model.CurrentState.IsGameTimePaused = true;
			Model.CurrentState.SetGameTime(TimeSpan.FromSeconds(0));
			WriteLog("---------New Game-------------------------------");
		}
		public void OnUndoSplit(object sender, EventArgs e) {
			currentSplit--;
		}
		public void OnSkipSplit(object sender, EventArgs e) {
			currentSplit++;
		}
		public void OnSplit(object sender, EventArgs e) {
			currentSplit++;
			if (isAutoSplit) {
				try {
					ISegment segment = Model.CurrentState.Run[Model.CurrentState.Run.Count - 1];
					segment.SplitTime = new Time(segment.SplitTime.RealTime.Value.Subtract(TimeSpan.FromSeconds(5.1)), segment.SplitTime.GameTime.Value.Subtract(TimeSpan.FromSeconds(5.1)));
				} catch { }
			}
		}
		public Control GetSettingsControl(LayoutMode mode) { return settings; }
		public void SetSettings(XmlNode document) { settings.SetSettings(document); }
		public XmlNode GetSettings(XmlDocument document) { return settings.UpdateSettings(document); }
		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
#endif
		public float HorizontalWidth { get { return 0; } }
		public float MinimumHeight { get { return 0; } }
		public float MinimumWidth { get { return 0; } }
		public float PaddingBottom { get { return 0; } }
		public float PaddingLeft { get { return 0; } }
		public float PaddingRight { get { return 0; } }
		public float PaddingTop { get { return 0; } }
		public float VerticalHeight { get { return 0; } }
		public void Dispose() { }
	}
}