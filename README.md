# UPFLoader-ActionSpeed



Loader for ActionSpeed Skyrim UPF Patch:<br>
https://www.nexusmods.com/skyrimspecialedition/mods/35097

This loader gathers the mods from your load order that contain the records ActionSpeed wants to edit. Any mod you do not want to include, just untick in ModOrganizer before launching Synthesis.

This outputs "ActionSpeedLoader.esp", which you then load in with zEdit to workaround zEdit's 255 plugin limit.

Once you've created your zEdit patch, please delete the loader esp.

This also outputs a dummy Synthesis.esp. Please only run this patch alone, and make a backup or rename any other "Synthesis.esp" you have.

Be sure to check for conflicts in your created zEdit patch afterwards in xEdit.

To use, install Synthesis and add this repo, or select it from the built-in list. <br>
https://github.com/Mutagen-Modding/Synthesis


Inspired by: https://www.nexusmods.com/skyrimspecialedition/mods/35012 <br>
This loader was made due to the xEdit script missing some mods that need to be included.
