using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LauretteBuildProcess : MonoBehaviour {

	private static string OutputDirectory = "Assets/Code/";
	
	[MenuItem ("Laurette/Process All")]
	static void ProcessAllStringsFiles() {
		Laurette laurette = new Laurette ();

		string[] allAssets = AssetDatabase.GetAllAssetPaths();
		List<string> allStringsFiles = new List<string> ();

		foreach (string asset in allAssets) {
			if (asset.EndsWith (".strings")) {
				allStringsFiles.Add (asset);
			}
		}

		string error = null;
		laurette.Process (allStringsFiles.ToArray(), OutputDirectory+"/Localizations.cs", out error);
	}
	
}