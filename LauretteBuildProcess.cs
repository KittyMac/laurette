using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LauretteBuildProcess : AssetPostprocessor  {

	private static string OutputDirectory = "Assets/Code/";

	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		bool stringsFileWasChanged = false;

		foreach (string str in importedAssets) {
			stringsFileWasChanged = stringsFileWasChanged | str.EndsWith (".strings");
		}
		foreach (string str in deletedAssets) {
			stringsFileWasChanged = stringsFileWasChanged | str.EndsWith (".strings");
		}

		for (var i = 0; i < movedAssets.Length; i++) {
			stringsFileWasChanged = stringsFileWasChanged | movedAssets [i].EndsWith (".strings");
			stringsFileWasChanged = stringsFileWasChanged | movedFromAssetPaths [i].EndsWith (".strings");
		}

		if (stringsFileWasChanged) {
			Laurette laurette = new Laurette ();

			string[] allAssets = AssetDatabase.GetAllAssetPaths();
			List<string> allStringsFiles = new List<string> ();

			foreach (string asset in allAssets) {
				if (asset.EndsWith (".strings") || asset.EndsWith (".strings.txt")) {
					allStringsFiles.Add (asset);
				}
			}

			string error = null;
			laurette.Process (allStringsFiles.ToArray(), OutputDirectory+"/Localizations.cs", out error);
		}
	}
}