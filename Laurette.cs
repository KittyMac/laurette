﻿// The MIT License (MIT)
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

// Laurette is inspired by Laurine for Swift ( https://github.com/JiriTrecak/Laurine ).
// Conceptually, Laurette will process .strings files and generate C# code classes from
// them to allow for easy localization usage in C# code. However, unlike Laurine which
// then utilizes the existing API for translation (ie NSLocalizedString ), Laurette
// will embed all strings in the code it generates. This is to remove the gummy core
// of Laurette from being tied to any one particular environment (like Unity).


using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System;


public class Laurette {

	static private string[] invalidKeywords = {
		"abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
	};

	public bool Process(string[] allFiles, string outputFile, out string error) {

		error = null;

		// 0) set up the LauretteTree, which will hold the hierarchy of our key-value pairs
		LauretteTree tree = new LauretteTree();

		// 1) run through all files and process each one
		foreach (string file in allFiles) {
			if (!ProcessFile (file, tree, out error)) {
				return false;
			}
		}



		// Generate the source code
		StringBuilder sb = new StringBuilder();

	
		AppendTabFormat (sb, 0, "// Autogenerated by Laurette ( https://github.com/KittyMac/laurette )\n\n");
		AppendTabFormat (sb, 0, "using System.Collections.Generic;\n\n");

		tree.Traverse ((branch, isStart, isLeaf, depth) => {

			if(isLeaf){

				bool isFormatString = false;
				int numArguments = 0;

				foreach (string languageCode in tree.AllLanguageCodes) {
					string value = branch.Translate(languageCode);
					if(value.Contains("{0}")) {
						isFormatString = true;

						while(value.Contains("{"+numArguments+"}")) {
							numArguments++;
						}

						break;
					}
				}

				if(isFormatString){

					StringBuilder formatString = new StringBuilder();
					StringBuilder argsString = new StringBuilder();
					for(int i = 0; i < numArguments; i++){
						formatString.Append(string.Format("object arg{0}, ", i));
						argsString.Append(string.Format("arg{0}, ", i));
					}
					formatString.Length = formatString.Length-2;
					argsString.Length = argsString.Length-2;

					AppendTabFormat(sb, depth, "/// <summary>English: \"{0}\"</summary>\n", branch.Translate("en"));
					AppendTabFormat(sb, depth, "public static string {0}({1}) {{\n", branch.Name, formatString.ToString());

					depth++;
					AppendTabFormat(sb, depth, "return string.Format({0}[(int)currentLanguage], {1});\n", branch.Path, argsString.ToString());

					depth--;
					AppendTabFormat(sb, depth, "}}\n");


				}else{


					AppendTabFormat(sb, depth, "/// <summary>English: \"{0}\"</summary>\n", branch.Translate("en"));
					AppendTabFormat(sb, depth, "public static string {0} {{\n", branch.Name);

					depth++;
					AppendTabFormat(sb, depth, "get {{\n");

					depth++;
					AppendTabFormat(sb, depth, "return {0}[(int)currentLanguage];\n", branch.Path);

					depth--;
					AppendTabFormat(sb, depth, "}}\n");

					depth--;
					AppendTabFormat(sb, depth, "}}\n");
				}

			} else {
				if (isStart) {
					AppendTabFormat(sb, depth, "public struct {0} {{\n", branch.Name);

					// Create the LanguageCode enum
					if(depth == 0) {
						depth++;

						AppendTabFormat(sb, depth, "public enum LanguageCode {{\n");

						depth++;
						foreach(string languageCode in tree.AllLanguageCodes) {
							AppendTabFormat(sb, depth, "{0},\n", languageCode);
						}
						depth--;

						AppendTabFormat(sb, depth, "}}\n\n");

						AppendTabFormat(sb, depth, "private static LanguageCode currentLanguage = LanguageCode.{0};\n\n", tree.AllLanguageCodes[0]);

						AppendTabFormat(sb, depth, "public static string GetLanguageCode() {{ return currentLanguage.ToString(); }}\n\n");

						AppendTabFormat(sb, depth, "public static void SetLanguageCode(LanguageCode code) {{ currentLanguage = code; }}\n\n");

						AppendTabFormat(sb, depth, "public static void SetLanguageCode(string codeAsString) {{ currentLanguage = (LanguageCode)System.Enum.Parse(typeof(LanguageCode), codeAsString); }}\n\n");

						depth--;
					}

				} else {
					AppendTabFormat(sb, depth, "}}\n");
				}
			}
		});


		sb.Length = sb.Length - 2;
		sb.AppendFormat ("\n\n\n");


		AppendTabFormat(sb, 1, "private static Dictionary<string,string[]> keyLUT = new Dictionary<string,string[]>();\n\n");
		AppendTabFormat(sb, 1, "public static string TranslateKey(string key) {{\n");
		AppendTabFormat(sb, 2, "if(keyLUT.Count == 0) {{\n");

		tree.Traverse ((branch3, isStart3, isLeaf3, depth3) => {
			if (isLeaf3) {
				AppendTabFormat(sb, 3, "keyLUT[\"{0}\"] = {1};\n", branch3.OriginalKey, branch3.Path);
			}
		});

		AppendTabFormat(sb, 2, "}}\n");
		AppendTabFormat(sb, 2, "if(keyLUT.ContainsKey(key) == false) {{ return null; }};\n");
		AppendTabFormat(sb, 2, "return keyLUT[key][(int)currentLanguage];\n");
		AppendTabFormat(sb, 1, "}}\n");

		tree.Traverse ((branch2, isStart2, isLeaf2, depth2) => {

			if (isLeaf2) {
				AppendTabFormat (sb, 1, "static readonly string[] {0} = new string[] {{\n", branch2.Path);

				foreach (string languageCode in tree.AllLanguageCodes) {
					string value = branch2.Translate (languageCode);
					AppendTabFormat (sb, 2, "\"{0}\",\n", value);
				}

				AppendTabFormat (sb, 1, "}};\n");
			}
		});

		sb.AppendFormat ("}}");

		File.WriteAllText (outputFile, sb.ToString ());

		return true;
	}

	private void AppendTabFormat(StringBuilder sb, int depth, string format, params string[] args){
		AddTabs (sb, depth);
		sb.AppendFormat (format, args);
	}

	private void AddTabs(StringBuilder sb, int depth){
		for (int i = 0; i < depth; i++) {
			sb.Append ("\t");
		}
	}
	
	private bool ProcessFile(string filePath, LauretteTree tree, out string error) {

		error = null;

		// 0) Read in the .strings file
		string stringsFileAsString = File.ReadAllText (filePath);
		if (stringsFileAsString == null) {
			error = string.Format ("Unable to read file: {0}", filePath);
			return false;
		}


		// extract the two-character language code from the path; it should be the directory the .strings file it in
		string languageCode = Path.GetFileName(Path.GetDirectoryName(filePath));

		// 1) Process the strings file, pulling out all of the key-value pairs
		// old, doesn't handle escaped quotes: "\"([^\"]+)\"\\s*=\\s*\"([^\"]+)\""
		// new, handles escaped quotes: "([^"]+)"\s*=\s((?<![\\])['"])((?:.(?!(?<![\\])\2))*.?)\2;
		MatchCollection matches = Regex.Matches (stringsFileAsString, "\"([^\"]+)\"\\s*=\\s((?<![\\\\])['\"])((?:.(?!(?<![\\\\])\\2))*.?)\\2;");
		foreach (Match match in matches) {

			string value = match.Groups [3].Value;
			string key = match.Groups [1].Value;
			string originalKey = key;

			// handle special cases for the value before processing:
			key = key.Trim (".".ToCharArray ());

			while (key.Contains (" ")) {
				key = key.Replace (" ", "_");
			}
			while (key.Contains ("..")) {
				key = key.Replace ("..", ".");
			}
				
			for (char i = (char)0; i < (char)128; i++) {
				if (Char.IsLetter (i) == false && i != '.') {
					key = key.Replace (""+i, "_");
				}
			}

			string[] parts = key.Split (".".ToCharArray ());
			for (int i = 0; i < parts.Length; i++) {
				string part = parts [i];
				if (Char.IsLetter (part, 0) == false) {
					part = "_" + part;
				}
					
				if (Array.IndexOf(invalidKeywords, part) >= 0) {
					part = "_" + part;
				}

				parts [i] = part;
			}
			key = string.Join (".", parts);

			tree.Add (originalKey, key, value, languageCode);
		}

		return true;
	}


}


public class LauretteTree {
	private LauretteBranch root;
	private List<string> allLanguageCodes = new List<string>();

	public LauretteTree() {
		root = new LauretteBranch ("Localizations", "Localizations", null, 0);
	}

	public List<string> AllLanguageCodes {
		get {
			return new List<string> (allLanguageCodes);
		}
	}

	public void Add(string originalKey, string key, string value, string languageCode) {
		string[] keyPaths = key.Split (".".ToCharArray ());
		LauretteBranch branch = root.TraverseAndAddBranches(originalKey, keyPaths, 0);
		branch.AddTranslation (value, languageCode);

		if (allLanguageCodes.Contains (languageCode) == false) {
			allLanguageCodes.Add (languageCode);
		}
	}

	public void Traverse(Action<LauretteBranch,bool,bool,int> block) {
		root.Traverse (block, 0);
	}

}

public class LauretteBranch {

	private Dictionary<string,LauretteBranch> branches = new Dictionary<string,LauretteBranch>();
	private string name;
	private string path;
	private string originalKey;

	private Dictionary<string,string> translations = new Dictionary<string,string>();


	public string Name {
		get {
			return name;
		}
	}

	public string Path {
		get {
			return path;
		}
	}

	public string OriginalKey {
		get {
			return originalKey;
		}
	}

	public Dictionary<string,string> Translations {
		get {
			return new Dictionary<string,string> (translations);
		}
	}

	public LauretteBranch(string originalKey, string name, string[] keys, int keyIdx) {
		this.name = name;
		this.originalKey = originalKey;

		if (keys != null) {
			StringBuilder sb = new StringBuilder ();
			for (int i = 0; i < keyIdx; i++) {
				sb.Append (keys [i]);
				sb.Append ("_");
			}
			path = sb.ToString ();
		}
	}

	public void AddTranslation(string value, string languageCode){
		translations [languageCode] = value;
	}

	public string Translate(string languageCode){
		if (translations.ContainsKey (languageCode) == false) {
			if (translations.ContainsKey ("en") == false) {
				return "";
			}
			return Translate ("en");
		}
		return translations [languageCode];
	}

	public void Traverse(Action<LauretteBranch,bool,bool,int> block, int depth) {

		if (branches.Count == 0) {
			block (this, true, true, depth);
		} else {
			block (this, true, false, depth);
			foreach (string key in branches.Keys) {
				LauretteBranch branch = branches [key];
				branch.Traverse (block, depth + 1);
			}
			block (this, false, false, depth);
		}
	}

	public LauretteBranch TraverseAndAddBranches(string originalKey, string[] keyPaths, int idx){

		if (idx >= keyPaths.Length) {
			return this;
		}

		string key = keyPaths [idx];
		if (branches.ContainsKey (key)) {
			return branches [key].TraverseAndAddBranches (originalKey, keyPaths, idx + 1);
		}


		branches [key] = new LauretteBranch (originalKey, key, keyPaths, idx + 1);
		return branches [key].TraverseAndAddBranches (originalKey, keyPaths, idx + 1);
	}
}