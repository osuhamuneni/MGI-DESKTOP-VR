using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Galaxy.Manager.Helper
{
    public class GalaxyCacheHelper : MonoBehaviour
    {
        public string location;
        public List<string> fitsDirs = new List<string>();

        private void OnValidate()
        {
            location = Application.persistentDataPath;
            string[] dirs = Directory.GetDirectories(location);
            fitsDirs = dirs.ToList();
        }

        public void RefreshCache()
        {
            location = Application.persistentDataPath;
            string[] dirs = Directory.GetDirectories(location);
            fitsDirs = dirs.ToList();
        }

        public void ClearCache()
        {
            string[] dirs = Directory.GetDirectories(location);
            fitsDirs.Clear();
            fitsDirs = new List<string>();
            fitsDirs = dirs.ToList();
            foreach (string dir in dirs)
            {
                Directory.Delete(dir,true);
            }

            fitsDirs.Clear();
            dirs = null;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(GalaxyCacheHelper))]
    public class GalaxyCacheEditor : Editor
    {
        GalaxyCacheHelper galaxyCache;
        private void OnEnable()
        {
            galaxyCache = (GalaxyCacheHelper)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Refresh Cache"))
            {
                galaxyCache.RefreshCache();
            }

            if (GUILayout.Button("Clear Cache"))
            {
                galaxyCache.ClearCache();
            }
        }
    }
#endif
}