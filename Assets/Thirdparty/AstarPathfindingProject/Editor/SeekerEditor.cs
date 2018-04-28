using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Pathfinding {
	[CustomEditor(typeof(Seeker))]
	[CanEditMultipleObjects]
	public class SeekerEditor : EditorBase {
		static bool tagPenaltiesOpen;
		static List<Seeker> scripts = new List<Seeker>();

		protected override void Inspector () {
			base.Inspector();

			scripts.Clear();
			foreach (var script in targets) scripts.Add(script as Seeker);

			Undo.RecordObjects(targets, "Modify settings on Seeker");

			var startEndModifierProp = FindProperty("startEndModifier");
			startEndModifierProp.isExpanded = EditorGUILayout.Foldout(startEndModifierProp.isExpanded, startEndModifierProp.displayName);
			if (startEndModifierProp.isExpanded) {
				EditorGUI.indentLevel++;
				PropertyField("startEndModifier.exactStartPoint", "Start Point Snapping");
				PropertyField("startEndModifier.exactEndPoint", "End Point Snapping");
				PropertyField("startEndModifier.addPoints", "Add Points");

				if (PropertyField("startEndModifier.useRaycasting", "Physics Raycasting")) {
					EditorGUI.indentLevel++;
					PropertyField("startEndModifier.mask", "Layer Mask");
					EditorGUI.indentLevel--;
					EditorGUILayout.HelpBox("Using raycasting to snap the start/end points has largely been superseded by the 'ClosestOnNode' snapping option. It is both faster and usually closer to what you want to achieve.", MessageType.Info);
				}

				if (PropertyField("startEndModifier.useGraphRaycasting", "Graph Raycasting")) {
					EditorGUILayout.HelpBox("Using raycasting to snap the start/end points has largely been superseded by the 'ClosestOnNode' snapping option. It is both faster and usually closer to what you want to achieve.", MessageType.Info);
				}

				EditorGUI.indentLevel--;
			}

			tagPenaltiesOpen = EditorGUILayout.Foldout(tagPenaltiesOpen, new GUIContent("Tags", "Settings for each tag"));
			if (tagPenaltiesOpen) {
				EditorGUI.indentLevel++;
				string[] tagNames = AstarPath.FindTagNames();
				if (tagNames.Length != 32) {
					tagNames = new string[32];
					for (int i = 0; i < tagNames.Length; i++) tagNames[i] = "" + i;
				}

				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("Tag", EditorStyles.boldLabel, GUILayout.MaxWidth(120));
				for (int i = 0; i < tagNames.Length; i++) {
					EditorGUILayout.LabelField(tagNames[i], GUILayout.MaxWidth(120));
				}

				// Make sure the arrays are all of the correct size
				for (int i = 0; i < scripts.Count; i++) {
					if (scripts[i].tagPenalties == null || scripts[i].tagPenalties.Length != tagNames.Length) scripts[i].tagPenalties = new int[tagNames.Length];
				}

				if (GUILayout.Button("Edit names", EditorStyles.miniButton)) {
					AstarPathEditor.EditTags();
				}
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("Penalty", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
				var prop = FindProperty("tagPenalties").FindPropertyRelative("Array");
				prop.Next(true);
				for (int i = 0; i < tagNames.Length; i++) {
					prop.Next(false);
					EditorGUILayout.PropertyField(prop, GUIContent.none, false, GUILayout.MinWidth(100));
					// Penalties should not be negative
					if (prop.intValue < 0) prop.intValue = 0;
				}
				if (GUILayout.Button("Reset all", EditorStyles.miniButton)) {
					for (int i = 0; i < tagNames.Length; i++) {
						for (int j = 0; j < scripts.Count; j++) {
							scripts[j].tagPenalties[i] = 0;
						}
					}
				}
				EditorGUILayout.EndVertical();

				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("Traversable", EditorStyles.boldLabel, GUILayout.MaxWidth(100));
				for (int i = 0; i < tagNames.Length; i++) {
					var anyFalse = false;
					var anyTrue = false;
					for (int j = 0; j < scripts.Count; j++) {
						var prevTraversable = ((scripts[j].traversableTags >> i) & 0x1) != 0;
						anyTrue |= prevTraversable;
						anyFalse |= !prevTraversable;
					}
					EditorGUI.BeginChangeCheck();
					EditorGUI.showMixedValue = anyTrue & anyFalse;
					var newTraversable = EditorGUILayout.Toggle(anyTrue);
					EditorGUI.showMixedValue = false;
					if (EditorGUI.EndChangeCheck()) {
						for (int j = 0; j < scripts.Count; j++) {
							scripts[j].traversableTags = (scripts[j].traversableTags & ~(1 << i)) | ((newTraversable ? 1 : 0) << i);
						}
					}
				}

				if (GUILayout.Button("Set all/none", EditorStyles.miniButton)) {
					for (int j = scripts.Count - 1; j >= 0; j--) {
						scripts[j].traversableTags = (scripts[0].traversableTags & 0x1) == 0 ? -1 : 0;
					}
				}
				EditorGUILayout.EndVertical();

				EditorGUILayout.EndHorizontal();
			}
		}
	}
}
