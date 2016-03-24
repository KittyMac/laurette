// The MIT License (MIT)
// 
// Copyright (c) 2016 Rocco Bowling
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// 	copies or substantial portions of the Software.
// 
// 	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// 	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// 	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// 	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// 	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// 	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// 	SOFTWARE.

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
			stringsFileWasChanged = true;
		}

		foreach (string str in importedAssets) {
			stringsFileWasChanged = stringsFileWasChanged | str.EndsWith (".strings");
			stringsFileWasChanged = stringsFileWasChanged | str.EndsWith (".strings.txt");
			stringsFileWasChanged = stringsFileWasChanged | str.EndsWith ("Laurette.cs");
		}
		foreach (string str in deletedAssets) {
			stringsFileWasChanged = stringsFileWasChanged | str.EndsWith (".strings");
			stringsFileWasChanged = stringsFileWasChanged | str.EndsWith (".strings.txt");
		}

		for (var i = 0; i < movedAssets.Length; i++) {
			stringsFileWasChanged = stringsFileWasChanged | movedAssets [i].EndsWith (".strings");
			stringsFileWasChanged = stringsFileWasChanged | movedFromAssetPaths [i].EndsWith (".strings");

			stringsFileWasChanged = stringsFileWasChanged | movedAssets [i].EndsWith (".strings.txt");
			stringsFileWasChanged = stringsFileWasChanged | movedFromAssetPaths [i].EndsWith (".strings.txt");
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

		AssetDatabase.Refresh ();

		Debug.Log ("Laurette was here");
	}
}