TextMeshPro (TMP) Quick Usage Guide
Hungry Dragon 
Created by Alger Ortín Castellví on 30/06/2016.
Copyright (c) 2016 Ubisoft. All rights reserved.

TEXTFIELDS:
- Most textfields in the game should be created using the TextMeshPro Text component, using the Latin fonts (FNT_Default and FNT_Bold) and assigning the material matching the desired style.
- Exception to this rule are textfields with "dynamic" text: Strings that we don't know beforehand such as usernames, chat, messages, etc. These textfields should be created with the default uGui Text component, using a dynamic font that defaults to the OS fonts when characters are missing.
	- In this project, both FNT_Default/FNT_Default.ttf and FNT_Bold/FNT_Bold.ttf

FONT ASSETS:
- For each ttf font to be used, a TextMeshPro font asset must be created using the TMP Font Asset Creator tool (see https://youtu.be/qzJNIGCFFtY)
	- Final settings should be as following:
		- Font Size: set manually after testing, shouldn't be lower than 30
		- Font Padding: Up to you, bigger padding allows for better up-scaling and bigger outline/shadow effects, but also requires a bigger texture size
			- A value from 1 to 4 is suitable for standard text fonts
			- A value bigger than 5 is suitable for titles, big score numbers, etc.
		- Atlas Resolution: As small as possible to fit all needed characters. Never bigger than 2048x2048 (mobile limitations)
		- Character Set (see attached UnicodeRanges.txt file for more info):
			- Latin (includes numbers, symbols, punctuation and currencies): 0000-007F,0030-0039,0080-00FF,0100-017F,0180-024F,2C60-2C7F,A720-A7FF,AB30-AB6F,1E00-1EFF,2000-206F,20A0-20CF
			- RU (fallback for Latin font): 0000-007F,0400-04FF,0500-052F,20A0-20CF
			- JP, KO, ZH and other asian languages: Use the "Characters from file" option and input your localization file for the target language.
		- Font Render Mode:
			- Use Raster while testing (i.e. when determining optimal font size/atlas size)
			- Use Distance Field 32 for the final export (takes a while to generate)
		- The rest of the parameters can be freely modified as needed
- Asian font assets should be updated every time localization is changed
- All font assets should always Fallback to one of the latin fonts, to include numbers, symbols and other international stuff
	- To do this, select the font asset once it's created and find the "Fallback Font Assets" section on the inspector
	- Alternatively, modify the general fallback settings in the TMP Settings file
- Consider having a separate font asset for numbers:
	- Useful for score counters, multipliers, damage..., so the font on these texts doesn't get replaced when switching languages (thus replacing them by something much more ugly)
	- Allows you to have custom materials exclusively for those texts (i.e. Big, nice, x16 score multiplier with gradient, bevel, shadow, etc.)
	- Allows you to put in a bigger padding, allowing bigger outlines, shadows, etc.
	- Include also decimal and thousands separator characters (in all supported languages), or even the full Latin set (although not recommended, should be a small asset since it will be in memory together with the main font asset)

MATERIALS:
- DO NOT modify the default material (the one nested within the font asset)
- A material should be created for every font-style combination.
- Remember that modifying a material property will affect all textfields using that same material!
- To create a new material, duplicate an existing material of the same font and then modify the properties of the new material (or paste them from another material, could be from a material for another font)
- Each font has its own folder, put materials there with:
	- Font name as prefix (otherwise it won't be listed in the materials list on the textfield inspector)
	- Style name as suffix, the same name for all fonts
	- Example:  FNT_Default/FNT_Default_BlackStroke and FNT_KO/FNT_KO_BlackStroke)
- Use the TMPro/Mobile/Distance Field shader whenever possible, switch to the TMPro/Distance Field if you need some of the advanced features in TMPro
- Make sure that material's texture matches the related font atlas
- The system will automatically switch a textfield's font when required and will look for the matching style for the new font
- Remember that you can modify multiple materials at once by selecting them all

MORE HELP:
- Please refer to the following sites:
	- Youtube tutorials: https://www.youtube.com/user/Zolran/videos
	- Official TMP forum: http://digitalnativestudios.com/forum/index.php
	- Unity's forum TMP thread: http://forum.unity3d.com/threads/text-mesh-pro-the-ultimate-text-solution-for-unity-powerful-flexible-advanced-text-rendering.248636/