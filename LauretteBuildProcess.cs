using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class LauretteBuildProcess : AssetPostprocessor  {

	private static string OutputDirectory = "Assets/";

	static LauretteBuildProcess() {
		// Check if the output file exists, if not we should probably try generating it!
		if (File.Exists (OutputDirectory + "/Localizations.cs") == false) {
			ProcessStringsFiles ();
		}
	}

	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		bool stringsFileWasChanged = false;

		// Check if the output file exists, if not we should probably try generating it!
		if (File.Exists (OutputDirectory + "/Localizations.cs") == false) {
			ProcessStringsFiles ();
		}

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
			ProcessStringsFiles ();
		}
	}

	static void ProcessStringsFiles() {
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