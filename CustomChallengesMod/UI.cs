using System.Linq; // contains extensions

namespace CustomChallengesMod
{
    public class UI
    {
        #region "Private fields"
            // Create a GameObject for your window to attach to.
            private static UnityEngine.GameObject _windowHolder=null;

            // Random window ID to avoid conflicts with other mods.
            private static readonly int _mainWindowID = SFS.UI.ModGUI.Builder.GetRandomID();

            private static bool _debug=false;
            private static string _filter="All";
            private static bool _isActive=false;
            private static bool _isOpen=false;
            private static SFS.UI.ModGUI.Window _window=null;
        #endregion

        #region "Private methods"
            /// <summary>Display the data - all </summary>
            private static void DisplayAll()
            {
                Filter="All";
            }

            /// <summary>Display the data - Complete Only </summary>
            private static void DisplayComplete()
            {
                Filter="Complete";
            }

            /// <summary>Display the data - In Progress Only </summary>
            private static void DisplayInProgress()
            {
                Filter="InProgress";
            }

            /// <summary>Display the data - Incomplete Only </summary>
            private static void DisplayIncomplete()
            {
                Filter="Incomplete";
            }

            /// <summary>Display the data</summary>
            private static void Display()
            {
                string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                UnityEngine.Vector2Int pos = SettingsManager.settings.windowPosition;
                UnityEngine.RectTransform canvas = UITools.UIUtility.CanvasRectTransform;
                UnityEngine.Vector2 canvasSize = new UnityEngine.Vector2(canvas.rect.width * canvas.lossyScale.x, canvas.rect.height * canvas.lossyScale.y);
                System.Text.StringBuilder display = new System.Text.StringBuilder();
                System.Collections.Generic.Dictionary<SFS.Logs.Challenge, (int i, string progressData)> progress=null;
                System.Collections.Generic.HashSet<string> completeChallenges=null;
//~                 System.Collections.Generic.HashSet<SFS.Logs.Challenge> completeChallenges_Rocket=null;

                if (_isOpen)
                {
                    if (scene=="World_PC")
                    {
                        if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                        {
//~                             completeChallenges_Rocket= rocket.stats.challengeRecorder.GetCompleteChallenges();

                            progress= (System.Collections.Generic.Dictionary<SFS.Logs.Challenge, (int i, string progressData)>)
                                typeof(SFS.Stats.ChallengeRecorder)
                                    .GetField("progress", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                                    .GetValue(rocket.stats.challengeRecorder);

                        }
                    }

                    if (scene=="World_PC" || _debug)
                    {
                        if
                            (
                                SFS.Base.worldBase!=null
                                && SFS.Base.worldBase.paths!=null
                                && SFS.Base.worldBase.paths.worldPersistentPath!=null
                            )
                        {
                            SFS.IO.FilePath filePath = SFS.Base.worldBase.paths.worldPersistentPath.ExtendToFile("Challenges.txt");
                            System.Collections.Generic.List<string> completeChallenges_List = new System.Collections.Generic.List<string>();

                            if
                                (
                                    filePath.FileExists()
                                    && SFS.Parsers.Json.JsonWrapper.TryLoadJson<System.Collections.Generic.List<string>>(filePath, out completeChallenges_List)
                                )
                            {
                                completeChallenges = completeChallenges_List.ToHashSet();
                            }
                        }
                    }

//~                     display.AppendFormat("completeChallenges.Count={0:D}",completeChallenges.Count).AppendLine();
//~                     if (completeChallenges_Rocket!=null) display.AppendFormat("completeChallenges_Rocket.Count={0:D}",completeChallenges_Rocket.Count).AppendLine();

                    if (!_debug) _filter="InProgress";
                    if (progress==null && _filter=="InProgress") _filter="All";
                    if (completeChallenges==null && _filter=="Complete") _filter="All";
                    if (completeChallenges==null && _filter=="Incomplete") _filter="All";

                    foreach (SFS.Logs.Challenge oneChallenge in SFS.Base.worldBase.challengesArray)
                    {
                        bool isComplete=false;
//~                         bool isComplete_Rocket=false;
                        int progressStepNo = 0;
                        string progressState = "";

                        if (oneChallenge.steps!=null && oneChallenge.steps.Count!=0)
                        {
                            if (progress!=null && progress.ContainsKey(oneChallenge))
                            {
                                progressStepNo=progress[oneChallenge].Item1+1;
                                progressState=progress[oneChallenge].Item2;
                            }
                            if (completeChallenges!=null && completeChallenges.Contains(oneChallenge.id))
                            {
                                isComplete=true;
                            }
//~                             if (completeChallenges_Rocket!=null && completeChallenges_Rocket.Contains(oneChallenge))
//~                             {
//~                                 isComplete_Rocket=true;
//~                             }
                        }

                        if
                            (
                                _filter=="All"
                                || (_filter=="InProgress" && !isComplete && progressStepNo!=0)
                                || (_filter=="Complete" && isComplete)
                                || (_filter=="Incomplete" && !isComplete)
                            )
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
                                int stepNo = 1;

                                if (oneChallenge.steps.Count==1)
                                {
                                    display.Append(" (one step");
                                }
                                else
                                {
                                    display.AppendFormat(" ({0:N0} steps",oneChallenge.steps.Count);
                                }

                                if (isComplete)
                                {
                                    display.Append(", complete");
                                }
//~                                 if (isComplete_Rocket)
//~                                 {
//~                                     display.Append(", complete (rocket)");
//~                                 }
                                display.Append(")");

                                foreach (SFS.Logs.ChallengeStep oneStep in oneChallenge.steps)
                                {
                                    display.AppendLine();

                                    if (oneStep is CustomChallengesMod.CustomSteps.Step_OneOf oneOneOfStep)
                                    {
                                        display.AppendFormat("... {0:D}: {1}", stepNo, oneOneOfStep.ToStringExt(progress:progressState));

                                        if ((string.IsNullOrEmpty(progressState) || stepNo!=progressStepNo) && (stepNo<=progressStepNo))
                                        {
                                            display.AppendLine();
                                            display.Append("... (complete)");
                                        }
                                    }
                                    else if(oneStep is CustomChallengesMod.CustomSteps.Step_AllOf oneAllOfStep)
                                    {
                                        display.AppendFormat("... {0:D}: {1}", stepNo, oneAllOfStep.ToStringExt(progress:progressState));

                                        if ((string.IsNullOrEmpty(progressState) || stepNo!=progressStepNo) && (stepNo<=progressStepNo))
                                        {
                                            display.AppendLine();
                                            display.Append("... (complete)");
                                        }
                                    }
                                    else if(oneStep is CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt oneAny_LandmarkStep)
                                    {
                                        display.AppendFormat("... {0:D}: {1}", stepNo, oneAny_LandmarkStep.ToStringExt(progress:progressState));

                                        if ((string.IsNullOrEmpty(progressState) || stepNo!=progressStepNo) && (stepNo<=progressStepNo))
                                        {
                                            display.Append(" (complete)");
                                        }
                                    }
                                    else
                                    {
                                        display.AppendFormat("... {0:D}: {1}", stepNo, oneStep);

                                        if (!string.IsNullOrEmpty(progressState) && stepNo==progressStepNo)
                                        {
                                            display.AppendFormat(" (progress \"{0}\")",progressState);
                                        }
                                        else if (stepNo<=progressStepNo)
                                        {
                                            display.Append(" (complete)");
                                        }
                                    }
                                    stepNo++;
                                }
                                display.AppendLine();
                            }
                        }
                    }
                }

                if (_window!=null)
                {
                    UnityEngine.Object.Destroy(_window);
                    _window=null;
                }

                if (_windowHolder!=null)
                {
                    UnityEngine.Object.Destroy(_windowHolder);
                    _windowHolder=null;
                }

                // Create the window holder, attach it to the currently active scene so it's removed when the scene changes.
                _windowHolder = SFS.UI.ModGUI.Builder.CreateHolder(SFS.UI.ModGUI.Builder.SceneToAttach.CurrentScene, "CustomChallengesMod GUI Holder");

                _window = SFS.UI.ModGUI.Builder.CreateWindow
                    (
                        _windowHolder.transform
                        ,_mainWindowID
                        ,_isOpen?1200:160
                        ,_isOpen?(int)(canvasSize.y*1.8):90
                        ,posX: pos.x
                        ,posY: pos.y
                        ,draggable: true
                        ,savePosition: true
                        ,opacity: 0.95f
                        ,titleText: (_debug?(_isOpen?"Custom Challenges Debug":"CC Debug"):"Progress")
                    );

                // Create a layout group for the window. This will tell the GUI builder how it should position elements of your UI.
                _window.CreateLayoutGroup(SFS.UI.ModGUI.Type.Vertical, UnityEngine.TextAnchor.UpperCenter,10f);
                _window.EnableScrolling(SFS.UI.ModGUI.Type.Vertical);

                _window.gameObject.GetComponent<SFS.UI.DraggableWindowModule>().OnDropAction += () =>
                {
                    CustomChallengesMod.SettingsManager.settings.windowPosition = UnityEngine.Vector2Int.RoundToInt(_window.Position);
                    CustomChallengesMod.SettingsManager.Save();
                };

                if (_isOpen)
                {
                    SFS.UI.ModGUI.Container buttons_Container =  SFS.UI.ModGUI.Builder.CreateContainer(_window);
                    buttons_Container.CreateLayoutGroup(SFS.UI.ModGUI.Type.Horizontal, UnityEngine.TextAnchor.MiddleCenter,5f);

                    if (_debug)
                    {
                        SFS.UI.ModGUI.Button all_Button=SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 150, 30,onClick:DisplayAll, text: "All");
                        SFS.UI.ModGUI.Button inProgress_Button=null;
                        SFS.UI.ModGUI.Button complete_Button=null;
                        SFS.UI.ModGUI.Button incomplete_Button=null;

                        if (progress!=null) inProgress_Button=SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 150, 30,onClick:DisplayInProgress, text: "In Progress");
                        if (completeChallenges!=null) complete_Button=SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 150, 30,onClick:DisplayComplete, text: "Complete");
                        if (completeChallenges!=null)  incomplete_Button=SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 150, 30,onClick:DisplayIncomplete, text: "Incomplete");

                        switch (_filter)
                        {
                            case "All":all_Button.Text=all_Button.Text.ToUpper();break;
                            case "InProgress":
                                if (inProgress_Button!=null) inProgress_Button.Text=inProgress_Button.Text.ToUpper();
                            break;

                            case "Complete":
                                if (complete_Button!=null) complete_Button.Text=complete_Button.Text.ToUpper();
                            break;

                            case "Incomplete":
                                if (incomplete_Button!=null) incomplete_Button.Text=incomplete_Button.Text.ToUpper();
                            break;
                        }
                    }
                    SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 150, 30,onClick:Hide, text: "Hide");
                }
                else
                {
                    SFS.UI.ModGUI.Builder.CreateButton(_window, 100, 30, posY:30, onClick:Show, text: "Show");
                }

                if (_isOpen)
                {
                    foreach (string oneLine in display.ToString().Split(new string[]{System.Environment.NewLine},System.StringSplitOptions.None))
                    {
                        SFS.UI.ModGUI.Label display_Label=SFS.UI.ModGUI.Builder.CreateLabel
                            (
                                _window
                                ,1180
                                ,30
                                ,text:oneLine
                            );
                        display_Label.AutoFontResize=true;
                        display_Label.FontSize=25;
                        display_Label.TextAlignment = TMPro.TextAlignmentOptions.TopLeft;
                    }
                }
            }

            /// <summary>Hide the data</summary>
            private static void Hide()
            {
                IsOpen=false;
            }

            /// <summary>Show the data</summary>
            private static void Show()
            {
                IsOpen=true;
            }
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

            /// <summary>The current filter: "All", "InProgress", "Complete", "Incomplete"</summary>
            public static string Filter
            {
                get
                {
                    return _filter;
                }
                set
                {
                    if (value.ToLower()!= _filter.ToLower())
                    {
                        switch (value.ToLower())
                        {
                            case "inprogress": _filter="InProgress";break;
                            case "complete": _filter="Complete";break;
                            case "incomplete": _filter="Incomplete";break;
                            default: _filter="All";break;
                        }
                        Display();
                    }
                }
            }

            /// <summary>True if the GUI is currently active</summary>
            public static bool IsActive
            { get { return _isActive;}}

            /// <summary>True if the window is open</summary>
            public static bool IsOpen
            {
                get
                {
                    return _isOpen;
                }
                set
                {
                    if (value!= _isOpen)
                    {
                        _isOpen=value;
                        Display();
                    }
                }
            }

        #endregion

        #region "Public methods"
            /// <summary>Show the GUI</summary>
            public static void ShowGUI()
            {
                _isActive = false;
                _debug =  SettingsManager.settings.debug;
                Display();
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
