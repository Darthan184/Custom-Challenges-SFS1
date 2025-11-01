using System.Linq; // contains extensions
using HarmonyLib; // contains extensions
using UnityEngine; // contains extensions

namespace CustomChallengesMod
{
    [HarmonyLib.HarmonyPatch(typeof(SFS.Logs.Challenge), "CollectChallenges")]
    class WorldBase_PlanetLoader
    {
        private class _InternalException : System.Exception
        {
            public _InternalException(string message):base(message) {}
        }

        private struct _ChallengeIntermediate
        {
            public int displayPriority;
            public string id;
            public SFS.WorldBase.Planet owner;
            public UnityEngine.Sprite icon;
            public System.Func<string> title;
            public System.Func<string> description;
            public SFS.Logs.Difficulty difficulty;
            public bool returnSafely;
            public System.Collections.Generic.List<SFS.Logs.ChallengeStep> steps;
        }

        static void Postfix(ref System.Collections.Generic.List<SFS.Logs.Challenge> __result)
        {
            System.Collections.Generic.List<SFS.Logs.ChallengeStep>
                GetSteps(string systemName, string id, CustomChallengesMod.CustomChallengesData.Step[] inputSteps)
            {
                System.Collections.Generic.List<SFS.Logs.ChallengeStep> outputSteps = new System.Collections.Generic.List<SFS.Logs.ChallengeStep>();
                int stepCount = 1;

                foreach (CustomChallengesMod.CustomChallengesData.Step oneInputStep in inputSteps)
                {
                    SFS.WorldBase.Planet planet=null;
                    string stepID = string.Format("{0}/{1:D}", id, stepCount++);

                    if (oneInputStep.stepType.ToLower().Trim() != "multi")
                    {
                        if (oneInputStep.planetName.Trim()=="")
                        {
                            throw new _InternalException
                            (
                                string.Format
                                    (
                                        "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a missing planetName field"
                                        , systemName
                                        , stepID
                                    )
                            );
                        }

                        if (!SFS.Base.planetLoader.planets.ContainsKey(oneInputStep.planetName.Trim()))
                        {
                            throw new _InternalException
                            (
                                string.Format
                                    (
                                        "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an planetName field referring to a non-existent planet \"{1}\""
                                        ,systemName
                                        , stepID
                                        ,oneInputStep.planetName.Trim()
                                    )
                            );
                        }
                        planet=SFS.Base.planetLoader.planets[oneInputStep.planetName.Trim()];
                    }

                    if (oneInputStep.stepType.Trim()=="")
                    {
                        throw new _InternalException
                        (
                            string.Format
                                (
                                    "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a missing stepType field"
                                    , systemName
                                    , stepID
                                )
                        );
                    }

                    switch(oneInputStep.stepType.ToLower().Trim())
                    {
                        case "multi":
                        {
                            SFS.Logs.MultiStep oneOutputStep = new SFS.Logs.MultiStep();
                            oneOutputStep.steps=GetSteps(systemName, stepID, oneInputStep.steps);
                            outputSteps.Add(oneOutputStep);
                        }
                        break;

                        case "any_landmarks":
                        {
                            SFS.Logs.Step_Any_Landmarks oneOutputStep = new SFS.Logs.Step_Any_Landmarks();
                            oneOutputStep.planet=planet;
                            oneOutputStep.count=oneInputStep.count;
                            outputSteps.Add(oneOutputStep);
                        }
                        break;

                        case "downrange":
                        {
                            SFS.Logs.Step_Downrange oneOutputStep = new SFS.Logs.Step_Downrange();
                            oneOutputStep.planet=planet;
                            oneOutputStep.downrange=oneInputStep.downrange;
                            outputSteps.Add(oneOutputStep);
                        }
                        break;

                        case "height":
                        {
                            SFS.Logs.Step_Height oneOutputStep = new SFS.Logs.Step_Height();
                            oneOutputStep.planet=planet;
                            oneOutputStep.height=oneInputStep.height;
                            oneOutputStep.checkVelocity=oneInputStep.checkVelocity;
                            outputSteps.Add(oneOutputStep);
                        }
                        break;

                        case "impact":
                        {
                            SFS.Logs.Step_Impact oneOutputStep = new SFS.Logs.Step_Impact();
                            oneOutputStep.planet=planet;
                            oneOutputStep.impactVelocity=oneInputStep.impactVelocity;
                            outputSteps.Add(oneOutputStep);
                        }
                        break;

                        case "land":
                        {
                            SFS.Logs.Step_Land oneOutputStep = new SFS.Logs.Step_Land();
                            oneOutputStep.planet=planet;
                            outputSteps.Add(oneOutputStep);
                        }
                        break;

                        case "orbit":
                        {
                            SFS.Logs.Step_Orbit oneOutputStep = new SFS.Logs.Step_Orbit();
                            oneOutputStep.planet=planet;

                            switch(oneInputStep.orbitType.ToLower().Trim())
                            {
                                case "none": oneOutputStep.orbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.None ; break;
                                case "esc": oneOutputStep.orbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.Esc ; break;
                                case "sub": oneOutputStep.orbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.Sub ; break;
                                case "high": oneOutputStep.orbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.High ; break;
                                case "trans": oneOutputStep.orbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.Trans ; break;
                                case "low": oneOutputStep.orbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.Low ; break;
                                default:
                                {
                                    throw new _InternalException
                                    (
                                        string.Format
                                            (
                                                "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an invalid orbitType field: \"{2}\""
                                                ,systemName
                                                ,stepID
                                                ,oneInputStep.orbitType
                                            )
                                    );
                                }
                            }
                            outputSteps.Add(oneOutputStep);
                        }
                        break;

                        default:
                        {
                            throw new _InternalException
                            (
                                string.Format
                                    (
                                        "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an invalid stepType field: \"{2}\""
                                        , systemName
                                        , stepID
                                        ,oneInputStep.stepType
                                    )
                            );
                        }
                    }
                }
                return outputSteps;
            }

            // Load a PNG or JPG image from disk to a UnityEngine.Texture2D, assign this texture to a new sprite and return its reference
            UnityEngine.Sprite LoadNewSprite(string FilePath, float pixelsPerUnit = 100.0f)
            {
                UnityEngine.Texture2D spriteTexture = LoadTexture(FilePath);
                if (spriteTexture==null)
                {
                    return null;
                }
                else
                {
                    return UnityEngine.Sprite.Create(spriteTexture, new UnityEngine.Rect(0, 0, spriteTexture.width, spriteTexture.height),new UnityEngine.Vector2(0,0), pixelsPerUnit);
                }
            }

            // Load a PNG or JPG file from disk to a UnityEngine.Texture2D
            // Returns null if load fails
            UnityEngine.Texture2D LoadTexture(string filePath)
            {
                UnityEngine.Texture2D tex2D;
                byte[] fileData;

                if (System.IO.File.Exists(filePath))
                {
                    fileData = System.IO.File.ReadAllBytes(filePath);

                    // Create new "empty" texture
                    tex2D = new UnityEngine.Texture2D(2, 2);

                    // Load the imagedata into the texture (size is set automatically)
                    if (tex2D.LoadImage(fileData)) return tex2D;
                }
                return null;
            }

            string traceID="T-01";

            try
            {
                SFS.WorldBase.SolarSystemReference solarSystem =  SFS.Base.worldBase.settings.solarSystem;
                System.Collections.Generic.List<CustomChallengesMod.CustomChallengesData> custom_Challenges =
                    new  System.Collections.Generic.List<CustomChallengesMod.CustomChallengesData>();

                System.Collections.Generic.Dictionary<string, SFS.Logs.Challenge> challengesById = new  System.Collections.Generic.Dictionary<string, SFS.Logs.Challenge>();
                if (UnityEngine.Application.isEditor)
                {
                    ResourcesLoader.main = UnityEngine.Object.FindObjectOfType<ResourcesLoader>();
                }
                ResourcesLoader.ChallengeIcons challengeIcons = ResourcesLoader.main.challengeIcons;

                if (solarSystem.name.Length > 0)
                {
                    SFS.IO.FilePath filePath = FileLocations.SolarSystemsFolder.Extend(solarSystem.name).ExtendToFile("Custom_Challenges.txt");

                    if
                        (
                            filePath.FileExists()
                            && !SFS.Parsers.Json.JsonWrapper.TryLoadJson
                                <System.Collections.Generic.List<CustomChallengesMod.CustomChallengesData>>
                                    (filePath, out custom_Challenges)
                        )
                    {
                        UnityEngine.Debug.LogErrorFormat
                            ("[CustomChallengesMod.CollectChallenges.Postfix] Solar system \"{0}\" has an invalid Custom_Challenges.txt file", solarSystem.name);
                        return;
                    }
                }

                traceID="T-02";

                // populate dictionary for existing challenges
                foreach (SFS.Logs.Challenge oneChallenge in  __result) challengesById[oneChallenge.id]=oneChallenge;

                traceID="T-03";

                if (custom_Challenges!=null)
                {
                    // copy the list as read into a dictionary, ignoring invalid items
                    foreach (CustomChallengesMod.CustomChallengesData oneInputChallenge in custom_Challenges)
                    {
                        _ChallengeIntermediate oneOutputChallenge=new _ChallengeIntermediate();

                        try
                        {
                            bool hasData = true;

                            if (oneInputChallenge.id.Trim()=="")
                            {
                                throw new _InternalException
                                (
                                    string.Format
                                        ("Solar system \"{0}\" Custom_Challenges.txt file has a missing id field", solarSystem.name)
                                );
                            }
                            oneOutputChallenge.id=oneInputChallenge.id.Trim();

                            string icon= oneInputChallenge.icon.ToLower().Trim();

                            switch (icon)
                            {
                                case "":hasData=false;break;
                                case "firstflight":oneOutputChallenge.icon=challengeIcons.firstFlight;break;
                                case "10km":oneOutputChallenge.icon=challengeIcons.icon_10Km;break;
                                case "30km":oneOutputChallenge.icon=challengeIcons.icon_30Km;break;
                                case "50km":oneOutputChallenge.icon=challengeIcons.icon_50Km;break;
                                case "downrange":oneOutputChallenge.icon=challengeIcons.icon_Downrange;break;
                                case "reach_orbit":oneOutputChallenge.icon=challengeIcons.icon_Reach_Orbit;break;
                                case "orbit_high":oneOutputChallenge.icon=challengeIcons.icon_Orbit_High;break;
                                case "capture":oneOutputChallenge.icon=challengeIcons.icon_Capture;break;
                                case "tour":oneOutputChallenge.icon=challengeIcons.icon_Tour;break;
                                case "crash":oneOutputChallenge.icon=challengeIcons.icon_Crash;break;
                                case "unmannedlanding":oneOutputChallenge.icon=challengeIcons.icon_UnmannedLanding;break;
                                case "mannedlanding":oneOutputChallenge.icon=challengeIcons.icon_MannedLanding;break;
                                default:
                                {
                                    if (icon.EndsWith(".png"))
                                    {
                                        UnityEngine.Sprite sprite=LoadNewSprite
                                            (
                                                FileLocations.SolarSystemsFolder.Extend(solarSystem.name)
                                                    .ExtendToFile("Custom_Challenge_Icons/" + icon)
                                            );

                                        if (sprite==null)
                                        {
                                            string.Format
                                                (
                                                    "Solar system \"{0}\" Custom_Challenges.txt file id:{1} cannot find icon file: \"{2}\""
                                                    ,solarSystem.name
                                                    ,oneOutputChallenge.id
                                                    ,"Custom_Challenge_Icons/" + icon
                                                );
                                        }

                                        oneOutputChallenge.icon=sprite;
                                    }
                                    else
                                    {
                                        throw new _InternalException
                                            (
                                                string.Format
                                                    (
                                                        "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an invalid icon field: \"{2}\""
                                                        ,solarSystem.name
                                                        ,oneOutputChallenge.id
                                                        ,oneInputChallenge.icon
                                                    )
                                            );
                                    }
                                }
                                break;
                            }

                            if (hasData)
                            {
                                if (oneInputChallenge.ownerName.Trim()=="")
                                {
                                    throw new _InternalException
                                    (
                                        string.Format
                                            (
                                                "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a missing ownerName field"
                                                ,oneOutputChallenge.id
                                                , solarSystem.name
                                            )
                                    );
                                }

                                if (!SFS.Base.planetLoader.planets.ContainsKey(oneInputChallenge.ownerName.Trim()))
                                {
                                    throw new _InternalException
                                    (
                                        string.Format
                                            (
                                                "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an ownerName field referring to a non-existent planet \"{1}\""
                                                ,solarSystem.name
                                                ,oneOutputChallenge.id
                                                ,oneInputChallenge.ownerName.Trim()
                                            )
                                    );
                                }
                                oneOutputChallenge.owner=SFS.Base.planetLoader.planets[oneInputChallenge.ownerName.Trim()];

                                oneOutputChallenge.displayPriority=(100*oneInputChallenge.priority - challengesById.Count);

                                if (oneInputChallenge.title.Trim()=="")
                                {
                                    throw new _InternalException
                                        (
                                            string.Format
                                                (
                                                    "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a missing title field"
                                                    ,solarSystem.name
                                                    ,oneOutputChallenge.id
                                                )
                                        );
                                }

                                string title=oneInputChallenge.title;
                                oneOutputChallenge.title=new System.Func<string>(() => title);

                                if (oneInputChallenge.description.Trim()=="")
                                {
                                    throw new _InternalException
                                        (
                                            string.Format
                                                (
                                                    "Solar system \"{0}\" Custom_Challenges.txt file id:{1}  has a missing description field"
                                                    ,solarSystem.name
                                                    ,oneOutputChallenge.id
                                                )
                                        );
                                }

                                string description=oneInputChallenge.description;
                                oneOutputChallenge.description=new System.Func<string>(() => description);

                                switch (oneInputChallenge.difficulty.ToLower().Trim())
                                {
                                    case "easy":oneOutputChallenge.difficulty=SFS.Logs.Difficulty.Easy;break;
                                    case "medium":oneOutputChallenge.difficulty=SFS.Logs.Difficulty.Medium;break;
                                    case "hard":oneOutputChallenge.difficulty=SFS.Logs.Difficulty.Hard;break;
                                    case "extreme":oneOutputChallenge.difficulty=SFS.Logs.Difficulty.Extreme;break;
                                    default:
                                        throw new _InternalException
                                            (
                                                string.Format
                                                    (
                                                        "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an invalid difficulty field: \"{2}\""
                                                        ,solarSystem.name
                                                        ,oneOutputChallenge.id
                                                        ,oneInputChallenge.difficulty
                                                    )
                                            );
                                }

                                oneOutputChallenge.returnSafely = oneInputChallenge.returnSafely;
                                oneOutputChallenge.steps = GetSteps(solarSystem.name,oneOutputChallenge.id,oneInputChallenge.steps);
                            }

                            if (hasData)
                            {
                                challengesById[oneOutputChallenge.id]=new SFS.Logs.Challenge
                                    (
                                        oneOutputChallenge.displayPriority
                                        ,oneOutputChallenge.id
                                        ,oneOutputChallenge.owner
                                        ,oneOutputChallenge.icon
                                        ,oneOutputChallenge.title
                                        ,oneOutputChallenge.description
                                        ,oneOutputChallenge.difficulty
                                        ,oneOutputChallenge.returnSafely
                                        ,oneOutputChallenge.steps
                                    );
                            }
                            else if (challengesById.ContainsKey(oneOutputChallenge.id))
                            {
                                challengesById.Remove(oneOutputChallenge.id);
                            }
                        }
                        catch (_InternalException excp)
                        {
                            UnityEngine.Debug.LogErrorFormat("[CustomChallengesMod.CollectChallenges.Postfix] {0}",excp.ToString());
                        }
                    }
                }
                traceID="T-04";
                __result.Clear();
                traceID="T-05";
                foreach (SFS.Logs.Challenge oneChallenge in  challengesById.Values)
                {
                    __result.Add(oneChallenge);
                }
                traceID="T-06";
                __result.Sort((SFS.Logs.Challenge a, SFS.Logs.Challenge b) => b.displayPriority.CompareTo(a.displayPriority));
            }
            catch (System.Exception excp)
            {
                UnityEngine.Debug.LogErrorFormat("[CustomChallengesMod.CollectChallenges.Postfix-{0}] {1}",traceID,excp.ToString());
                return;
            }
        }
    }
}