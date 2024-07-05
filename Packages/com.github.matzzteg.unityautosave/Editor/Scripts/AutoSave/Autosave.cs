
#if UNITY_EDITOR
namespace Matzzteg.Editor
{
    using System;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine.SceneManagement;
    using UnityEngine;

    /// <summary>
    /// Will auto save the current open scenes if any are unsaved.
    /// Can either do it silently in the background or ask the user if they want to save.
    /// Option to save with an interval from 1 minute to 30 minutes.
    /// Option to save scenes when entering play mode.
    /// </summary>
    [InitializeOnLoad]
    public class AutoSave : EditorWindow
    {
        private static readonly string _versionNum = "v1.1.4";

        private static bool _autoSaveScene = true;
        private static bool _showDebugMessage = true;
        private static bool _askToSave = true;
        private static bool _resetTimerOnExitPlay = false;
        private static bool _autoSaveOnPlay = true;
        private static bool _asktoSaveOnPlay = true;
        private static bool _isStarted = false;
        private static int _intervalScene = 1;
        private static DateTime _lastSaveTimeScene;
        private static bool forceStopTimeUpdateOnPlay = false;

        private static string _projectPath;
        private static string _activeScenePath;

        // Have we loaded the prefs yet
        private static bool prefsLoaded = false;

        //preference strings
        private static readonly string _preferencesPrefix = "AutoSave: ";
        private static readonly string AutoSaveScenePref = _preferencesPrefix + "." + "Enable Timed Auto Save";
        private static readonly string IntervalPref = _preferencesPrefix + "." + "Interval Scene";
        private static readonly string ResetTimerOnExitPlayPref = _preferencesPrefix + "." + "Reset Timer On Exit Play";
        private static readonly string AskToSavePref = _preferencesPrefix + "." + "Ask To Save";

        private static readonly string AutoSaveOnPlayPref = _preferencesPrefix + "." +  "Auto Save On Play";
        private static readonly string AskToAutoSaveOnPlayPref = _preferencesPrefix + "." +  "Ask To Save On Play";

        private static readonly string ShowDebugMessagePref = _preferencesPrefix + "." + "Show Debug Message";

        //GUI
        private static Vector2 scrollPos;

        // Load editor preferences
        private static void LoadPrefs()
        {
            // Load the preferences
            if (prefsLoaded)
            {
                return;
            }

            _autoSaveScene = EditorPrefs.GetBool(AutoSaveScenePref, true);
            _resetTimerOnExitPlay = EditorPrefs.GetBool(ResetTimerOnExitPlayPref, false);
            _showDebugMessage = EditorPrefs.GetBool(ShowDebugMessagePref, true);
            _askToSave = EditorPrefs.GetBool(AskToSavePref, true);
            _autoSaveOnPlay = EditorPrefs.GetBool(AutoSaveOnPlayPref, true);
            _asktoSaveOnPlay = EditorPrefs.GetBool(AskToAutoSaveOnPlayPref, true);
            _intervalScene = EditorPrefs.GetInt(IntervalPref, 1);
            prefsLoaded = true;
        }

        // Save editor preferences
        private static void SavePrefs()
        {
            EditorPrefs.SetBool(AutoSaveScenePref, _autoSaveScene);
            EditorPrefs.SetBool(ResetTimerOnExitPlayPref, _resetTimerOnExitPlay);
            EditorPrefs.SetBool(ShowDebugMessagePref, _showDebugMessage);
            EditorPrefs.SetBool(AskToSavePref, _askToSave);
            EditorPrefs.SetBool(AutoSaveOnPlayPref, _autoSaveOnPlay);
            EditorPrefs.SetBool(AskToAutoSaveOnPlayPref, _asktoSaveOnPlay);
            EditorPrefs.SetInt(IntervalPref, _intervalScene);
        }

        // Initialise
        static AutoSave()
        {
            _projectPath = Application.dataPath;
            EditorApplication.update += Update;
            EditorSceneManager.sceneSaved += OnSceneSaved;
            EditorApplication.playmodeStateChanged += OnPlaymodeStateChanged;
            LoadPrefs();
            UpdateLastSavedTime();
        }

        // Using the preference GUI
        [PreferenceItem("Auto Save")]
        public static void PreferencesGUI()
        {
            EditorGUI.BeginChangeCheck();

            LoadPrefs();

            //GUILayout.Label(_versionNum, EditorStyles.boldLabel);
            //GUILayout.Space(3);
            GUILayout.Label("Current Info:", EditorStyles.boldLabel);
            _activeScenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
            EditorGUILayout.LabelField("Active Scene:");
            EditorGUILayout.HelpBox(_activeScenePath, MessageType.None);
            EditorGUILayout.LabelField("Project Folder Path:");
            EditorGUILayout.HelpBox(_projectPath, MessageType.None);
            EditorGUILayout.LabelField("Saving scene/s:");
            EditorGUILayout.HelpBox(GetAllOpenScenesAsSingleStringList(), MessageType.None);

            EditorGUILayout.LabelField("Last Timed Save:", _isStarted ? "" + _lastSaveTimeScene : "");
            DateTime nextSaveTime = _lastSaveTimeScene.AddMinutes(_intervalScene);
            EditorGUILayout.LabelField("Next Timed Save:", _isStarted ? "" + nextSaveTime : "");

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUILayout.Space(5);
            GUILayout.Label("Options:", EditorStyles.boldLabel);

            _showDebugMessage = EditorGUILayout.ToggleLeft("Show Save Debug Message", _showDebugMessage);

            _autoSaveScene = EditorGUILayout.BeginToggleGroup("Enable Timed Auto Save", _autoSaveScene);
            EditorGUI.indentLevel++;
            _intervalScene = EditorGUILayout.IntSlider("Interval (minutes)", _intervalScene, 1, 30);
            _resetTimerOnExitPlay = EditorGUILayout.ToggleLeft("Reset Timer On Exiting Play", _resetTimerOnExitPlay);
            _askToSave = EditorGUILayout.ToggleLeft("Ask To Save", _askToSave);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndToggleGroup();

            GUILayout.Space(1);
            _autoSaveOnPlay = EditorGUILayout.BeginToggleGroup("Enable Auto Save On Play", _autoSaveOnPlay);
            EditorGUI.indentLevel++;
            _asktoSaveOnPlay = EditorGUILayout.ToggleLeft("Ask To Save", _asktoSaveOnPlay);
            EditorGUI.indentLevel--;
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndScrollView();

            // Save editor prefs when gui has finished its changes
            if (EditorGUI.EndChangeCheck() || GUI.changed)
            {
                SavePrefs();
            }
        }


        //[MenuItem("Matzzteg/Auto Save")]
        //public static void ShowAutoSaveWindow()
        //{
        //    GetWindow<AutoSave>("Auto Save");
        //}

        // update the editor window
        //private void OnGUI()
        //{

        //}


        private static string GetAllOpenScenesAsSingleStringList()
        {
            string scenesAsList = string.Empty;
            if (SceneManager.sceneCount > 0)
            {
                for (int i = 0; i < SceneManager.sceneCount; ++i)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (scene.name == string.Empty || scene.name == "")
                    {
                        scenesAsList += "Untitled.unity";
                        continue;
                    }
                    scenesAsList += i == 0 ? scene.name + ".unity" : "\n" + scene.name + ".unity";
                }
            }

            return scenesAsList;
        }

        private static Scene[] GetAllOpenScenesAsSceneArray()
        {
            Scene[] scenes = null;
            if (SceneManager.sceneCount > 0)
            {
                scenes = new Scene[SceneManager.sceneCount];
                for (int i = 0; i < SceneManager.sceneCount; ++i)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    scenes[i] = scene;
                }
            }
            else
            {
                return scenes;
            }

            return scenes;
        }

        static void Update()
        {
            if (_autoSaveScene)
            {
                if (DateTime.Now.Minute >= (_lastSaveTimeScene.Minute + _intervalScene) ||
                    DateTime.Now.Minute == 59 && DateTime.Now.Second == 59)
                {
                    SaveScene(_askToSave);
                }
            }
            else
            {
                _isStarted = false;
            }
        }

        // Will save the scene based on user settings
        private static void SaveScene(bool showSaveDialogue)
        {
            if (!EditorApplication.isPlaying)
            {
                if (showSaveDialogue)
                {
                    // ask the user to save any current work
                    if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        UpdateLastSavedTime();
                        return; // if the user cancels the save we exit out of the code
                    }

                    AssetDatabase.SaveAssets(); // we can save the assetdatabase anytime as far as I know
                    UpdateLastSavedTime();
                }
                else //save behind the scenes
                {
                    EditorSceneManager.SaveOpenScenes();
                    AssetDatabase.SaveAssets();
                    UpdateLastSavedTime();
                }
            }
        }

        // Update last save time and display to screen if required
        private static void UpdateLastSavedTime()
        {
            if (forceStopTimeUpdateOnPlay)
            {
                return;
            }
            _lastSaveTimeScene = DateTime.Now;
            _isStarted = true;
        }

        // Log whenever a scene is saved
        static void OnSceneSaved(UnityEngine.SceneManagement.Scene scene)
        {
            UpdateLastSavedTime();

            if (_showDebugMessage)
            {
                if (SceneManager.sceneCount > 1)
                {
                    foreach (var currentScene in GetAllOpenScenesAsSceneArray())
                    {
                        Debug.Log("[AutoSave Message] Saved: " + currentScene.path + " on " + _lastSaveTimeScene + "\n To disable these messages go to: Edit/Preferences/AutoSave/ShowMessage.");
                    }
                }
                else
                {
                    Debug.Log("[AutoSave Message] Saved: " + scene.path + " on " + _lastSaveTimeScene + "\n To disable these messages go to: Edit/Preferences/AutoSave/ShowMessage.");
                }
            }
        }

        // if the user has it enabled this is used for saving before entering play mode
        static void OnPlaymodeStateChanged()
        {
            // are we entering play mode from not being in play mode
            if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                //if automatic save on entering play mode is active
                if (_autoSaveOnPlay)
                {
                    SaveScene(_asktoSaveOnPlay);
                }
            }

            // are we exiting play mode from just being in play mode
            if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // reset the timer if we are exiting play mode and want to ensure the timer does not trigger an auto save if the user does not want to
                if (_autoSaveScene && _resetTimerOnExitPlay)
                {
                    UpdateLastSavedTime();
                }
            }
        }

    }
}
#endif
