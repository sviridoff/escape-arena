using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Pathfinding {
	/** Helper for creating editors */
	[CustomEditor(typeof(VersionedMonoBehaviour), true)]
	public class EditorBase : Editor {
		static System.Collections.Generic.Dictionary<string, string> cachedTooltips;
		static System.Collections.Generic.Dictionary<string, string> cachedURLs;
		Dictionary<string, SerializedProperty> props = new Dictionary<string, SerializedProperty>();
		Dictionary<string, string> localTooltips = new Dictionary<string, string>();

		static GUIContent content = new GUIContent();
		static GUIContent showInDocContent = new GUIContent("Show in online documentation", "");
		static GUILayoutOption[] noOptions = new GUILayoutOption[0];

		static void LoadMeta () {
			if (cachedTooltips == null) {
				var filePath = EditorResourceHelper.editorAssets + "/tooltips.tsv";

				try {
					cachedURLs = System.IO.File.ReadAllLines(filePath).Select(l => l.Split('\t')).Where(l => l.Length == 2).ToDictionary(l => l[0], l => l[1]);
					cachedTooltips = new System.Collections.Generic.Dictionary<string, string>();
				} catch {
					cachedURLs = new System.Collections.Generic.Dictionary<string, string>();
					cachedTooltips = new System.Collections.Generic.Dictionary<string, string>();
				}
			}
		}

		static string FindURL (System.Type type, string path) {
			// Find the correct type if the path was not an immediate member of #type
			while (true) {
				var index = path.IndexOf('.');
				if (index == -1) break;
				var fieldName = path.Substring(0, index);
				var remaining = path.Substring(index + 1);
				var field = type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
				if (field != null) {
					type = field.FieldType;
					path = remaining;
				} else {
					// Could not find the correct field
					return null;
				}
			}

			// Find a documentation entry for the field, fall back to parent classes if necessary
			while (type != null) {
				var url = FindURL(type.FullName + "." + path);
				if (url != null) return url;
				type = type.BaseType;
			}
			return null;
		}

		static string FindURL (string path) {
			LoadMeta();
			string url;
			cachedURLs.TryGetValue(path, out url);
			return url;
		}

		static string FindTooltip (string path) {
			LoadMeta();

			string tooltip;
			cachedTooltips.TryGetValue(path, out tooltip);
			return tooltip;
		}

		string FindLocalTooltip (string path) {
			string result;

			if (!localTooltips.TryGetValue(path, out result)) {
				var fullPath = target.GetType().Name + "." + path;
				result = localTooltips[path] = FindTooltip(fullPath);
			}
			return result;
		}

		protected virtual void OnEnable () {
			foreach (var target in targets) (target as IVersionedMonoBehaviourInternal).OnUpgradeSerializedData(int.MaxValue, true);
		}

		public sealed override void OnInspectorGUI () {
			EditorGUI.indentLevel = 0;
			serializedObject.Update();
			Inspector();
			serializedObject.ApplyModifiedProperties();
		}

		protected virtual void Inspector () {
			// Basically the same as DrawDefaultInspector, but with tooltips
			bool enterChildren = true;

			for (var prop = serializedObject.GetIterator(); prop.NextVisible(enterChildren); enterChildren = false) {
				PropertyField(prop.propertyPath);
			}
		}

		protected SerializedProperty FindProperty (string name) {
			SerializedProperty res;

			if (!props.TryGetValue(name, out res)) res = props[name] = serializedObject.FindProperty(name);
			if (res == null) throw new System.ArgumentException(name);
			return res;
		}

		protected bool PropertyField (string propertyPath, string label = null, string tooltip = null) {
			return PropertyField(FindProperty(propertyPath), label, tooltip, propertyPath);
		}

		protected bool PropertyField (SerializedProperty prop, string label = null, string tooltip = null) {
			return PropertyField(prop, label, tooltip, prop.propertyPath);
		}

		bool PropertyField (SerializedProperty prop, string label, string tooltip, string propertyPath) {
			content.text = label ?? prop.displayName;
			content.tooltip = tooltip ?? FindTooltip(propertyPath);
			var contextClick = Event.current.type == EventType.ContextClick;
			EditorGUILayout.PropertyField(prop, content, true, noOptions);
			if (contextClick && Event.current.type == EventType.Used) {
				var url = FindURL(target.GetType(), propertyPath);
				if (url != null) {
					Event.current.Use();
					var menu = new GenericMenu();
					menu.AddItem(showInDocContent, false, () => Application.OpenURL(AstarUpdateChecker.GetURL("documentation") + url));
					menu.ShowAsContext();
				}
			}
			return prop.propertyType == SerializedPropertyType.Boolean ? !prop.hasMultipleDifferentValues && prop.boolValue : true;
		}

		protected void IntSlider (string name, int left, int right) {
			var prop = FindProperty(name);

			content.text = prop.displayName;
			content.tooltip = FindTooltip(name);
			EditorGUILayout.IntSlider(prop, left, right, content, noOptions);
		}

		protected void Clamp (string name, float min, float max = float.PositiveInfinity) {
			var prop = FindProperty(name);

			if (!prop.hasMultipleDifferentValues) prop.floatValue = Mathf.Clamp(prop.floatValue, min, max);
		}

		protected void ClampInt (string name, int min, int max = int.MaxValue) {
			var prop = FindProperty(name);

			if (!prop.hasMultipleDifferentValues) prop.intValue = Mathf.Clamp(prop.intValue, min, max);
		}
	}
}
