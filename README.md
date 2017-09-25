# Double Fine Tool Decompiled
## Description
This is the decompiled source code for the Double Fine Tool (http://forum.xentax.com/blog/?tag=double-fine-tool).
This tool can be used to unpack .~h and .~p files from Double Fine games such as Brütal Legend, The Cave, Costume Quest, Stacking, and Iron Brigade.

## Requirements
To use, you must have the .NET Framework version 4.0 or higher installed on your system. This has not been tested on Linux or macOS, but may work using Wine if properly installed.
Once compiled, be sure to have the "DoubleFineTool.exe.config" file in the same directory as "DoubleFineTool.exe".

## Usage
Unpacking:
 DoubleFineTool.exe -u [.~h file name] [folder path for unpacked files (OPTIONAL)]
Packing:
 DoubleFineTool.exe -p [folder path of unpacked files] [file name for repack (without file extension) (OPTIONAL)]
 
## Note
I did not create this tool! The author listed on the Xentax page claims they are not the author of the tool. The only lead I have to finding the real author is this Japanese blog: https://sites.google.com/site/jpmodfiles/localize/stacking. Unfortunately, there seems to be no way to contact the author that I am aware of. If anyone does have contact info for the creator of this tool, please feel free to message me. I, in no way, claim this tool or its code as my own.

## Issues/TODO/Future Goals
The reason I have decompiled and uploaded this tool on GitHub is to ask the community for assistance. Currently, using this tool, it is possible to make the files contained in the packages (.~p files) smaller (i.e. subtracting bytes). Since this tool recalculates the header (.~h file), the game is able to properly read the new offset for each of the files. Unfortunately, adding bytes is far more limited. For this example, I used Double Fine Tool to extract the "Man_Trivial.~h/.~p" files from Brütal Legend. In the folder it created, there is the file ".\gameplay\difficulty.DifficultySet". I can add up to 9 bytes to it without the game crashing, but 10 bytes causes the game to completely lock up. This is because for some reason, the offset for the file data is recalculated for up to 9 bytes, but not for 10 or more bytes. For other files in the extracted folder, the limit is different (i.e. ".\gameplay\levels\level.LevelList" has a limit of somewhere between 4 and 8 additional bytes, but I haven't had the time to get a specific value for that file yet). On the plus side, it doesn't seem like adding bytes to multiple files affects anything, as long as you don't add more than the limit of each individual file. I am hoping that someone can help brainstorm a solution for this issue.
Once the issue mentioned is resolved, I intend to create my own Java-based tool that is cross-platform and open sourced, using this source code and improvements made via pull request as a basis for understanding what needs to happen. Again, I wish to reiterate that the Double Fine Tool is not my work, I do not take credit for it, and if anyone has contact info for the person who did create it, please let me know in some form or fashion.