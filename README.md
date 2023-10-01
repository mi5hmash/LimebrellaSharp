[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/LimebrellaSharp?label=version)](https://github.com/mi5hmash/LimebrellaSharp/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-Unlicense-blueviolet.svg)](https://opensource.org/licenses/MIT)
[![Visual Studio 2022](https://img.shields.io/badge/VS%202022-blueviolet?logo=visualstudio&logoColor=white)](https://visualstudio.microsoft.com/)
[![dotNET7](https://img.shields.io/badge/.NET%207-blueviolet)](https://visualstudio.microsoft.com/)

# 🍋☂️ LimebrellaSharp - What is it :interrobang:
This app can **unpack and pack the "Lime" encrypted archives**. It can also **resign files** with your own SteamID to **use any SaveData on your profile**.

## Supported titles*
| Game Title                | App ID  | Tested Version | Platform |
|---------------------------|---------|----------------|----------|
| Resident Evil 4 (Remake)  | 2050650 | 12103332       | PC       |

**the latest versions that I have tested and are supported*

# 🤯 Why was it created :interrobang:
I wanted to share my SaveData with a friend and find out why the game is taking so long to load my SaveData.

# :scream: Is it safe?
**No.** You can corrupt your SaveData files and lose your progress or get banned from playing online if you unreasonably modify your save.

Remember to always make a backup of the files that you want to edit, before modifying them.

Also, disable the Steam Cloud before you replace any SaveData files.

You have been warned and now you can move on to the next chapter, fully aware of possible consequences.

# :scroll: How to use this tool

<img src="https://github.com/mi5hmash/LimebrellaSharp/blob/main/.resources/images/MainWindow.png" alt="MainWindow"/>

## Setting the Input Directory
There are three ways to achieve this. The first one is to drop the SaveData file or folder it is in on a TextBox **(1)** or a button **(2)**. Alternatively, you may use button **(2)** to open a folder picker and navigate to the directory from it. Also, you can type in the directory path into the text box **(1)**.

> **Note:** Starting from version 1.0.1, the program will extract the Steam32_ID from the "Input Folder Path" TextBox **(1)** (if it ends with *"<steam_id>\2050650\remote\win64_save"*) and will fill the TextBox **(3)** for you.

## Unpacking data
Type in the Steam32_ID that was used to pack the SaveData file\s, into a TextBox **(3)** and press the **"Unpack All"** button **(6)**.

> **Note:** After that, you can find the unpacked files inside the ".\\_OUTPUT" directory.

## Packing data
Type in the Steam32_ID that you want to pack the SaveData file\s with, into a TextBox **(4)** and press the **"Pack All"** button **(7)**.

> **Note:** After that, you can find the packed files inside the ".\\_OUTPUT" directory.

## Resigning files
If you just want to resign your SaveData files to use them on another Steam Account then follow the steps below.

First, type in the Steam32_ID that was used to pack the SaveData file\s, into a TextBox **(3)**. It is a numeric ID of a Steam Account that belongs to the person that gave you their SaveData files.
Then, type in the Steam32_ID, that you want to pack the SaveData file\s with, into a TextBox **(4)**. This should be a numeric ID of a Steam Account on which you would like to load up the resigned SaveData files. If you don't know your SteamID, then you can use [this site](https://www.steamidfinder.com) to find it. Once you have both IDs typed in, just click the **"Resign All"** button **(8)** to resign all SaveData files.

> **Note:** After that, you can find the packed files inside the ".\\_OUTPUT" directory. All of them are signed with the Steam32_ID from the TextBox **(4)**.

## Open the Output Directory
You can open the **".\\_OUTPUT"** directory in a new Explorer window by using the button **(9)**.

## Other buttons
Button **(5)** swaps TextBox **(3)** and TextBox **(4)** values.
Button **(10)** cancels the currently running operation.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issue (hope you won't) then please, feel free to report it [there](https://github.com/mi5hmash/LimebrellaSharp/issues).

**IF YOU DO NOT SEE SAVEDATA FILES THAT YOU HAVE RESIGNED THEN PLEASE, READ <a href="https://github.com/mi5hmash/LimebrellaSharp/tree/main/.resources/Save%20Files" target="_blank">THIS DOCUMENT</a>.**

# :star: Sources
* https://github.com/tremwil/DS3SaveUnpacker
