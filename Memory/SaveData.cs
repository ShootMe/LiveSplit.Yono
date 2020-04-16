namespace LiveSplit.Yono {
    public class SaveData {
        public string Key;
        public string Value;
        public override bool Equals(object obj) {
            return obj is SaveData save && save.Key == Key && save.Value == Value;
        }
        public override int GetHashCode() {
            return Key.GetHashCode();
        }
        public override string ToString() {
            return $"{Key}={Value}";
        }
    }
}