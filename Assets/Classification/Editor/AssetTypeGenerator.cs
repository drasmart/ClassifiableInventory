using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Classification
{
    public class AssetTypeGenerator : Editor
    {
        private static string templatesPath { get { return Application.dataPath + "/Classification/Editor/"; } }

        [MenuItem("Assets/Create/Classification/New TypeAsset Script", false, 0)]
        public static void CreateStory()
        {
            string startPath = null;
            var obj = Selection.activeObject;
            if (obj != null)
            {
                startPath = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            }
            string path = EditorUtility.SaveFilePanelInProject("Generate TypeAsset", "NewTypeAsset", "cs", "Save generated TypeAsset", startPath);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("No name specified");
                return;
            }

            string mainFile = path.EndsWith("TypeAsset.cs") ? path : path.Replace(".cs", "TypeAsset.cs");
            string filterFile = path.Replace("TypeAsset.cs", "TypeFilterAsset.cs");
            string className = System.IO.Path.GetFileNameWithoutExtension(mainFile).Replace("TypeAsset", "");

            Clone("SimpleTypeAsset.txt", className, mainFile);
            Clone("SimpleTypeFilterAsset.txt", className, filterFile);

            AssetDatabase.ImportAsset(mainFile);
            AssetDatabase.ImportAsset(filterFile);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        }

        private static void Clone(string srcFileName, string dstTypeName, string dstPath)
        {
            string templatePath = templatesPath + srcFileName;

            // So copy all the template lines, replace the template names, and write them to the new file.
            string[] lines = File.ReadAllLines(templatePath);

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Replace("Simple", dstTypeName);
            }

            File.WriteAllLines(dstPath, lines);
        }
    }
}
