# :old_key: Unlocking SaveData Slots

If you do not see several or one of the save files after loading the game, you may have never before saved the game manually in the slot where the save file is missing. Slots unlock when the first SaveData is written by a game on a given slot.

## How to locate SaveData directory

Complete the following path pattern with your data to find your SaveData files location: `<steam_installation_folder>\userdata\<steam_id>\<steam_appid>\remote\win64_save\`.

| Game Title                | App ID  |
|---------------------------|---------|
| Resident Evil 4 Remake    | 2050650 |

## How to unlock locked SaveData slots

First of all, temporarily disable Steam Cloud and enable it after you are done.

<img src="https://github.com/mi5hmash/LimebrellaSharp/blob/main/.resources/images/How to%20disable%20steam%20cloud.png" alt="How to disable steam cloud"/>

### data00-1.bin
When you are starting on a new Steam Account the first thing you should do is to run the game and reach the Main Menu. This should unlock the slot for the `data00-1.bin` file.

### data000.bin
Next, you should choose an option to start a New Game and reach the point where the game creates its first AutoSave. This should unlock the slot for the `data000.bin` file.

### data001Slot.bin - data020Slot.bin
In order to unlock the rest of the slots, you should save the game on each of these slots one by one. For Resident Evil games, you must reach the first typewriter to manually save the game state.

I have prepared an AutoSave in front of the typewriter to make it easier for you. Feel free to download the one corresponding to your game, from the table below:

| Game Title                | AutoSave  |
|---------------------------|-----------|
| Resident Evil 4 Remake    | <a href="https://github.com/mi5hmash/LimebrellaSharp/raw/main/.resources/Save%20Files/re4.zip" target="_blank">re4.zip</a> |
| Resident Evil 4 Remake - Separate Ways DLC | <a href="https://github.com/mi5hmash/LimebrellaSharp/raw/main/.resources/Save%20Files/re4%20-%20separate%20ways.zip" target="_blank">re4 - separate ways.zip</a> |

After you unzip the archive, you have to resign the unpacked SaveData with my tool. Type your Steam32_ID into the **"Steam32_ID (OUTPUT)"** TextBox and push the **"Pack All"** button. Then, put the packed `data000.bin` in your SaveData files directory, launch the game, and load the AutoSave.

Once you are standing in front of the typewriter, simply use it to create a SaveData file in every slot possible.
  
The next steps are: Exit to the Desktop, Navigate to SaveData files directory, Delete all SaveData files in there, Copy & Paste the resigned SaveData files that you want to use.  
  
Now, when you launch the game again, you should be able to see your shiny SaveData files ready to be loaded.

When everything is done and working, you can enable Steam Cloud.

Have fun! :smile:
