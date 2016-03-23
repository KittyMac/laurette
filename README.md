# Laurette
**Localization code generator** written in vanilla C# with editor scripts for use in Unity3D. Laurette was heavily inspired by the awesome [Laurine for Swift](https://github.com/JiriTrecak/Laurine), and serves much of the same function for C#/Unity3D.  Three cheers mate!

## Why use Laurette?

Laurette automatically processes and then generates high performance C# code out of your projects .strings files. In your source code, you then reference the generate structures to access your localized content. If your .strings files change, the Laurette generated code will subsequently change and, if anything changes for the worse, your friendly compiler will let you know with errors exactly where they should be!

Laurette provides Unity editor scripts for seamless integration in Unity3D; just drop the two C# files (Laurette.cs and LauretteBuildProcess.cs) files into Assets/Editor. Laurette will monitor for any changes to .strings and .strings.txt files, and will immediately process them.  By default, it will put the generated code in Assets/Localizations.cs.

## Pretty examples pls!

Once Laurette has generated the Assets/Localizations.cs file, you can access the localizations from the Localizations static struct.

```C#
"MainMenu.PlayButton.Title" = "Play My Game!";
```

can now be accessed from code by:

```C#
buttonLabel.text = Localizations.MainMenu.PlayButton.Title;
```

![Image : Autocomplete Variables](https://raw.githubusercontent.com/KittyMac/laurette/master/image1.png)


Laurette will also properly detect format strings, and provide built-in methods for them:

```C#
"GameOver.ScoreField.Title" = "You vanquished {0} {1}!";
```

```C#
gameOverField.text = Localizations.GameOver.ScoreField.Title (5, Localizations.Monsters.Goblins);
```

![Image : Autocomplete Methods](https://raw.githubusercontent.com/KittyMac/laurette/master/image2.png)


## Nested Structures

Laurette will generate nested structures for language keys which follow a dot-separated format.  For example, the key "Monsters.Goblins" will generate the following code:

```C#
public struct Localizations {
	public struct Monsters {
		/// <summary>English: "Goblins"</summary>
		public static string Goblins {
			get {
				return Monsters_Goblins_[(int)currentLanguage];
			}
		}
	}
}
```

## Embedded Strings

There are dozens of different method for localizing content in Unity3D; just [search on the Asset Store.](https://www.assetstore.unity3d.com/en/#!/search/page=1/sortby=relevance/query=localization).  To make Laurette as lean, mean, and performant as possible,  Laurette will compile the translated strings directly into the main Localizations structure.  Translation lookups are then a single static string[] access; not need for resource loading, caching, or whatnot.


The MIT License (MIT)

Copyright (c) 2016 Rocco Bowling

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
