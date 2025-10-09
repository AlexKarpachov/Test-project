#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AlignToGround))]
[CanEditMultipleObjects]
public class AlignToGroundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();

        if (GUILayout.Button("Align to Ground"))
        {
            foreach (Object o in targets)
            {
                AlignToGround atg = o as AlignToGround;
                if (atg == null) continue;

                Undo.RecordObject(atg.transform, "Align To Ground");
                RaycastHit hit;
                if (!atg.TryAlign(out hit))
                    Debug.LogWarning("AlignToGround: raycast hit nothing under " + atg.name, atg);
                EditorUtility.SetDirty(atg.transform);
            }
        }
    }
}
#endif
