using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace LiveSplit.Yono {
	public class SplitterMemory {
		private static ProgramPointer SceneManager = new ProgramPointer(AutoDeref.Double, new ProgramSignature(PointerVersion.V1, "8B0D????????568BF185C974088B018B106A00FFD26A5856E8", 2));
		private static ProgramPointer GlobalSaveData = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "893883EC0C57E8????????83C41083EC0C50E8????????83C4108B4714", -4));
		private static ProgramPointer PhantBlinking = new ProgramPointer(AutoDeref.None, 0xeab540);
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public SplitterMemory() {
			lastHooked = DateTime.MinValue;
		}

		public EyeType Eyes() {
			float eyes = PhantBlinking.Read<float>(Program, 0x44, 0x14);
			if (eyes < 0.2) {
				return EyeType.Normal;
			} else if (eyes < 0.4) {
				return EyeType.Closed;
			} else if (eyes < 0.7) {
				return EyeType.Happy;
			}
			return EyeType.Dead;
		}
		public void SetEyes(EyeType eyes) {
			PhantBlinking.Write<float>(Program, 99999999f, 0x44, 0x1c);
			float offset = 0;
			switch (eyes) {
				case EyeType.Closed: offset = 0.25f; break;
				case EyeType.Happy: offset = 0.5f; break;
				case EyeType.Dead: offset = 0.75f; break;
			}
			PhantBlinking.Write<float>(Program, offset, 0x44, 0x14);
		}
		public int SaveDataCount() {
			return GlobalSaveData.Read<int>(Program, 0x0, 0x10, 0x8, 0xc);
		}
		public string SaveData(string key) {
			IntPtr saveEntries = (IntPtr)GlobalSaveData.Read<uint>(Program, 0x0, 0x10, 0x8);
			int length = Program.Read<int>(saveEntries, 0xc);
			for (int i = 0; i < length; i++) {
				IntPtr item = (IntPtr)Program.Read<uint>(saveEntries, 0x10 + (i * 4));
				if (item == IntPtr.Zero) { continue; }

				string name = Program.Read((IntPtr)Program.Read<uint>(item, 0x8));
				string identity = Program.Read((IntPtr)Program.Read<uint>(item, 0xc));
				if (key.Equals($"{name}({identity})", StringComparison.OrdinalIgnoreCase)) {
					return GetValue(item, name);
				}
			}
			return string.Empty;
		}
		public List<SaveData> SaveData() {
			IntPtr saveEntries = (IntPtr)GlobalSaveData.Read<uint>(Program, 0x0, 0x10, 0x8);
			List<SaveData> data = new List<SaveData>();
			int length = Program.Read<int>(saveEntries, 0xc);
			for (int i = 0; i < length; i++) {
				IntPtr item = (IntPtr)Program.Read<uint>(saveEntries, 0x10 + (i * 4));
				if (item == IntPtr.Zero) { continue; }

				string name = Program.Read((IntPtr)Program.Read<uint>(item, 0x8));
				string identity = Program.Read((IntPtr)Program.Read<uint>(item, 0xc));

				data.Add(new SaveData() {
					Key = $"{name}({identity})",
					Value = GetValue(item, name)
				});
			}
			return data;
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
					return Program.Read((IntPtr)Program.Read<uint>(item, 0x10));
				default:
					return Program.Read<int>(item, 0x10, 0x8).ToString();
			}
		}
		public bool IsLoading() {
			return !IsHooked || SceneManager.Read<int>(Program, 0xc) > 1 || SceneManager.Read<int>(Program, 0x20) > 0;
		}
		public string SceneName() {
			string name = SceneManager.Read(Program, (IntPtr)(SceneManager.Read<uint>(Program, 0x14, 0x20)));
			if (string.IsNullOrEmpty(name)) {
				return SceneManager.Read(Program, (IntPtr)(SceneManager.Read<uint>(Program, 0x14) + 0x20));
			}
			return name;
		}
		public bool HookProcess() {
			IsHooked = Program != null && !Program.HasExited;
			if (!IsHooked && DateTime.Now > lastHooked.AddSeconds(1)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("Yono and the Celestial Elephants");
				Program = processes.Length == 0 ? null : processes[0];
				if (Program != null) {
					MemoryReader.Update64Bit(Program);
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
	public enum PointerVersion {
		V1
	}
	public enum AutoDeref {
		None,
		Single,
		Double
	}
	public class ProgramSignature {
		public PointerVersion Version { get; set; }
		public string Signature { get; set; }
		public int Offset { get; set; }
		public ProgramSignature(PointerVersion version, string signature, int offset) {
			Version = version;
			Signature = signature;
			Offset = offset;
		}
		public override string ToString() {
			return Version.ToString() + " - " + Signature;
		}
	}
	public class ProgramPointer {
		private int lastID;
		private DateTime lastTry;
		private ProgramSignature[] signatures;
		private int[] offsets;
		public IntPtr Pointer { get; private set; }
		public PointerVersion Version { get; private set; }
		public AutoDeref AutoDeref { get; private set; }

		public ProgramPointer(AutoDeref autoDeref, params ProgramSignature[] signatures) {
			AutoDeref = autoDeref;
			this.signatures = signatures;
			lastID = -1;
			lastTry = DateTime.MinValue;
		}
		public ProgramPointer(AutoDeref autoDeref, params int[] offsets) {
			AutoDeref = autoDeref;
			this.offsets = offsets;
			lastID = -1;
			lastTry = DateTime.MinValue;
		}

		public T Read<T>(Process program, params int[] offsets) where T : struct {
			GetPointer(program);
			return program.Read<T>(Pointer, offsets);
		}
		public string Read(Process program, IntPtr ptr) {
			GetPointer(program);
			return program.ReadAscii(ptr);
		}
		public byte[] ReadBytes(Process program, int length, params int[] offsets) {
			GetPointer(program);
			return program.Read(Pointer, length, offsets);
		}
		public void Write<T>(Process program, T value, params int[] offsets) where T : struct {
			GetPointer(program);
			program.Write<T>(Pointer, value, offsets);
		}
		public void Write(Process program, byte[] value, params int[] offsets) {
			GetPointer(program);
			program.Write(Pointer, value, offsets);
		}
		public IntPtr GetPointer(Process program) {
			if (program == null) {
				Pointer = IntPtr.Zero;
				lastID = -1;
				return Pointer;
			} else if (program.Id != lastID) {
				Pointer = IntPtr.Zero;
				lastID = program.Id;
			}

			if (Pointer == IntPtr.Zero && DateTime.Now > lastTry.AddSeconds(1)) {
				lastTry = DateTime.Now;

				Pointer = GetVersionedFunctionPointer(program);
				if (Pointer != IntPtr.Zero) {
					if (AutoDeref != AutoDeref.None) {
						Pointer = (IntPtr)program.Read<uint>(Pointer);
						if (AutoDeref == AutoDeref.Double) {
							if (MemoryReader.is64Bit) {
								Pointer = (IntPtr)program.Read<ulong>(Pointer);
							} else {
								Pointer = (IntPtr)program.Read<uint>(Pointer);
							}
						}
					}
				}
			}
			return Pointer;
		}
		private IntPtr GetVersionedFunctionPointer(Process program) {
			if (signatures != null) {
				MemorySearcher searcher = new MemorySearcher();
				for (int i = 0; i < signatures.Length; i++) {
					ProgramSignature signature = signatures[i];

					IntPtr ptr = searcher.FindSignature(program, signature.Signature);
					if (ptr != IntPtr.Zero) {
						Version = signature.Version;
						return ptr + signature.Offset;
					}
				}

				return IntPtr.Zero;
			}

			return (IntPtr)program.Read<uint>(program.MainModule.BaseAddress, offsets);
		}
	}
}