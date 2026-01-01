using System.Linq; // contains extensions

namespace CustomChallengesMod
{
    public class UI:UnityEngine.MonoBehaviour
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
            private static SFS.UI.ModGUI.Button _all_Button=null;
            private static SFS.UI.ModGUI.Button _complete_Button=null;
            private static SFS.UI.ModGUI.Button _incomplete_Button=null;
            private static SFS.UI.ModGUI.Button _inProgress_Button=null;
            private static SFS.UI.ModGUI.Button _hide_Button=null;
            private static SFS.UI.ModGUI.Button _show_Button=null;
            private static System.Collections.Generic.List<SFS.UI.ModGUI.Label> display_Labels=new System.Collections.Generic.List<SFS.UI.ModGUI.Label>();
        #endregion

        #region "Private methods"
            /// <summary>Display the data</summary>
            private static void Display()
            {
                string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

                if (_isActive && scene=="World_PC" || _debug)
                {
                    _all_Button.Active=false;
                    _complete_Button.Active=false;
                    _incomplete_Button.Active=false;
                    _inProgress_Button.Active=false;
                    _hide_Button.Active=false;
                    _show_Button.Active=false;

                    if (_isOpen)
                    {
                        UnityEngine.RectTransform canvas = UITools.UIUtility.CanvasRectTransform;
                        UnityEngine.Vector2 canvasSize = new UnityEngine.Vector2(canvas.rect.width * canvas.lossyScale.x, canvas.rect.height * canvas.lossyScale.y);
                        System.Text.StringBuilder display = new System.Text.StringBuilder();
                        System.Collections.Generic.Dictionary<SFS.Logs.Challenge, (int i, string progressData)> progress=null;
                        System.Collections.Generic.HashSet<string> completeChallenges_File=null;
                        System.Collections.Generic.HashSet<SFS.Logs.Challenge> completeChallenges_Rocket=null;
                        System.Collections.Generic.HashSet<string> completeChallenges_LogManager=null;

                        if (scene=="World_PC")
                        {
                            if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                            {
                                completeChallenges_Rocket= rocket.stats.challengeRecorder.GetCompleteChallenges();

                                progress= (System.Collections.Generic.Dictionary<SFS.Logs.Challenge, (int i, string progressData)>)
                                    typeof(SFS.Stats.ChallengeRecorder)
                                        .GetField("progress", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                                        .GetValue(rocket.stats.challengeRecorder);
                            }

                            if (SFS.Stats.LogManager.main!=null)
                                completeChallenges_LogManager=SFS.Stats.LogManager.main.completeChallenges;
                        }

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
                                completeChallenges_File = completeChallenges_List.ToHashSet();
                            }
                        }

    //~                     display.AppendFormat("completeChallenges_File.Count={0:D}",completeChallenges_File.Count).AppendLine();
    //~                     if (completeChallenges_Rocket!=null) display.AppendFormat("completeChallenges_Rocket.Count={0:D}",completeChallenges_Rocket.Count).AppendLine();

                        if (!_debug) _filter="InProgress";
                        if (progress==null && _filter=="InProgress") _filter="All";
                        if (completeChallenges_File==null && _filter=="Complete") _filter="All";
                        if (completeChallenges_File==null && _filter=="Incomplete") _filter="All";

                        System.Collections.Generic.List<SFS.Logs.Challenge> challenges = new System.Collections.Generic.List<SFS.Logs.Challenge> (SFS.Base.worldBase.challengesArray);
                        challenges.Sort((SFS.Logs.Challenge a, SFS.Logs.Challenge b) => a.id.CompareTo(b.id));

                        foreach (SFS.Logs.Challenge oneChallenge in challenges)
                        {
                            bool isComplete=false;
                            bool isComplete_File=false;
                            bool isComplete_Rocket=false;
                            bool isComplete_LogManager=false;
                            int progressStepNo = 0;
                            string progressState = "";

                            if (oneChallenge.steps!=null && oneChallenge.steps.Count!=0)
                            {
                                if (progress!=null && progress.ContainsKey(oneChallenge))
                                {
                                    progressStepNo=progress[oneChallenge].Item1+1;
                                    progressState=progress[oneChallenge].Item2;
                                }
                                if (completeChallenges_File!=null && completeChallenges_File.Contains(oneChallenge.id))
                                {
                                    isComplete=true;
                                    isComplete_File=true;
                                }
                                if (completeChallenges_Rocket!=null && completeChallenges_Rocket.Contains(oneChallenge))
                                {
                                    isComplete_Rocket=true;
                                }
                                if (completeChallenges_LogManager!=null && completeChallenges_LogManager.Contains(oneChallenge.id))
                                {
                                    isComplete=true;
                                    isComplete_LogManager=true;
                                }
                            }

                            if
                                (
                                    _filter=="All"
                                    || (_filter=="InProgress" && !isComplete && (progressStepNo!=0 || isComplete_Rocket))
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

                                        if (_debug)
                                        {
                                            System.Text.StringBuilder indicators = new System.Text.StringBuilder();

                                            if (isComplete_File)
                                            {
                                                if (indicators.Length>0) indicators.Append(", ");
                                                indicators.Append("file");
                                            }

                                            if (isComplete_Rocket)
                                            {
                                                if (indicators.Length>0) indicators.Append(", ");
                                                indicators.Append("rocket");
                                            }

                                            if (isComplete_LogManager)
                                            {
                                                if (indicators.Length>0) indicators.Append(", ");
                                                indicators.Append("log manager");
                                            }

                                            display.AppendFormat(" ({0})", indicators.ToString());
                                        }
                                    }
                                    else if (isComplete_Rocket)
                                    {
                                        display.Append(", awaiting recovery");
                                    }
                                    display.Append(")");

                                    foreach (SFS.Logs.ChallengeStep oneStep in oneChallenge.steps)
                                    {
                                        display.AppendLine();

                                        if (oneStep is CustomChallengesMod.CustomSteps.Step_OneOf oneOneOfStep)
                                        {
                                            display.AppendFormat("... {0:D}: {1}", stepNo, oneOneOfStep.ToStringExt(progress:progressState));

                                            if
                                                (
                                                   isComplete_Rocket
                                                   ||
                                                   (
                                                        (string.IsNullOrEmpty(progressState) || stepNo!=progressStepNo)
                                                        && stepNo<=progressStepNo
                                                    )
                                                )
                                            {
                                                display.AppendLine();
                                                display.Append("... (complete)");
                                            }
                                        }
                                        else if(oneStep is CustomChallengesMod.CustomSteps.Step_AllOf oneAllOfStep)
                                        {
                                            display.AppendFormat("... {0:D}: {1}", stepNo, oneAllOfStep.ToStringExt(progress:progressState));

                                            if
                                                (
                                                   isComplete_Rocket
                                                   ||
                                                   (
                                                        (string.IsNullOrEmpty(progressState) || stepNo!=progressStepNo)
                                                        && stepNo<=progressStepNo
                                                    )
                                                )
                                            {
                                                display.AppendLine();
                                                display.Append("... (complete)");
                                            }
                                        }
                                        else if(oneStep is CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt oneAny_LandmarkStep)
                                        {
                                            display.AppendFormat("... {0:D}: {1}", stepNo, oneAny_LandmarkStep.ToStringExt(progress:progressState));

                                            if
                                                (
                                                   isComplete_Rocket
                                                   ||
                                                   (
                                                        (string.IsNullOrEmpty(progressState) || stepNo!=progressStepNo)
                                                        && stepNo<=progressStepNo
                                                    )
                                                )
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
                                            else if (isComplete_Rocket || stepNo<=progressStepNo)
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
                        string[] lines =display.ToString().Split(new string[]{System.Environment.NewLine},System.StringSplitOptions.None);
                        _window.Size=new UnityEngine.Vector2(1200,System.Math.Min(90f+lines.Length*40f,canvasSize.y*1.8f));
                        _window.Title=_debug?"Custom Challenges Debug":"Progress";
                        _hide_Button.Active=true;

                        if (_debug)
                        {
                            _all_Button.Active=true;
                            _complete_Button.Active=true;
                            _incomplete_Button.Active=true;
                            if (scene=="World_PC") _inProgress_Button.Active=true;

                            _all_Button.Text=_all_Button.Text.ToLower();
                            _inProgress_Button.Text=_inProgress_Button.Text.ToLower();
                            _complete_Button.Text=_complete_Button.Text.ToLower();
                            _incomplete_Button.Text=_incomplete_Button.Text.ToLower();

                            switch (_filter)
                            {
                                case "All":_all_Button.Text=_all_Button.Text.ToUpper();break;
                                case "InProgress":_inProgress_Button.Text=_inProgress_Button.Text.ToUpper();break;
                                case "Complete":_complete_Button.Text=_complete_Button.Text.ToUpper();break;
                                case "Incomplete": _incomplete_Button.Text=_incomplete_Button.Text.ToUpper();break;
                            }
                        }
                        for (int lineIndex=0; lineIndex<lines.Length; lineIndex++)
                        {
                            SFS.UI.ModGUI.Label oneDisplay_Label=null;

                            if (lineIndex<display_Labels.Count)
                            {
                                oneDisplay_Label=display_Labels[lineIndex];
                                oneDisplay_Label.Active=true;
                            }
                            else
                            {
                                oneDisplay_Label=SFS.UI.ModGUI.Builder.CreateLabel
                                    (
                                        _window
                                        ,1180
                                        ,30
                                    );
                                oneDisplay_Label.AutoFontResize=true;
                                oneDisplay_Label.FontSize=25;
                                oneDisplay_Label.TextAlignment = TMPro.TextAlignmentOptions.TopLeft;
                                display_Labels.Add(oneDisplay_Label);
                            }

                            oneDisplay_Label.Text=lines[lineIndex];
                        }

                        if (lines.Length<display_Labels.Count)
                            for (int lineIndex=lines.Length; lineIndex<display_Labels.Count; lineIndex++)
                                display_Labels[lineIndex].Active=false;
                    }
                    else
                    {
                        _window.Size=new UnityEngine.Vector2(160f,90f);
                        _window.Title=_debug?"CC Debug":"Progress";
                        _show_Button.Active=true;
                    }
                }
            }

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
                    Display();
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
            private void Refresh()
            {
                string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

                if (_isActive && scene=="World_PC") Display();
            }

            private void Refresh1()
            {
                string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

                if (_isActive && scene!="World_PC") Display();
            }

            private void Start()
            {
                base.InvokeRepeating("Refresh", UnityEngine.Random.Range(0.5f, 1.5f), 1f);
                base.Invoke("Refresh1", 0.25f);
                base.Invoke("Refresh1", 1f);
                base.Invoke("Refresh1", 2f);
            }

            /// <summary>Show the GUI</summary>
            public static void ShowGUI()
            {
                UnityEngine.Vector2Int pos = SettingsManager.settings.windowPosition;
                _isActive = false;
                _debug =  SettingsManager.settings.debug;
                _isOpen = false;

                // Create the window holder, attach it to the currently active scene so it's removed when the scene changes.
                _windowHolder = SFS.UI.ModGUI.Builder.CreateHolder(SFS.UI.ModGUI.Builder.SceneToAttach.CurrentScene, "CustomChallengesMod GUI Holder");
                _window = SFS.UI.ModGUI.Builder.CreateWindow
                    (
                        _windowHolder.transform
                        ,_mainWindowID
                        ,160
                        ,90
                        ,posX: pos.x
                        ,posY: pos.y
                        ,draggable: true
                        ,savePosition: true
                        ,opacity: 0.95f
                    );

                // Create a layout group for the window. This will tell the GUI builder how it should position elements of your UI.
                _window.CreateLayoutGroup(SFS.UI.ModGUI.Type.Vertical, UnityEngine.TextAnchor.UpperCenter,10f);
                _window.EnableScrolling(SFS.UI.ModGUI.Type.Vertical);

                _window.gameObject.GetComponent<SFS.UI.DraggableWindowModule>().OnDropAction += () =>
                {
                    CustomChallengesMod.SettingsManager.settings.windowPosition = UnityEngine.Vector2Int.RoundToInt(_window.Position);
                    CustomChallengesMod.SettingsManager.Save();
                };

                SFS.UI.ModGUI.Container buttons_Container =  SFS.UI.ModGUI.Builder.CreateContainer(_window);
                buttons_Container.CreateLayoutGroup(SFS.UI.ModGUI.Type.Horizontal, UnityEngine.TextAnchor.MiddleCenter,5f);

                _all_Button=SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 150, 30,onClick:DisplayAll, text: "all");
                _inProgress_Button=SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 150, 30,onClick:DisplayInProgress, text: "in progress");
                _complete_Button=SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 150, 30,onClick:DisplayComplete, text: "complete");
                _incomplete_Button=SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 150, 30,onClick:DisplayIncomplete, text: "incomplete");
                _hide_Button=SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 150, 30,onClick:Hide, text: "Hide");
                _show_Button=SFS.UI.ModGUI.Builder.CreateButton(buttons_Container, 100, 30, onClick:Show, text: "Show");

                _isActive = true;
                Display();
            }

            /// <summary>Note that the GUI is no longer active</summary>
            public static void GUIInActive()
            {
                _isActive = false;
                display_Labels.Clear();
            }
        #endregion
    }
}
