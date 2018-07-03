using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneComponent
{
    public class SceneEntity
    {
        private const string PREFAB_PATH = "Assets/Plugins/SceneComponent/Prefabs";
        private const string PREFAB_FORMAT = "{0}/{1}.prefab";
        private const string COMPONENT_CONTAINER = "SceneComponentContainer";

        private static GameObject container;

        [InitializeOnLoadMethod]
        static void CreatePrefabFolder()
        {
            Directory.CreateDirectory(PREFAB_PATH);
        }

#if UNITY_EDITOR
        public static GameObject CreateScenePrefab(string scenePath, params System.Type[] components)
        {
            var guid = AssetDatabase.AssetPathToGUID(scenePath);
            var prefabPath = string.Format(PREFAB_FORMAT, PREFAB_PATH, guid);
            
            var go = new GameObject(guid, components);
            
            var prefab = PrefabUtility.CreatePrefab(prefabPath, go);
            
            Object.DestroyImmediate(go);
            
            return prefab;
        }
#endif

        public static GameObject GetScenePrefab(string scenePath)
        {
            var guid = AssetDatabase.AssetPathToGUID(scenePath);
            var prefabPath = string.Format(PREFAB_FORMAT, PREFAB_PATH, guid);
            
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        [PostProcessScene]
        static void OnPostProcessScene()
        {
            var scenePath = SceneManager.GetActiveScene().path;

            if (string.IsNullOrEmpty(scenePath))
                return;

            var prefab = GetScenePrefab(scenePath);

            if (prefab)
            {
                container = Object.Instantiate(prefab);
                container.name = COMPONENT_CONTAINER;
            }
            else
            {
                container = null;
            }
        }

        public static T[] GetComponents<T>()
        {
            if (container == null)
                return null;

            return container.GetComponents<T>();
        }

        public static T GetComponent<T>()
        {
            if (container == null)
                return default(T);

            return container.GetComponent<T>();
        }
    }
}
