using System.Linq; // contains extensions

namespace CustomChallengesMod
{
    public class UI
    {
        #region "Private fields"
            // Create a GameObject for your window to attach to.
            private static UnityEngine.GameObject windowHolder;

            // Random window ID to avoid conflicts with other mods.
            private static readonly int MainWindowID = SFS.UI.ModGUI.Builder.GetRandomID();

            private static bool _debug=false;
            private static bool _isActive=false;
        #endregion

        #region "Private methods"
        #endregion

        #region "Public properties"
            /// <summary>True if debugging mode is on</summary>
            public static bool Debug
            {
                get
                {
                    return _debug;
                }
                set
                {
                    _debug=value;
                   SettingsManager.settings.debug = _debug;
                    CustomChallengesMod.SettingsManager.Save();
                }
            }

            /// <summary>True if the GUI is currently active</summary>
            public static bool IsActive
            { get { return _isActive;}}

        #endregion

        #region "Public methods"
            /// <summary>Show the GUI</summary>
            public static void ShowGUI()
            {
                _isActive = false;
                _debug =  SettingsManager.settings.debug;
                // Create the window holder, attach it to the currently active scene so it's removed when the scene changes.
                windowHolder = SFS.UI.ModGUI.Builder.CreateHolder(SFS.UI.ModGUI.Builder.SceneToAttach.CurrentScene, "CustomChallengesMod GUI Holder");
                UnityEngine.Vector2Int pos = SettingsManager.settings.windowPosition;
                UnityEngine.DisplayInfo displayInfo = UnityEngine.Screen.mainWindowDisplayInfo;
                UITools.ClosableWindow window = UITools.UIToolsBuilder.CreateClosableWindow
                    (
                        windowHolder.transform
                        ,MainWindowID
                        ,displayInfo.width*9/10
                        ,displayInfo.height*9/10
                        ,posX: pos.x
                        ,posY: pos.y
                        ,draggable: true
                        ,savePosition: true
                        ,opacity: 0.95f
                        ,titleText:"Custom Challenges Debug"
                        ,minimized: false
                    );

                // Create a layout group for the window. This will tell the GUI builder how it should position elements of your UI.
                window.CreateLayoutGroup(SFS.UI.ModGUI.Type.Vertical, UnityEngine.TextAnchor.UpperCenter,10f);
                window.EnableScrolling(SFS.UI.ModGUI.Type.Vertical);

                window.gameObject.GetComponent<SFS.UI.DraggableWindowModule>().OnDropAction += () =>
                {
                    CustomChallengesMod.SettingsManager.settings.windowPosition = UnityEngine.Vector2Int.RoundToInt(window.Position);
                    CustomChallengesMod.SettingsManager.Save();
                };
                System.Text.StringBuilder display = new System.Text.StringBuilder();

                foreach (SFS.Logs.Challenge oneChallenge in SFS.Base.worldBase.challengesArray)
                {
                    if (display.Length>0) display.AppendLine();

                    display.AppendFormat
                        (
                            "[{0}] {1} ({2}) for \"{3}\""
                            ,oneChallenge.id
                            ,oneChallenge.title()
                            ,oneChallenge.difficulty
                            ,oneChallenge.owner.codeName
                        );

                    if (oneChallenge.returnSafely) display.Append("+return safely");
                    if (oneChallenge.steps==null || oneChallenge.steps.Count==0)
                    {
                        display.Append(" (no steps)");
                    }
                    else
                    {
                        if (oneChallenge.steps.Count==1)
                        {
                            display.Append(" (one step)");
                        }
                        else
                        {
                            display.AppendFormat(" ({0:N0} steps)",oneChallenge.steps.Count);
                        }

                        int stepNo = 1;
                        foreach (SFS.Logs.ChallengeStep oneStep in oneChallenge.steps)
                        {
                            display.AppendLine();
                            display.AppendFormat("... {0:D}: {1}", stepNo++, oneStep);
                        }
                        display.AppendLine();
                    }
                }

                foreach (string oneLine in display.ToString().Split(new string[]{System.Environment.NewLine},System.StringSplitOptions.None))
                {
                    SFS.UI.ModGUI.Label display_Label=SFS.UI.ModGUI.Builder.CreateLabel
                        (
                            window
                            ,displayInfo.width*9/10-20
                            ,30
                           ,text:oneLine
                        );
                    display_Label.AutoFontResize=false;
                    display_Label.FontSize=25;
                    display_Label.TextAlignment = TMPro.TextAlignmentOptions.TopLeft;
                }

                _isActive = true;
            }

            /// <summary>Note that the GUI is no longer active</summary>
            public static void GUIInActive()
            {
                _isActive = false;
            }
        #endregion
    }
}
