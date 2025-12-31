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

        /// <summary>Get a distance from the supplied value</summary>
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

        /// <summary>Get a mass from the supplied value</summary>
        private static double GetMass(SFS.WorldBase.Planet planet,string systemName, string stepID, string fieldName,string value)
        {
//~             double scale=System.Math.Exp(System.Math.Sqrt(difficulty.DefaultSmaScale*System.Math.Sqrt(difficulty.DryMassMultiplier))/difficulty.IspMultiplier-1);
            double scale=1.0;

            switch(SFS.Base.worldBase.settings.difficulty.difficulty)
            {
                case SFS.WorldBase.Difficulty.DifficultyType.Hard: scale=2.0;break;
                case SFS.WorldBase.Difficulty.DifficultyType.Realistic: scale=4.0;break;
            }

            value =value.Trim();

            if (value=="") return double.NaN;
            if (value.Length==1) return double.Parse(value);

            try
            {
                switch (value.Length>=3?value.Substring(value.Length-2).ToLower():"")
                {
                    case "ln": return double.Parse(value.Substring(0,value.Length-2))*scale;
                    case "on": return double.Parse(value.Substring(0,value.Length-2))/scale;

                    default:
                        switch (value.Substring(value.Length-1).ToLower())
                        {
                            case "k": return double.Parse(value.Substring(0,value.Length-1))*1e3;
                            case "m": return double.Parse(value.Substring(0,value.Length-1))*1e6;
                            case "g": return double.Parse(value.Substring(0,value.Length-1))*1e9;
                            default: return double.Parse(value);
                        }
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

        /// <summary>Get a velocity from the supplied value</summary>
        private static int GetVelocity(SFS.WorldBase.Planet planet,string systemName, string stepID, string fieldName,string value)
        {
            value =value.Trim();

            if (value=="") return 0;
            if (value.Length==1) return int.Parse(value);

            try
            {
                if (value.Length>=3 && value.Substring(value.Length-2).ToLower()=="vn")
                {
                    return (int)
                        (
                            double.Parse(value.Substring(0,value.Length-2))
                            *System.Math.Sqrt(SFS.Base.worldBase.settings.difficulty.DefaultSmaScale)
                        );
                }
                else switch (value.Substring(value.Length-1).ToLower())
                {
                    case "k": return (int)(double.Parse(value.Substring(0,value.Length-1))*1000);
                    case "m": return (int)(double.Parse(value.Substring(0,value.Length-1))*1000000);
                    case "g": return (int)(double.Parse(value.Substring(0,value.Length-1))*1000000000);
                    case "c": return (int)(double.Parse(value.Substring(0,value.Length-1))*299762458);
                    default: return int.Parse(value);
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
        private static string ExpandValues(SFS.WorldBase.Planet planet,string systemName, string stepID, string fieldName,string value)
        {
            string traceID="T-01";

            try
            {
                string input =value;
                System.Text.StringBuilder output = new System.Text.StringBuilder();
                // ###########
//~                 UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.ExpandValues-I-01] {0}",input);
                while(input.Length>0)
                {
                    int start = input.IndexOf("[[");
                    int end = 0;
                    string[] unitValueInputSplit = {};

                    if (start<0)
                    {
                        output.Append(input);
                        break;
                    }

                    output.Append(input.Substring(0,start)); // everything left of "[["
                    traceID="T-02";
                    input=input.Substring(start+2); // everything right of "[["
                    traceID="T-03";

                    end = input.IndexOf("]]");

                    if (end<0)
                    {
                        output.Append("[[" + input);
                        break;
                    }

                    unitValueInputSplit = input.Remove(end).Split(':'); // string between [[ and ]] split on ":"
                    input=input.Substring(end+2); // everything right of "]]"
                    traceID="T-04";

                    if (unitValueInputSplit.Length==0)
                    {
                        continue;
                    }
                    else
                    {
                        SFS.WorldBase.Planet selPlanet = planet;

                        if (unitValueInputSplit.Length>=2)
                        {
                            if (!SFS.Base.planetLoader.planets.ContainsKey(unitValueInputSplit[1].Trim()))
                            {
                                throw new _InternalException
                                (
                                    string.Format
                                        (
                                            "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a {2} field containing a [[..:..]] escape with a planet name referring to a non-existent planet \"{3}\""
                                            ,systemName
                                            , stepID
                                            ,fieldName
                                            ,unitValueInputSplit[1].Trim()
                                        )
                                );
                            }
                            selPlanet = SFS.Base.planetLoader.planets[unitValueInputSplit[1].Trim()];
                        }

                        if (unitValueInputSplit[0].ToLower().EndsWith("mk"))
                        {
                            output.AppendFormat("{0:N0}",System.Math.Min(int.Parse(unitValueInputSplit[0].Substring(0,unitValueInputSplit[0].Length-2)), selPlanet.data.landmarks.Count));
                            traceID="T-05";
                        }
                        else if (unitValueInputSplit[0].ToLower().EndsWith("vn") || unitValueInputSplit[0].ToLower().EndsWith("c"))
                        {
                            int unitValue=GetVelocity(selPlanet,systemName,stepID,"[[...]] item in a " + fieldName,unitValueInputSplit[0]);

                            switch((int)System.Math.Floor(System.Math.Log10(unitValue)))
                            {
                                case 3:
                                case 4:
                                case 5:
                                    output.AppendFormat("{0:G4} km/s", unitValue/1.0e3);
                                break;
                                case 6:
                                case 7:
                                    output.AppendFormat("{0:G4} Mm/s", unitValue/1.0e6);
                                break;
                                default:
                                    if (unitValue<1000)
                                    {
                                        output.AppendFormat("{0:G4} m/s", unitValue);
                                    }
                                    else
                                    {
                                        output.AppendFormat("{0:G4} c", unitValue/299762458.0);
                                    }
                                 break;
                            }
                        }
                        else if (unitValueInputSplit[0].ToLower().EndsWith("n"))
                        {
                            double unitValue=GetMass(selPlanet,systemName,stepID,"[[...]] item in a " + fieldName,unitValueInputSplit[0]);

                            switch((int)System.Math.Floor(System.Math.Log10(unitValue)/3))
                            {
                                case 0:output.AppendFormat("{0:G4} t", unitValue); break;
                                case 1:output.AppendFormat("{0:G4} kt", unitValue/1.0e3); break;
                                case 2:output.AppendFormat("{0:G4} Mt", unitValue/1.0e6); break;
                                default:output.AppendFormat("{0:G4} Gt", unitValue/1.0e9); break;
                            }
                        }
                        else
                        {
                            double unitValue=GetDistance(selPlanet,systemName,stepID,"[[...]] item in a " + fieldName,unitValueInputSplit[0]);

                            switch((int)System.Math.Floor(System.Math.Log10(unitValue)/3))
                            {
                                case 1:output.AppendFormat("{0:G4} km", unitValue/1.0e3); break;
                                case 2:output.AppendFormat("{0:G4} Mm", unitValue/1.0e6); break;
                                case 3:output.AppendFormat("{0:G4} Gm", unitValue/1.0e9); break;
                                case 4:output.AppendFormat("{0:G4} Tm", unitValue/1.0e12); break;
                                default:
                                    if (unitValue<1000)
                                    {
                                        output.AppendFormat("{0:G4} m", unitValue);
                                    }
                                    else
                                    {
                                        output.AppendFormat("{0:G4} ly", unitValue/(86400.0*365.2425*299762458.0));
                                    }
                                 break;
                            }
                        }
                    }
                }
                return output.ToString();
            }
            catch (System.Exception excp)
            {
                UnityEngine.Debug.LogErrorFormat("[CustomChallengesMod.CollectChallenges.ExpandValues-{0}] {1}",traceID,excp.ToString());
                return "";
            }
        }

        /// <summary>Get the data for one challenge</summary>
        private static _ChallengeIntermediate GetChallenge
            (
                string systemName
                ,CustomChallengesMod.CustomChallengesData oneInputChallenge
                ,int sequenceNo
                ,SFS.WorldBase.Planet planet=null
            )
        {
            _ChallengeIntermediate oneOutputChallenge=new _ChallengeIntermediate();

            if (planet==null)
            {
                oneOutputChallenge.id=oneInputChallenge.id.Trim();

                if (oneInputChallenge.ownerName.Trim()=="")
                {
                    throw new _InternalException
                    (
                        string.Format
                            (
                                "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a missing ownerName field"
                                ,systemName
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
                                ,systemName
                                ,oneOutputChallenge.id
                                ,oneInputChallenge.ownerName.Trim()
                            )
                    );
                }
                oneOutputChallenge.owner=SFS.Base.planetLoader.planets[oneInputChallenge.ownerName.Trim()];
            }
            else
            {
                oneOutputChallenge.id=oneInputChallenge.id.Trim().Replace("{planet}", planet.codeName);
                oneOutputChallenge.owner=planet;
            }

            oneOutputChallenge.displayPriority=(100*oneInputChallenge.priority - sequenceNo);

            if (oneInputChallenge.title.Trim()=="")
            {
                throw new _InternalException
                    (
                        string.Format
                            (
                                "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a missing title field"
                                ,systemName
                                ,oneInputChallenge.id
                            )
                    );
            }

            string title=ExpandValues
                (
                    oneOutputChallenge.owner
                    ,systemName
                    ,oneInputChallenge.id
                    ,"title"
                    ,oneInputChallenge.title
                );

            if (planet!=null)
            {
                title=title.Replace("{planet}", planet.codeName);
                if (planet.parentBody!=null) title=title.Replace("{primary}", planet.parentBody.codeName);
            }
            oneOutputChallenge.title=new System.Func<string>(() => title);

            if (oneInputChallenge.description.Trim()=="")
            {
                throw new _InternalException
                    (
                        string.Format
                            (
                                "Solar system \"{0}\" Custom_Challenges.txt file id:{1}  has a missing description field"
                                ,systemName
                                ,oneInputChallenge.id
                            )
                    );
            }

            string description=ExpandValues
                (
                    oneOutputChallenge.owner
                    ,systemName
                    ,oneInputChallenge.id
                    ,"description"
                    ,oneInputChallenge.description
                );

            if (planet!=null)
            {
                description=description.Replace("{planet}", planet.codeName);
                if (planet.parentBody!=null) title=title.Replace("{primary}", planet.parentBody.codeName);
            }
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
                                    ,systemName
                                    ,oneInputChallenge.id
                                )
                        );
                default:
                    throw new _InternalException
                        (
                            string.Format
                                (
                                    "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an invalid difficulty field: \"{2}\""
                                    ,systemName
                                    ,oneInputChallenge.id
                                    ,oneInputChallenge.difficulty
                                )
                        );
            }

            oneOutputChallenge.returnSafely = oneInputChallenge.returnSafely;
            oneOutputChallenge.steps = GetSteps(systemName,oneInputChallenge.id,oneInputChallenge.steps,oneOutputChallenge.owner);
            return oneOutputChallenge;
        }

        /// <summary>Get the challenge steps from the supplied values</summary>
        private static System.Collections.Generic.List<SFS.Logs.ChallengeStep>
            GetSteps
                (
                    string systemName
                    ,string id
                    ,System.Collections.Generic.IEnumerable<CustomChallengesMod.CustomChallengesData.Step> inputSteps
                    ,SFS.WorldBase.Planet owner
                    ,int depth=0
                )
        {
            System.Collections.Generic.List<SFS.Logs.ChallengeStep> outputSteps = new System.Collections.Generic.List<SFS.Logs.ChallengeStep>();
            int stepCount = 1;

            foreach (CustomChallengesMod.CustomChallengesData.Step oneInputStep in inputSteps)
            {
                System.Collections.Generic.List<SFS.WorldBase.Planet> planets=new System.Collections.Generic.List<SFS.WorldBase.Planet>();
                string stepID = string.Format("{0}/{1:D}", id, stepCount++);

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

                    switch(oneInputStep.planetName.Trim())
                    {
                        case "{planet}": planets.Add(owner); break;
                        case "{primary}":
                        {
                            if (owner.parentBody!=null)
                            {
                                planets.Add(owner.parentBody);
                            }
                            else
                            {
                                throw new _InternalException
                                (
                                    string.Format
                                        (
                                            "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a planetName field of \"{{primary}}\" but \"{2}\" does not have a primary"
                                            ,systemName
                                            , stepID
                                            ,owner.codeName
                                        )
                                );
                            }
                        }
                        break;

                        case "{sat}":
                        {
                            if (owner.satellites!=null && owner.satellites.Length>0)
                            {
                                foreach (SFS.WorldBase.Planet oneSatellite in owner.satellites)
                                    if (MatchesFilter(oneSatellite,oneInputStep.filter))
                                    {
                                        planets.Add(oneSatellite);
                                    }
                            }
                            else
                            {
                                throw new _InternalException
                                (
                                    string.Format
                                        (
                                            "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a planetName field of \"{{sat}}\" but \"{2}\" does not have any satellites"
                                            ,systemName
                                            , stepID
                                            ,owner.codeName
                                        )
                                );
                            }
                        }
                        break;

                        default:
                        {
                            if (!SFS.Base.planetLoader.planets.ContainsKey(oneInputStep.planetName.Trim()))
                            {
                                throw new _InternalException
                                (
                                    string.Format
                                        (
                                            "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a planetName field referring to a non-existent planet \"{2}\""
                                            ,systemName
                                            , stepID
                                            ,oneInputStep.planetName.Trim()
                                        )
                                );
                            }

                            planets.Add(SFS.Base.planetLoader.planets[oneInputStep.planetName.Trim()]);
                        }
                        break;
                    }
                }

                switch(oneInputStep.stepType.ToLower().Trim())
                {
                    case "multi":
                    {
                        CustomChallengesMod.CustomSteps.Step_AllOf oneOutputStep = new CustomChallengesMod.CustomSteps.Step_AllOf();
                        oneOutputStep.Depth = depth;
                        oneOutputStep.steps=GetSteps(systemName, stepID, oneInputStep.steps,owner,depth+1);
                        outputSteps.Add(oneOutputStep);
                    }
                    break;

                    case "any":
                    {
                        CustomChallengesMod.CustomSteps.Step_OneOf oneOutputStep = new CustomChallengesMod.CustomSteps.Step_OneOf();
                        oneOutputStep.Depth = depth;
                        oneOutputStep.steps=GetSteps(systemName, stepID, oneInputStep.steps,owner,depth+1);
                        outputSteps.Add(oneOutputStep);
                    }
                    break;

                    default:
                        foreach (SFS.WorldBase.Planet onePlanet in planets)
                        {
                            switch(oneInputStep.stepType.ToLower().Trim())
                            {
                                case "any_landmarks":
                                {
                                    if (onePlanet.data.landmarks==null || onePlanet.data.landmarks.Count==0)
                                    {
                                        throw new _InternalException
                                        (
                                            string.Format
                                                (
                                                    "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has a stepType field of \"Any_Landmarks\" but planet \"{2}\" has no landmarks"
                                                    , systemName
                                                    , stepID
                                                    ,onePlanet.codeName
                                                )
                                        );
                                    }
                                    CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt oneOutputStep =
                                        new CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt();
                                    oneOutputStep.planet=onePlanet;
                                    oneOutputStep.count=System.Math.Min(oneInputStep.count, onePlanet.data.landmarks.Count);
                                    oneOutputStep.hasEngines=oneInputStep.hasEngines;
                                    oneOutputStep.minMass=GetMass(onePlanet,systemName,stepID,"minMass",oneInputStep.minMass);
                                    oneOutputStep.maxMass=GetMass(onePlanet,systemName,stepID,"maxMass",oneInputStep.maxMass);
                                    oneOutputStep.Depth = depth;
                                    outputSteps.Add(oneOutputStep);
                                }
                                break;

                                case "downrange":
                                {
                                    CustomChallengesMod.CustomSteps.Step_Downrange oneOutputStep = new CustomChallengesMod.CustomSteps.Step_Downrange();
                                    oneOutputStep.planet=onePlanet;
                                    oneOutputStep.downrange=(int)GetDistance(onePlanet,systemName,stepID,"downrange",oneInputStep.downrange);
                                    outputSteps.Add(oneOutputStep);
                                }
                                break;

                                case "height":
                                {
                                    CustomChallengesMod.CustomSteps.Step_HeightExt oneOutputStep =
                                        new CustomChallengesMod.CustomSteps.Step_HeightExt();
                                    oneOutputStep.planet=onePlanet;
                                    oneOutputStep.hasEngines=oneInputStep.hasEngines;
                                    oneOutputStep.minHeight=GetDistance(onePlanet,systemName,stepID,"minHeight",oneInputStep.minHeight);
                                    oneOutputStep.minMass=GetMass(onePlanet,systemName,stepID,"minMass",oneInputStep.minMass);
                                    oneOutputStep.maxHeight=GetDistance(onePlanet,systemName,stepID,"maxHeight",oneInputStep.maxHeight);
                                    oneOutputStep.maxMass=GetMass(onePlanet,systemName,stepID,"maxMass",oneInputStep.maxMass);
                                    outputSteps.Add(oneOutputStep);
                                }
                                break;

                                case "impact":
                                {
                                    CustomChallengesMod.CustomSteps.Step_Impact oneOutputStep = new CustomChallengesMod.CustomSteps.Step_Impact();
                                    oneOutputStep.planet=onePlanet;
                                    oneOutputStep.impactVelocity=GetVelocity(onePlanet,systemName,stepID,"impactVelocity",oneInputStep.impactVelocity);
                                    outputSteps.Add(oneOutputStep);
                                }
                                break;

                                case "land":
                                {
                                    CustomChallengesMod.CustomSteps.Step_LandExt oneOutputStep =
                                        new  CustomChallengesMod.CustomSteps.Step_LandExt();
                                    oneOutputStep.planet=onePlanet;
                                    oneOutputStep.hasEngines=oneInputStep.hasEngines;
                                    oneOutputStep.minMass=GetMass(onePlanet,systemName,stepID,"minMass",oneInputStep.minMass);
                                    oneOutputStep.maxMass=GetMass(onePlanet,systemName,stepID,"maxMass",oneInputStep.maxMass);
                                    outputSteps.Add(oneOutputStep);
                                }
                                break;

                                case "customorbit":
                                {
                                    CustomChallengesMod.CustomSteps.Step_CustomOrbit oneOutputStep =
                                        new  CustomChallengesMod.CustomSteps.Step_CustomOrbit();
                                    oneOutputStep.planet=onePlanet;
                                    oneOutputStep.hasEngines=oneInputStep.hasEngines;

                                    oneOutputStep.maxApoapsis=GetDistance(onePlanet,systemName,stepID,"maxApoapsis",oneInputStep.maxApoapsis)+onePlanet.Radius;
                                    oneOutputStep.maxEcc=oneInputStep.maxEcc;
                                    oneOutputStep.maxMass=GetMass(onePlanet,systemName,stepID,"maxMass",oneInputStep.maxMass);
                                    oneOutputStep.maxPeriapsis=GetDistance(onePlanet,systemName,stepID,"maxPeriapsis",oneInputStep.maxPeriapsis)+onePlanet.Radius;
                                    oneOutputStep.maxSma=GetDistance(onePlanet,systemName,stepID,"maxSma",oneInputStep.maxSma)+onePlanet.Radius;

                                    oneOutputStep.minApoapsis=GetDistance(onePlanet,systemName,stepID,"minApoapsis",oneInputStep.minApoapsis)+onePlanet.Radius;
                                    oneOutputStep.minEcc=oneInputStep.minEcc;
                                    oneOutputStep.minMass=GetMass(onePlanet,systemName,stepID,"minMass",oneInputStep.minMass);
                                    oneOutputStep.minPeriapsis=GetDistance(onePlanet,systemName,stepID,"minPeriapsis",oneInputStep.minPeriapsis)+onePlanet.Radius;
                                    oneOutputStep.minSma=GetDistance(onePlanet,systemName,stepID,"minSma",oneInputStep.minSma)+onePlanet.Radius;

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
                                    oneOutputStep.planet=onePlanet;
                                    oneOutputStep.orbit=outputOrbit;
                                    oneOutputStep.hasEngines=oneInputStep.hasEngines;
                                    oneOutputStep.minMass=GetMass(onePlanet,systemName,stepID,"minMass",oneInputStep.minMass);
                                    oneOutputStep.maxMass=GetMass(onePlanet,systemName,stepID,"maxMass",oneInputStep.maxMass);
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
                    break;
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

        static bool MatchesFilter(SFS.WorldBase.Planet planet,CustomChallengesMod.CustomChallengesData.FilterInfo filter)
        {
            if (filter==null) return true;
            if (filter.isSignificant!=null && filter.isSignificant!= planet.data.basics.significant) return false;
            if (filter.hasLandmarks!=null && filter.hasLandmarks!=(planet.data.landmarks!=null && planet.data.landmarks.Count>1)) return false;
            if (filter.hasSatellites!=null && filter.hasSatellites!=(planet.satellites!=null && planet.satellites.Length>1)) return false;
            if (filter.hasTerrain!=null && filter.hasTerrain!=planet.data.hasTerrain) return false;
            if (filter.logsLanded!=null && filter.logsLanded!=planet.data.logs.Landed) return false;
            if (filter.logsTakeoff!=null && filter.logsTakeoff!=planet.data.logs.Takeoff) return false;
            if (filter.logsAtmosphere!=null && filter.logsAtmosphere!=planet.data.logs.Atmosphere) return false;
            if (filter.logsOrbit!=null && filter.logsOrbit!=planet.data.logs.Orbit) return false;
            if (filter.logsCrash!=null && filter.logsCrash!=planet.data.logs.Crash) return false;
            if (filter.exclude!=null && filter.exclude.Contains(planet.codeName)) return false;
            if (filter.primaries!=null)
            {
                if (planet.parentBody==null) return false;

                foreach (string onePrimary in filter.primaries)
                    if (onePrimary.EndsWith("*"))
                    {
                        string primaryName =  onePrimary.Substring(0,onePrimary.Length-1);
                        for (SFS.WorldBase.Planet primary=planet.parentBody; primary!=null;  primary=primary.parentBody)
                        {
                            if (primary.codeName==primaryName) return true;
                        }
                    }
                    else
                    {
                        if (planet.parentBody.codeName==onePrimary) return true;
                    }

                return false;
            }
            return true;
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
                bool debug =CustomChallengesMod.SettingsManager.settings.debug;

                if (UnityEngine.Application.isEditor)
                {
                    ResourcesLoader.main = UnityEngine.Object.FindObjectOfType<ResourcesLoader>();
                }
                ResourcesLoader.ChallengeIcons challengeIcons = ResourcesLoader.main.challengeIcons;
                if (debug) UnityEngine.Debug.Log("[CustomChallengesMod.CollectChallenges.Postfix-I-01] Loading Custom_Challenges.txt");

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

                // populate dictionary for existing challenges, replacing the steps with the equivalent custom steps
                foreach (SFS.Logs.Challenge oneChallenge in  __result)
                {
                    if (oneChallenge.steps.Count>0)
                    {
                        for (int stepIndex=0;stepIndex<oneChallenge.steps.Count; stepIndex++)
                        {
                            if (oneChallenge.steps[stepIndex] is SFS.Logs.MultiStep oneMultiStep)
                            {
                                oneChallenge.steps[stepIndex] = CustomChallengesMod.CustomSteps.Step_AllOf.Create(oneMultiStep);
                            }
                            else if (oneChallenge.steps[stepIndex] is SFS.Logs.Step_Any_Landmarks oneStep_Any_Landmarks)
                            {
                                oneChallenge.steps[stepIndex] = CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt.Create(oneStep_Any_Landmarks);
                            }
                            else if (oneChallenge.steps[stepIndex] is SFS.Logs.Step_Downrange oneStep_Downrange)
                            {
                                oneChallenge.steps[stepIndex] = CustomChallengesMod.CustomSteps.Step_Downrange.Create(oneStep_Downrange);
                            }
                            else if (oneChallenge.steps[stepIndex] is SFS.Logs.Step_Height oneStep_Height)
                            {
                                oneChallenge.steps[stepIndex] = CustomChallengesMod.CustomSteps.Step_HeightExt.Create(oneStep_Height);
                            }
                            else if (oneChallenge.steps[stepIndex] is SFS.Logs.Step_Impact oneStep_Impact)
                            {
                                oneChallenge.steps[stepIndex] = CustomChallengesMod.CustomSteps.Step_Impact.Create(oneStep_Impact);
                            }
                            else if (oneChallenge.steps[stepIndex] is SFS.Logs.Step_Land oneStep_Land)
                            {
                                oneChallenge.steps[stepIndex] = CustomChallengesMod.CustomSteps.Step_LandExt.Create(oneStep_Land);
                            }
                            else if (oneChallenge.steps[stepIndex] is SFS.Logs.Step_Orbit oneStep_Orbit)
                            {
                                oneChallenge.steps[stepIndex] = CustomChallengesMod.CustomSteps.Step_OrbitExt.Create(oneStep_Orbit);
                            }
                        }
                    }

                    if (oneChallenge.returnSafely && oneChallenge.steps.Count==1)
                    {
                        CustomChallengesMod.CustomSteps.Step_LandExt landingStep=new CustomChallengesMod.CustomSteps.Step_LandExt();
                        landingStep.planet = SFS.Base.planetLoader.spaceCenter.Planet;
                        oneChallenge.steps.Add(landingStep);
                    }
                    challengesById[oneChallenge.id]=oneChallenge;
                }

                traceID="T-03";

                if (custom_Challenges!=null)
                {
                    int itemNo=0;
                    int sequenceNo=challengesById.Count;
                    System.Collections.Generic.List<_ChallengeIntermediate> outputChallenges= new System.Collections.Generic.List<_ChallengeIntermediate>();

                    if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-I-02] Custom_Challenges.txt defines {0:N0} challenges", custom_Challenges.Count);

                    // generate a list of challenges from the custom challenges supplied, ignoring invalid items
                    foreach (CustomChallengesMod.CustomChallengesData oneInputChallenge in custom_Challenges)
                    {
                        itemNo++;
//~                         if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-D-01] item:{0:D}", itemNo);

                        if (oneInputChallenge!=null)
                        {
                            try
                            {
                                bool canAdd=false;

//~                                 if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-D-01a] id:\"{0}\"", oneInputChallenge.id);

                                if (string.IsNullOrWhiteSpace(oneInputChallenge.id))
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

                                string inputIcon= oneInputChallenge.icon.Trim();
                                UnityEngine.Sprite outputIcon = null;

                                switch (inputIcon.ToLower())
                                {
                                    case "":outputIcon=null;break;
                                    case "firstflight":outputIcon=challengeIcons.firstFlight;break;
                                    case "10km":outputIcon=challengeIcons.icon_10Km;break;
                                    case "30km":outputIcon=challengeIcons.icon_30Km;break;
                                    case "50km":outputIcon=challengeIcons.icon_50Km;break;
                                    case "downrange":outputIcon=challengeIcons.icon_Downrange;break;
                                    case "reach_orbit":outputIcon=challengeIcons.icon_Reach_Orbit;break;
                                    case "orbit_high":outputIcon=challengeIcons.icon_Orbit_High;break;
                                    case "capture":outputIcon=challengeIcons.icon_Capture;break;
                                    case "tour":outputIcon=challengeIcons.icon_Tour;break;
                                    case "crash":outputIcon=challengeIcons.icon_Crash;break;
                                    case "land_one_way":outputIcon=challengeIcons.icon_UnmannedLanding;break;
                                    case "land_return":outputIcon=challengeIcons.icon_MannedLanding;break;
                                    default:
                                    {
                                        if (inputIcon.ToLower().EndsWith(".png"))
                                        {
                                            outputIcon=LoadNewSprite
                                                (
                                                    FileLocations.SolarSystemsFolder.Extend(solarSystem.name)
                                                        .ExtendToFile("Custom_Challenge_Icons/" + inputIcon)
                                                );

                                            if (outputIcon==null)
                                            {
                                                UnityEngine.Debug.LogFormat
                                                    (
                                                        "Solar system \"{0}\" Custom_Challenges.txt file id:{1} cannot find icon file: \"{2}\""
                                                        ,solarSystem.name
                                                        ,oneInputChallenge.id
                                                        ,"Custom_Challenge_Icons/" + inputIcon
                                                    );
                                            }

                                        }
                                        else
                                        {
                                            throw new _InternalException
                                                (
                                                    string.Format
                                                        (
                                                            "Solar system \"{0}\" Custom_Challenges.txt file id:{1} has an invalid icon field: \"{2}\""
                                                            ,solarSystem.name
                                                            ,oneInputChallenge.id
                                                            ,inputIcon
                                                        )
                                                );
                                        }
                                    }
                                    break;
                                }

//~                                 if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-D-02] item:{0:D} outputIcon:\"{1}\"",itemNo,outputIcon);

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

//~                                 if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-D-03] item:{0:D} canAdd:\"{1}\"",itemNo,canAdd);

                                if (canAdd)
                                {
                                    if (oneInputChallenge.id.Contains("{planet}"))
                                    {
                                        foreach (SFS.WorldBase.Planet onePlanet in SFS.Base.planetLoader.planets.Values)
                                        {
                                            if (MatchesFilter(onePlanet,oneInputChallenge.filter))
                                            {
                                                _ChallengeIntermediate oneOutputChallenge;

                                                if (outputIcon==null)
                                                {
                                                    oneOutputChallenge=new _ChallengeIntermediate();
                                                    oneOutputChallenge.id=oneInputChallenge.id.Trim().Replace("{planet}", onePlanet.codeName);
                                                }
                                                else
                                                {
                                                    oneOutputChallenge=GetChallenge(solarSystem.name, oneInputChallenge, sequenceNo++,onePlanet);
                                                }
                                                oneOutputChallenge.icon=outputIcon;
                                                outputChallenges.Add(oneOutputChallenge);
//~                                                 if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-D-04] item:{0:D} added id:\"{1}\"",itemNo,oneOutputChallenge.id);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _ChallengeIntermediate oneOutputChallenge;
                                        if (outputIcon==null)
                                        {
                                            oneOutputChallenge=new _ChallengeIntermediate();
                                            oneOutputChallenge.id=oneInputChallenge.id;
                                        }
                                        else
                                        {
                                            oneOutputChallenge=GetChallenge(solarSystem.name, oneInputChallenge, sequenceNo++);
                                        }
                                        oneOutputChallenge.icon=outputIcon;
                                        outputChallenges.Add(oneOutputChallenge);
//~                                         if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-D-05] item:{0:D} added id:\"{1}\"",itemNo,oneOutputChallenge.id);
                                   }
                                }
                            }
                            catch (_InternalException excp)
                            {
                                UnityEngine.Debug.LogErrorFormat("[CustomChallengesMod.CollectChallenges.Postfix] {0}",excp.Message);
                            }
                        }
                    }

                    if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-I-03] {0:N0} challenge definitions generated", outputChallenges.Count);
                    if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-I-04] {0:N0} challenges before merging", challengesById.Count);

                    // merge the challenges with the vanilla challenge list
                    foreach (_ChallengeIntermediate oneOutputChallenge in outputChallenges)
                    {
//~                         if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-I-05] id={0} icon={1}", oneOutputChallenge.id,oneOutputChallenge.icon);

                        if (oneOutputChallenge.icon!=null)
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
                                    ,false //returnSafely
                                    ,oneOutputChallenge.steps
                                );

                            // by setting returnSafely=false in the constructor and setting it here instead prevent to automatic land on earth step from being generated
                            challengesById[oneOutputChallenge.id].returnSafely = oneOutputChallenge.returnSafely;
//~                             if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-I-06] updating id={0}", oneOutputChallenge.id);
                        }
                        else if (challengesById.ContainsKey(oneOutputChallenge.id))
                        {
//~                             if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-I-07] removing id={0}", oneOutputChallenge.id);
                            challengesById.Remove(oneOutputChallenge.id);
                        }
                    }

                    if (debug) UnityEngine.Debug.LogFormat("[CustomChallengesMod.CollectChallenges.Postfix-I-08] {0:N0} challenges after merging", challengesById.Count);
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