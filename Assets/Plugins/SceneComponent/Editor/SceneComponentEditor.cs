using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SceneComponent
{
    [CustomEditor(typeof(SceneAsset))]
    public class SceneComponentEditor : Editor
    {
        private GameObject scenePrefab;

        Dictionary<Editor, bool> activeEditors = new Dictionary<Editor, bool>();

        void OnEnable()
        {
            Undo.undoRedoPerformed += InitActiveEditors;

            var assetPath = AssetDatabase.GetAssetPath(target);

            scenePrefab = SceneEntity.GetScenePrefab(assetPath);

            if (scenePrefab == null)
                scenePrefab = SceneEntity.CreateScenePrefab(assetPath);

            InitActiveEditors();
        }

        void OnDisable()
        {
            ClearActiveEditors();

            AssetDatabase.SaveAssets();
        }

        void InitActiveEditors()
        {
            ClearActiveEditors();

            foreach (var component in scenePrefab.GetComponents<Component>())
            {
                if (component == null)
                    continue;

                if (component is Transform)
                    continue;

                activeEditors.Add(CreateEditor(component), true);
            }
        }

        void ClearActiveEditors()
        {
            foreach (var activeEditor in activeEditors)
                DestroyImmediate(activeEditor.Key);

            activeEditors.Clear();
        }

        public override void OnInspectorGUI()
        {
            GUI.enabled = true;

            var editors = new List<Editor>(activeEditors.Keys);

            DrawInspector(editors);

            Interact(editors);
        }

        void DrawInspector(List<Editor> editors)
        {
            foreach (var editor in editors)
                DrawEditor(editor);
        }

        void DrawEditor(Editor editor)
        {
            DrawEditorTitle(editor);
            DrawEditorBody(editor);
        }

        void DrawEditorTitle(Editor editor)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20));
            rect.x = 0;
            rect.y -= 5;
            rect.width += 20;

            activeEditors[editor] = EditorGUI.InspectorTitlebar(rect, activeEditors[editor], editor.target, true);

            GUILayout.Space(-5f);
        }

        void DrawEditorBody(Editor editor)
        {
            if (activeEditors[editor] && editor.target != null)
                editor.OnInspectorGUI();

            EditorGUILayout.Space();
        }

        void Interact(List<Editor> editors)
        {
            if (editors.Any(e => e.target == null))
            {
                InitActiveEditors();
                Repaint();
            }

            var dragAndDropRect = GUILayoutUtility.GetRect(GUIContent.none,
                                                           GUIStyle.none,
                                                           GUILayout.ExpandHeight(true),
                                                           GUILayout.MinHeight(200));

            if (!dragAndDropRect.Contains(Event.current.mousePosition))
                return;

            switch (Event.current.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        var components = DragAndDrop.objectReferences
                                                    .Where(x => x is MonoScript)
                                                    .OfType<MonoScript>()
                                                    .Select(m => m.GetClass());

                        foreach (var component in components)
                            Undo.AddComponent(scenePrefab, component);

                        InitActiveEditors();
                    }

                    break;
            }
        }
    }
}
