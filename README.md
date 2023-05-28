[![License: MIT](https://img.shields.io/badge/License-Unlicense-blueviolet.svg)](https://opensource.org/licenses/MIT)
[![Release Version](https://img.shields.io/github/v/tag/mi5hmash/LimebrellaSharp?label=version)](https://github.com/mi5hmash/LimebrellaSharp/releases/latest)
[![Visual Studio 2022](https://img.shields.io/badge/VS%202022-blueviolet?logo=visualstudio&logoColor=white)](https://visualstudio.microsoft.com/)

# 🍋☂️ LimebrellaSharp - What is it :interrobang:
This app can **unpack and pack the "Lime" encrypted archives**. It can also **resign files** with your own SteamID to **use any SaveData on your profile**.

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
There are three options to achieve this. The first one is to drop the SaveData file or folder it is in on a TextBox **(1)** or a button **(2)**. Alternatively, you may use button **(2)** to open a folder picker and navigate to the directory from it. Also, you can type in the directory path into the text box **(1)**.

> **Note:** Starting from version 1.0.1, the program will extract the Steam32_ID from the "Input Folder Path" TextBox **(1)** (if it ends with *"<steam_id>\2050650\remote\win64_save"*) and will fill the TextBox **(3)** for you.

## Unpacking
Type in the Steam32_ID that was used to pack the SaveData file\s into a TextBox **(3)** and press the **"Unpack All"** button.

## Resigning files
Type in the Steam32_ID that was used to pack the SaveData file\s into a TextBox **(3)**.   
Then, type in the Steam32_ID, that you want to pack the SaveData file\s with, into a TextBox **(4)**. If you don't know your SteamID, then you can use [this site](https://www.steamidfinder.com) to find it. Once you have them typed in, just click the **"Resign All"** button **(7)** to resign all SaveData files.

## Packing
Type in the Steam32_ID that you want to pack the SaveData file\s with, into a TextBox **(4)** and press the **"Pack All"** button **(8)**.

## Open the Output Directory
You can open the Output directory in a new Explorer window by using the button **(9)**.

## Other buttons
Button **(5)** swaps TextBox **(3)** and TextBox **(4)** values.
Button **(10)** cancels the currently running operation.

# :fire: Issues
All the problems I've encountered during my tests have been fixed on the go. If you find any other issue (hope you won't) then please, feel free to report it [there](https://github.com/mi5hmash/LimebrellaSharp/issues).
# :star: Sources:
* https://github.com/tremwil/DS3SaveUnpacker
