using System.Linq; // contains extensions
using HarmonyLib; // contains extensions
using UnityEngine; // contains extensions

namespace CustomChallengesMod
{
    [HarmonyLib.HarmonyPatch(typeof(SFS.Logs.ChallengeStep), "IsEligible")]
    class SFS_Logs_ChallengeStep_IsEligible
    {
        /// <summary>Attempt to simulate a virtual function</summary>
        static bool Prefix(ref bool __result,SFS.WorldBase.Planet currentPlanet,SFS.Logs.ChallengeStep __instance)
        {
            if ( __instance is CustomChallengesMod.CustomSteps.Step_OneOf step_OneOf)
            {
                __result = step_OneOf.My_IsEligible(currentPlanet);
                return false;
            }
            else if ( __instance is CustomChallengesMod.CustomSteps.Step_AllOf step_AllOf)
            {
                __result = step_AllOf.My_IsEligible(currentPlanet);
                return false;
            }
            return true;
        }
    }

    [HarmonyLib.HarmonyPatch(typeof(SFS.Logs.Challenge), "CollectChallenges")]
    class SFS_Logs_Challenge_CollectChallenges
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

        /// <summary>Get a distance from the supplied values</summary>
        private static double GetDistance(SFS.WorldBase.Planet planet,string systemName, string stepID, string fieldName,string value)
        {
            value =value.Trim();

            if (value=="") return double.NaN;

            try
            {
                switch (value.Substring(value.Length-1).ToLower())
                {
                    case "k": return double.Parse(value.Substring(0,value.Length-1))*1e3;
                    case "m": return double.Parse(value.Substring(0,value.Length-1))*1e6;
                    case "g": return double.Parse(value.Substring(0,value.Length-1))*1e9;
                    case "t": return double.Parse(value.Substring(0,value.Length-1))*1e12;
                    case "l": return double.Parse(value.Substring(0,value.Length-1))*86400.0*365.2425*299762458.0;
                    case "r": return double.Parse(value.Substring(0,value.Length-1))*planet.Radius;
                    case "s": return double.Parse(value.Substring(0,value.Length-1))*planet.SOI-planet.Radius;
                    case "a": return double.Parse(value.Substring(0,value.Length-1))
                        * System.Math.Max
                            (
                                planet.AtmosphereHeightPhysics
                                ,System.Math.Max(planet.maxTerrainHeight,planet.data.basics.timewarpHeight)
                            );
                    default: return double.Parse(value);
                }
            }
            catch
            {
                 throw new _InternalException
                (
                    string.Format
                        (
                            "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an invalid {2} field: \"{3}\""
                            , systemName
                            , stepID
                            , fieldName
                            ,value
                        )
                );
            }
       }

        /// <summary>Expand distance values within the supplied string</summary>
        private static string ExpandDistances(SFS.WorldBase.Planet planet,string systemName, string stepID, string fieldName,string value)
        {
            string input =value;
            System.Text.StringBuilder output = new System.Text.StringBuilder();

            while(input.Length>0)
            {
                int start = input.IndexOf("[[");
                int end = 0;
                string[] distanceInputSplit = {};
                double distance;

                if (start<0)
                {
                    output.Append(input);
                    break;
                }

                output.Append(input.Substring(0,start)); // everything left of "[["
                input=input.Substring(start+2); // everything right of "[["

                end = input.IndexOf("]]");

                if (end<0)
                {
                    output.Append("[[" + input);
                    break;
                }


                distanceInputSplit = input.Remove(end).Split(':'); // string between [[ and ]] split on ":"
                input=input.Substring(end+2); // everything right of "]]"

                if (distanceInputSplit.Length==0)
                {
                    continue;
                }
                else if (distanceInputSplit.Length==1)
                {
                    distance=GetDistance(planet,systemName,stepID,"[[...]] item in a " + fieldName,distanceInputSplit[0]);
                }
                else
                {
                    if (!SFS.Base.planetLoader.planets.ContainsKey(distanceInputSplit[1].Trim()))
                    {
                        throw new _InternalException
                        (
                            string.Format
                                (
                                    "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a {2} field containing a [[..:..]] escape with a planet name referring to a non-existent planet \"{3}\""
                                    ,systemName
                                    , stepID
                                    ,fieldName
                                    ,distanceInputSplit[1].Trim()
                                )
                        );
                    }
                    distance=GetDistance
                        (
                            SFS.Base.planetLoader.planets[distanceInputSplit[1].Trim()]
                            ,systemName
                            ,stepID
                            ,"[[...:...]] item in a " + fieldName
                            ,distanceInputSplit[0].Trim()
                        );
                }

                switch((int)System.Math.Floor(System.Math.Log10(distance)/3))
                {
                    case 1:output.AppendFormat("{0:G4} km", distance/1.0e3); break;
                    case 2:output.AppendFormat("{0:G4} Mm", distance/1.0e6); break;
                    case 3:output.AppendFormat("{0:G4} Gm", distance/1.0e9); break;
                    case 4:output.AppendFormat("{0:G4} Tm", distance/1.0e12); break;
                    default:
                        if (distance<1000)
                        {
                            output.AppendFormat("{0:G4} m", distance);
                        }
                        else
                        {
                            output.AppendFormat("{0:G4} ly", distance/(86400.0*365.2425*299762458.0));
                        }
                     break;
                }
            }

            return output.ToString();
        }

        /// <summary>Get the challenge steps from the supplies values</summary>
        private static System.Collections.Generic.List<SFS.Logs.ChallengeStep>
            GetSteps(string systemName, string id, CustomChallengesMod.CustomChallengesData.Step[] inputSteps, int depth=0)
        {
            System.Collections.Generic.List<SFS.Logs.ChallengeStep> outputSteps = new System.Collections.Generic.List<SFS.Logs.ChallengeStep>();
            int stepCount = 1;

            foreach (CustomChallengesMod.CustomChallengesData.Step oneInputStep in inputSteps)
            {
                SFS.WorldBase.Planet planet=null;
                string stepID = string.Format("{0}/{1:D}", id, stepCount++);

                if (oneInputStep.stepType.ToLower().Trim() != "multi" && oneInputStep.stepType.ToLower().Trim() != "any")
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
                                    "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an planetName field referring to a non-existent planet \"{2}\""
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
                        CustomChallengesMod.CustomSteps.Step_AllOf oneOutputStep = new CustomChallengesMod.CustomSteps.Step_AllOf();
                        oneOutputStep.Depth = depth;
                        oneOutputStep.steps=GetSteps(systemName, stepID, oneInputStep.steps,depth+1);
                        outputSteps.Add(oneOutputStep);
                    }
                    break;

                    case "any":
                    {
                        CustomChallengesMod.CustomSteps.Step_OneOf oneOutputStep = new CustomChallengesMod.CustomSteps.Step_OneOf();
                        oneOutputStep.Depth = depth;
                        oneOutputStep.steps=GetSteps(systemName, stepID, oneInputStep.steps,depth+1);
                        outputSteps.Add(oneOutputStep);
                    }
                    break;

                    case "any_landmarks":
                    {
                        CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt oneOutputStep =
                            new CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt();
                        oneOutputStep.planet=planet;
                        oneOutputStep.count=oneInputStep.count;
                        oneOutputStep.hasEngines=oneInputStep.hasEngines;
                        oneOutputStep.minMass=oneInputStep.minMass;
                        oneOutputStep.maxMass=oneInputStep.maxMass;
                        oneOutputStep.Depth = depth;
                        outputSteps.Add(oneOutputStep);
                    }
                    break;

                    case "downrange":
                    {
                        CustomChallengesMod.CustomSteps.Step_Downrange oneOutputStep = new CustomChallengesMod.CustomSteps.Step_Downrange();
                        oneOutputStep.planet=planet;
                        oneOutputStep.downrange=(int)GetDistance(planet,systemName,stepID,"downrange",oneInputStep.downrange);
                        outputSteps.Add(oneOutputStep);
                    }
                    break;

                    case "height":
                    {
                        CustomChallengesMod.CustomSteps.Step_HeightExt oneOutputStep =
                            new CustomChallengesMod.CustomSteps.Step_HeightExt();
                        oneOutputStep.planet=planet;
                        oneOutputStep.hasEngines=oneInputStep.hasEngines;
                        oneOutputStep.minHeight=GetDistance(planet,systemName,stepID,"minHeight",oneInputStep.minHeight);
                        oneOutputStep.minMass=oneInputStep.minMass;
                        oneOutputStep.maxHeight=GetDistance(planet,systemName,stepID,"maxHeight",oneInputStep.maxHeight);
                        oneOutputStep.maxMass=oneInputStep.maxMass;
                        outputSteps.Add(oneOutputStep);
                    }
                    break;

                    case "impact":
                    {
                        CustomChallengesMod.CustomSteps.Step_Impact oneOutputStep = new CustomChallengesMod.CustomSteps.Step_Impact();
                        oneOutputStep.planet=planet;
                        oneOutputStep.impactVelocity=oneInputStep.impactVelocity;
                        outputSteps.Add(oneOutputStep);
                    }
                    break;

                    case "land":
                    {
                        CustomChallengesMod.CustomSteps.Step_LandExt oneOutputStep =
                            new  CustomChallengesMod.CustomSteps.Step_LandExt();
                        oneOutputStep.planet=planet;
                        oneOutputStep.hasEngines=oneInputStep.hasEngines;
                        oneOutputStep.minMass=oneInputStep.minMass;
                        oneOutputStep.maxMass=oneInputStep.maxMass;
                        outputSteps.Add(oneOutputStep);
                    }
                    break;

                    case "customorbit":
                    {
                        CustomChallengesMod.CustomSteps.Step_CustomOrbit oneOutputStep =
                            new  CustomChallengesMod.CustomSteps.Step_CustomOrbit();
                        oneOutputStep.planet=planet;
                        oneOutputStep.hasEngines=oneInputStep.hasEngines;

                        oneOutputStep.maxApoapsis=GetDistance(planet,systemName,stepID,"maxApoapsis",oneInputStep.maxApoapsis)+planet.Radius;
                        oneOutputStep.maxEcc=oneInputStep.maxEcc;
                        oneOutputStep.maxMass=oneInputStep.maxMass;
                        oneOutputStep.maxPeriapsis=GetDistance(planet,systemName,stepID,"maxPeriapsis",oneInputStep.maxPeriapsis)+planet.Radius;
                        oneOutputStep.maxSma=GetDistance(planet,systemName,stepID,"maxSma",oneInputStep.maxSma)+planet.Radius;

                        oneOutputStep.minApoapsis=GetDistance(planet,systemName,stepID,"minApoapsis",oneInputStep.minApoapsis)+planet.Radius;
                        oneOutputStep.minEcc=oneInputStep.minEcc;
                        oneOutputStep.minMass=oneInputStep.minMass;
                        oneOutputStep.minPeriapsis=GetDistance(planet,systemName,stepID,"minPeriapsis",oneInputStep.minPeriapsis)+planet.Radius;
                        oneOutputStep.minSma=GetDistance(planet,systemName,stepID,"minSma",oneInputStep.minSma)+planet.Radius;

                        outputSteps.Add(oneOutputStep);
                    }
                    break;

                    case "orbit":
                    {
                        SFS.Stats.StatsRecorder.Tracker.State_Orbit outputOrbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.None;

                        switch(oneInputStep.orbitType.ToLower().Trim())
                        {
                            case "none": outputOrbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.None ; break;
                            case "esc": outputOrbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.Esc ; break;
                            case "sub": outputOrbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.Sub ; break;
                            case "high": outputOrbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.High ; break;
                            case "trans": outputOrbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.Trans ; break;
                            case "low": outputOrbit=SFS.Stats.StatsRecorder.Tracker.State_Orbit.Low ; break;
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

                        CustomChallengesMod.CustomSteps.Step_OrbitExt oneOutputStep
                            = new CustomChallengesMod.CustomSteps.Step_OrbitExt();
                        oneOutputStep.planet=planet;
                        oneOutputStep.orbit=outputOrbit;
                        oneOutputStep.hasEngines=oneInputStep.hasEngines;
                        oneOutputStep.minMass=oneInputStep.minMass;
                        oneOutputStep.maxMass=oneInputStep.maxMass;
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
        private static UnityEngine.Sprite LoadNewSprite(string FilePath, float pixelsPerUnit = 100.0f)
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
        private static UnityEngine.Texture2D LoadTexture(string filePath)
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

        static void Postfix(ref System.Collections.Generic.List<SFS.Logs.Challenge> __result)
        {
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
                    int itemNo=0;

                    // copy the list as read into a dictionary, ignoring invalid items
                    foreach (CustomChallengesMod.CustomChallengesData oneInputChallenge in custom_Challenges)
                    {
                        _ChallengeIntermediate oneOutputChallenge=new _ChallengeIntermediate();
                        itemNo++;
                        try
                        {
                            bool hasData = true;
                            bool canAdd=false;

                            if (oneInputChallenge.id.Trim()=="")
                            {
                                throw new _InternalException
                                (
                                    string.Format
                                        (
                                            "Solar system \"{0}\" Custom_Challenges.txt file item #{1:N0} has a missing id field"
                                            ,solarSystem.name
                                            ,itemNo
                                        )
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
                                case "land_one_way":oneOutputChallenge.icon=challengeIcons.icon_UnmannedLanding;break;
                                case "land_return":oneOutputChallenge.icon=challengeIcons.icon_MannedLanding;break;
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
                                if (SFS.Base.worldBase!=null && SFS.Base.worldBase.settings!=null && SFS.Base.worldBase.settings.difficulty!=null)
                                {
                                    switch (SFS.Base.worldBase.settings.difficulty.difficulty)
                                    {
                                        case SFS.WorldBase.Difficulty.DifficultyType.Normal:
                                            canAdd=(oneInputChallenge.difficulty.ToLower()=="all" || oneInputChallenge.difficulty.ToLower()=="normal" );
                                        break;

                                        case SFS.WorldBase.Difficulty.DifficultyType.Hard:
                                            canAdd=(oneInputChallenge.difficulty.ToLower()=="all" || oneInputChallenge.difficulty.ToLower()=="hard" );
                                        break;

                                        case SFS.WorldBase.Difficulty.DifficultyType.Realistic:
                                            canAdd=(oneInputChallenge.difficulty.ToLower()=="all" || oneInputChallenge.difficulty.ToLower()=="realistic" );
                                        break;

                                        default:
                                            canAdd=(oneInputChallenge.difficulty.ToLower()=="all");
                                        break;
                                    }
                                }
                                else
                                {
                                    canAdd=(oneInputChallenge.difficulty.ToLower()=="all");
                                }
                            }

                            if (hasData && canAdd)
                            {
                                if (oneInputChallenge.ownerName.Trim()=="")
                                {
                                    throw new _InternalException
                                    (
                                        string.Format
                                            (
                                                "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a missing ownerName field"
                                                ,solarSystem.name
                                                ,oneOutputChallenge.id
                                            )
                                    );
                                }

                                if (!SFS.Base.planetLoader.planets.ContainsKey(oneInputChallenge.ownerName.Trim()))
                                {
                                    throw new _InternalException
                                    (
                                        string.Format
                                            (
                                                "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an ownerName field referring to a non-existent planet \"{2}\""
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

                                string title=ExpandDistances
                                    (
                                        oneOutputChallenge.owner
                                        ,solarSystem.name
                                        ,oneOutputChallenge.id
                                        ,"title"
                                        ,oneInputChallenge.title
                                    );
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

                                string description=ExpandDistances
                                    (
                                        oneOutputChallenge.owner
                                        ,solarSystem.name
                                        ,oneOutputChallenge.id
                                        ,"description"
                                        ,oneInputChallenge.description
                                    );
                                oneOutputChallenge.description=new System.Func<string>(() => description);

                                switch (oneInputChallenge.challengeDifficulty.ToLower().Trim())
                                {
                                    case "easy":oneOutputChallenge.difficulty=SFS.Logs.Difficulty.Easy;break;
                                    case "medium":oneOutputChallenge.difficulty=SFS.Logs.Difficulty.Medium;break;
                                    case "hard":oneOutputChallenge.difficulty=SFS.Logs.Difficulty.Hard;break;
                                    case "extreme":oneOutputChallenge.difficulty=SFS.Logs.Difficulty.Extreme;break;
                                    case "":
                                        throw new _InternalException
                                            (
                                                string.Format
                                                    (
                                                        "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a missing difficulty field"
                                                        ,solarSystem.name
                                                        ,oneOutputChallenge.id
                                                    )
                                            );
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
                                if (canAdd)
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
                            }
                            else if (challengesById.ContainsKey(oneOutputChallenge.id))
                            {
                                challengesById.Remove(oneOutputChallenge.id);
                            }
                        }
                        catch (_InternalException excp)
                        {
                            UnityEngine.Debug.LogErrorFormat("[CustomChallengesMod.CollectChallenges.Postfix] {0}",excp.Message);
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

    [HarmonyLib.HarmonyPatch(typeof(SFS.Stats.ChallengeRecorder), "TryCompleteSteps")]
    class SFS_Stats_ChallengeRecorder_TryCompleteSteps
    {
        static void Postfix(SFS.Stats.ChallengeRecorder __instance)
        {
            __instance.UpdateEligibleSteps();
        }
    }
}