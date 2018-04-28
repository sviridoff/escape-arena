using UnityEditor;

namespace Pathfinding {
	[CustomEditor(typeof(RaycastModifier))]
	[CanEditMultipleObjects]
	public class RaycastModifierEditor : EditorBase {
		protected override void Inspector () {
			PropertyField("iterations");
			ClampInt("iterations", 0);

			if (PropertyField("useRaycasting")) {
				EditorGUI.indentLevel++;

				PropertyField("use2DPhysics");
				if (PropertyField("thickRaycast")) {
					EditorGUI.indentLevel++;
					PropertyField("thickRaycastRadius");
					Clamp("thickRaycastRadius", 0f);
					EditorGUI.indentLevel--;
				}

				PropertyField("raycastOffset");
				PropertyField("mask", "Layer Mask");
				EditorGUI.indentLevel--;
			}

			PropertyField("useGraphRaycasting");
			if (FindProperty("useGraphRaycasting").boolValue) {
				EditorGUILayout.HelpBox("Graph raycasting is only available in the pro version for all built-in graphs.", MessageType.Info);
			}
			PropertyField("subdivideEveryIter", "Subdivide Every Iteration");
		}
	}
}
