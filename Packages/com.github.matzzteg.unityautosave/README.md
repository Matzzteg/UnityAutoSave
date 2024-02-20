# com.github.matzzteg.unityautosave by Matzzteg


Will auto save the current open scenes if any are unsaved.
Can either do it silently in the background or ask the user if they want to save.
Option to save with an interval from 1 minute to 30 minutes.
Option to save scenes when entering play mode.

Settings can be found under Edit/Preferences/AutoSave.

Settings:
Show Save Debug Message - 
	Enabled: Will display a debug log in the console whenever Unity saves a scene.
	Disabled: No debug message will be displayed

Enable Timed Auto Save - 
	Enabled: Will start the timer check for automatically saving any open unsaved scenes.
	Disabled: Disables the timed auto save
Interval (minutes) - How long in minutes from the last timed save will it trigger an automatic save again.
Reset Timer On Exiting Play - 
	Enabled: Will stop the automatic save from being triggered when exiting play mode. Resets the timer.
	Disabled: Will keep the previous last save time when exiting play mode.
Ask To Save - 
	Enabed: Will display the unity scene save dialogue box for all open scenes when and automatic save is triggered. 
	Disabled: All scenes will be saved without confirmation when an automatic timed save is triggered.

Enable Auto Save On Play - 
	Enabled: Will trigger a save for all open scenes when entering play mode.
	Disabled: Disables the save trigger when entering play mode
Ask To Save - 
	Enabed: Will display the unity scene save dialogue box for all open scenes when entering play mode. 
	Disabled: All scenes will be saved without confirmation when entering play mode.
