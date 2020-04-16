﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using LiveSplit.View;
namespace LiveSplit.Yono {
    public class Component : IComponent {
        public string ComponentName { get { return Factory.AutosplitterName; } }
        public TimerModel Model { get; set; }
        public IDictionary<string, Action> ContextMenuControls { get { return null; } }
        private LogicManager logic;
        private UserSettings userSettings;
        private LogManager log;
        private bool isRunning = false;
        private bool shouldLog = false;
        private bool isAutosplitting = false;
        private RunEditorDialog editorDialog;
        private System.ComponentModel.BindingList<ISegment> segmentList;
        private Split oldSplit;
        private DateTime lastRemovedSplit;
#if Console
        public static void Main(string[] args) {
            Component component = new Component(new LiveSplitState());
            component.log.EnableLogging = true;
            Application.Run();
        }
#endif
        public Component(LiveSplitState state) {
            log = new LogManager();
            userSettings = new UserSettings(state, log);
            logic = new LogicManager(userSettings.Settings);

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

            StartAutosplitter();
        }

        public void WaitForLogic() {
            lock (logic) {
                while (!shouldLog && isRunning) { Monitor.Wait(logic); }
                shouldLog = false;
            }
        }
        public void PulseLog() {
            lock (logic) {
                shouldLog = true;
                Monitor.PulseAll(logic);
            }
        }
        public void StartAutosplitter() {
            if (isRunning) { return; }
            isRunning = true;

            Task.Factory.StartNew(delegate () {
                try {
                    while (isRunning) {
                        try {
                            if (logic.IsHooked()) {
                                int currentSplit = Model.CurrentState.CurrentPhase == TimerPhase.NotRunning ? 0 : Model.CurrentState.CurrentSplitIndex + 1;
                                logic.Update(currentSplit);
                                PulseLog();
                            }
                            HandleLogic();
                        } catch (Exception ex) {
                            log.AddEntry(new EventLogEntry(ex.ToString()));
                        }
                        Thread.Sleep(7);
                    }
                } catch { }
            }, TaskCreationOptions.LongRunning);

            Task.Factory.StartNew(delegate () {
                try {
                    while (isRunning) {
                        try {
                            WaitForLogic();
                            log.Update(logic, userSettings.Settings);
                        } catch (Exception ex) {
                            log.AddEntry(new EventLogEntry(ex.ToString()));
                        }
                    }
                } catch { }
            }, TaskCreationOptions.LongRunning);
        }
        private void HandleLogic() {
            if (Model == null) { return; }

            Model.CurrentState.IsGameTimePaused = logic.Paused;
            if (logic.GameTime >= 0) {
                Model.CurrentState.SetGameTime(TimeSpan.FromSeconds(logic.GameTime));
            }

            if (logic.ShouldReset) {
                isAutosplitting = true;
                Model.Reset();
            } else if (logic.ShouldSplit) {
                isAutosplitting = true;
                if (Model.CurrentState.CurrentPhase == TimerPhase.NotRunning) {
                    Model.Start();
                } else {
                    Model.Split();
                }
            }
            isAutosplitting = false;
        }
        public void OnReset(object sender, TimerPhase e) {
            logic.Reset();
            if (!isAutosplitting) {
                log.AddEntry(new EventLogEntry("Reset Splits Manual"));
            } else {
                log.AddEntry(new EventLogEntry("Reset Splits Auto"));
            }
        }
        public void OnResume(object sender, EventArgs e) {
            log.AddEntry(new EventLogEntry("Resumed Splits"));
        }
        public void OnPause(object sender, EventArgs e) {
            log.AddEntry(new EventLogEntry("Paused Splits"));
        }
        public void OnStart(object sender, EventArgs e) {
            Model.CurrentState.IsGameTimePaused = true;
            Model.CurrentState.SetGameTime(TimeSpan.Zero);
            if (!isAutosplitting) {
                logic.Increment();
                log.AddEntry(new EventLogEntry("Started Splits Manual " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));
            } else {
                log.AddEntry(new EventLogEntry("Started Splits Auto " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));
            }
        }
        public void OnUndoSplit(object sender, EventArgs e) {
            logic.Decrement();
            log.AddEntry(new EventLogEntry($"Undo Current Split {Model.CurrentState.CurrentTime.RealTime.Value}"));
        }
        public void OnSkipSplit(object sender, EventArgs e) {
            logic.Increment();
            log.AddEntry(new EventLogEntry($"Skip Current Split {Model.CurrentState.CurrentTime.RealTime.Value}"));
        }
        public void OnSplit(object sender, EventArgs e) {
            if (!isAutosplitting) {
                logic.Increment();
                log.AddEntry(new EventLogEntry($"Split Manual {Model.CurrentState.CurrentTime.RealTime.Value}"));
            } else {
                log.AddEntry(new EventLogEntry($"Split Auto {Model.CurrentState.CurrentTime.RealTime.Value}"));
                if (logic.CurrentSplit > Model.CurrentState.Run.Count) {
                    try {
                        ISegment segment = Model.CurrentState.Run[Model.CurrentState.Run.Count - 1];
                        segment.SplitTime = new Time(Model.CurrentState.CurrentTime.RealTime.Value.Subtract(TimeSpan.FromSeconds(5.1)), Model.CurrentState.CurrentTime.GameTime.Value.Subtract(TimeSpan.FromSeconds(5.1)));
                    } catch { }
                }
            }
        }
        public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
            if (editorDialog == null && Form.ActiveForm is RunEditorDialog runEditor) {
                PropertyInfo info = typeof(RunEditorDialog).GetProperty("SegmentList", BindingFlags.Instance | BindingFlags.NonPublic);
                segmentList = info.GetValue(runEditor) as System.ComponentModel.BindingList<ISegment>;
                if (segmentList != null) {
                    editorDialog = runEditor;
                    segmentList.ListChanged += SegmentList_ListChanged;
                    runEditor.FormClosed += RunEditor_FormClosed;
                }
            }
        }
        private void RunEditor_FormClosed(object sender, FormClosedEventArgs e) {
            editorDialog.FormClosed -= RunEditor_FormClosed;
            editorDialog = null;
            segmentList.ListChanged -= SegmentList_ListChanged;
            segmentList = null;
        }
        private void SegmentList_ListChanged(object sender, System.ComponentModel.ListChangedEventArgs e) {
            if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemAdded) {
                if (oldSplit != null && DateTime.Now < lastRemovedSplit) {
                    oldSplit.Name = "splitChangedValue";
                    userSettings.Settings.Autosplits.Insert(e.NewIndex + 1, oldSplit);
                    oldSplit = null;
                } else {
                    userSettings.Settings.Autosplits.Insert(e.NewIndex + 1, new Split() { Name = "splitChangedValue", Type = SplitType.ManualSplit });
                }
            } else if (e.ListChangedType == System.ComponentModel.ListChangedType.ItemDeleted) {
                oldSplit = userSettings.Settings.Autosplits[e.NewIndex + 1];
                lastRemovedSplit = DateTime.Now.AddSeconds(0.1);
                userSettings.Settings.Autosplits.RemoveAt(e.NewIndex + 1);
                userSettings.Settings.Autosplits[userSettings.Settings.Autosplits.Count - 1].Name = "splitChangedValue";
            }
        }
        public Control GetSettingsControl(LayoutMode mode) { return userSettings; }
        public void SetSettings(XmlNode document) { userSettings.InitializeSettings(document); }
        public XmlNode GetSettings(XmlDocument document) { return userSettings.UpdateSettings(document); }
        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
        public float HorizontalWidth { get { return 0; } }
        public float MinimumHeight { get { return 0; } }
        public float MinimumWidth { get { return 0; } }
        public float PaddingBottom { get { return 0; } }
        public float PaddingLeft { get { return 0; } }
        public float PaddingRight { get { return 0; } }
        public float PaddingTop { get { return 0; } }
        public float VerticalHeight { get { return 0; } }
        public void Dispose() {
            isRunning = false;
            PulseLog();
            if (Model != null) {
                Model.CurrentState.OnReset -= OnReset;
                Model.CurrentState.OnPause -= OnPause;
                Model.CurrentState.OnResume -= OnResume;
                Model.CurrentState.OnStart -= OnStart;
                Model.CurrentState.OnSplit -= OnSplit;
                Model.CurrentState.OnUndoSplit -= OnUndoSplit;
                Model.CurrentState.OnSkipSplit -= OnSkipSplit;
                Model = null;
            }
            userSettings.Dispose();
        }
    }
}