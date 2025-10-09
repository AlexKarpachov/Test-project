#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom Editor script for the AlignToGround component.
/// Adds a "Align to Ground" button to the Inspector for easy alignment in the Editor.
/// </summary>
[CustomEditor(typeof(AlignToGround))]
[CanEditMultipleObjects]  
public class AlignToGroundEditor : Editor
{
    /// <summary>
    /// Overrides the default Inspector GUI.
    /// Draws the standard inspector fields, adds spacing, and a button to trigger alignment.
    /// When the button is clicked, it aligns all selected targets with Undo support.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.Space();  // Add some vertical space before the custom button

        // Custom button for alignment
        if (GUILayout.Button("Align to Ground"))
        {
            foreach (Object o in targets)
            {
                // Cast to AlignToGround component
                AlignToGround atg = o as AlignToGround;
                if (atg == null) continue;  

                // Record the transform for Undo (allows Ctrl+Z to revert)
                Undo.RecordObject(atg.transform, "Align To Ground");

                RaycastHit hit;  
                if (!atg.TryAlign(out hit))
                {
                    Debug.LogWarning("AlignToGround: raycast hit nothing under " + atg.name, atg);
                }

                // Mark the transform as dirty to ensure changes are saved
                EditorUtility.SetDirty(atg.transform);
            }
        }
    }
}
#endif
