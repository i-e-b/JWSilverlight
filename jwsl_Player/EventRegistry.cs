using System.Collections.Generic;

namespace JwslPlayer {
	public class EventRegistry {
		readonly Dictionary<string, List<string>> registry;

		public EventRegistry() {
			registry = new Dictionary<string, List<string>>();
		}

		public IEnumerable<string> this[string eventType] {
			get {
				if (registry.ContainsKey(eventType)) return registry[eventType] ?? new List<string>();
				return new List<string>();
			}
		}

		public void Bind (string eventType, string action) {
			if (!registry.ContainsKey(eventType)) registry.Add(eventType, new List<string>());
			registry[eventType].Add(action);
		}

		public void Unbind(string eventType, string action) {
			if (!registry.ContainsKey(eventType)) return;
			registry[eventType].Remove(action);
		}
	}
}