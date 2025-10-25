using System.Linq; // contains extensions
using HarmonyLib; // contains extensions

namespace CustomChallengesMod
{
    [HarmonyLib.HarmonyPatch(typeof(SFS.Logs.Challenge), "CollectChallenges")]
    class WorldBase_PlanetLoader
//~     {
//~         static void Prefix
//~             (
//~                 SFS.WorldBase.WorldSettings settings, SFS.I_MsgLogger log
//~                 , System.Action<bool> callback
//~                 , out System.Collections.Generic.List<MultiLaunchpadMod.SpaceCenterData> __state
//~             )
//~         {
//~         }

        private class _InternalException : System.Exception;

        static void Postfix(ref System.Collections.Generic.List<SFS.Logs.Challenge> __result)
        {
            SFS.WorldBase.SolarSystemReference solarSystem = settings.solarSystem;
            System.Collections.Generic.List<CustomChallengesMod.CustomChallengesData> custom_Challenges =
                new  System.Collections.Generic.List<CustomChallengesMod.CustomChallengesData>();

            System.Collections.Generic.Dictionary<string, SFS.Logs.Challenge> challengesById;
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
                    UnityEngine.Debug.Log
                        (
                            string.Format
                                ("CustomChallengesMod: Solar system \"{0}\" has an invalid Custom_Challenges.txt file", solarSystem.name)
                        );
                    return __result;
                }
            }

            // populate dictionary for existing challenges
            foreach (SFS.Logs.Challenge oneChallenge in  __result) challengesById[oneChallenge.id]=oneChallenge;

            // copy the list as read into a dictionary, ignoring unknown planets
            foreach (CustomChallengesMod.CustomChallengesData oneNewChallengeData in custom_Challenges)
            {
                #################### this wont work, there is no default constructor, need vars for each field and create the object from the vars
                SFS.Logs.Challenge oneNewChallenge = new SFS.Logs.Challenge();

                try
                {
                    bool hasData = true;

                    if (oneNewChallengeData.id.trim()="")
                    {
                        throw new _InternalException
                        (
                            string.Format
                                ("CustomChallengesMod: Solar system \"{0}\" Custom_Challenges.txt file has a missing id field", solarSystem.name)
                        );
                    }
                    oneNewChallenge.id=oneNewChallengeData.id.trim();

                    switch (oneNewChallengeData.icon.toLower().trim())
                    {
                        case "":hasData=false;break
                        case "firstflight":oneNewChallenge.icon=challengeIcons.firstFlight;break;
                        case "10km":oneNewChallenge.icon=challengeIcons.Icon_10Km;break;
                        case "30km":oneNewChallenge.icon=challengeIcons.Icon_30Km;break;
                        case "50km":oneNewChallenge.icon=challengeIcons.Icon_50Km;break;
                        case "downrange":oneNewChallenge.icon=challengeIcons.Icon_Downrange;break;
                        case "reach_orbit":oneNewChallenge.icon=challengeIcons.Icon_Reach_Orbit;break;
                        case "orbit_high":oneNewChallenge.icon=challengeIcons.Icon_Orbit_High;break;
                        case "capture":oneNewChallenge.icon=challengeIcons.Icon_Capture;break;
                        case "tour":oneNewChallenge.icon=challengeIcons.Icon_Tour;break;
                        case "crash":oneNewChallenge.icon=challengeIcons.Icon_Crash;break;
                        case "unmannedlanding":oneNewChallenge.icon=challengeIcons.Icon_UnmannedLanding;break;
                        case "mannedlanding":oneNewChallenge.icon=challengeIcons.Icon_MannedLanding;break;
                        default:
                            throw new _InternalException
                                (
                                    string.Format
                                        (
                                            "CustomChallengesMod: Solar system \"{0}\" Custom_Challenges.txt id:{1} file has an invalid icon field: \"{2}\""
                                            ,solarSystem.name
                                            ,oneNewChallenge.id
                                            ,oneNewChallengeData.icon
                                        )
                                );
                        break;
                    }

                    if (hasData)
                    {
                        oneNewChallenge.displayPriority=(100*oneNewChallengeData.priority - challengesById.Count);

                        if (oneNewChallengeData.title.trim()="")
                        {
                            throw new _InternalException
                                (
                                    string.Format
                                        (
                                            "CustomChallengesMod: Solar system \"{0}\" Custom_Challenges.txt id:{1} file has a missing title field"
                                            ,solarSystem.name
                                            ,oneNewChallenge.id
                                        )
                                );
                        }

                        string title=oneNewChallengeData.title;
                        oneNewChallenge.title=new System.Func<string>(() => title);

                        if (oneNewChallengeData.description.trim()="")
                        {
                            throw new _InternalException
                                (
                                    string.Format
                                        (
                                            "CustomChallengesMod: Solar system \"{0}\" Custom_Challenges.txt id:{1} file has a missing description field"
                                            ,solarSystem.name
                                            ,oneNewChallenge.id
                                        )
                                );
                        }

                        string description=oneNewChallengeData.description;
                        oneNewChallenge.description=new System.Func<string>(() => description);

                        switch (oneNewChallengeData.difficulty.toLower().trim())
                        {
                            case "easy":oneNewChallenge.difficulty=SFS.Logs.Difficulty.Easy;break;
                            case "medium":oneNewChallenge.difficulty=SFS.Logs.Difficulty.Medium;break;
                            case "hard":oneNewChallenge.difficulty=SFS.Logs.Difficulty.Hard;break;
                            case "extreme":oneNewChallenge.difficulty=SFS.Logs.Difficulty.Extreme;break;
                            default:
                                throw new _InternalException
                                    (
                                        string.Format
                                            (
                                                "CustomChallengesMod: Solar system \"{0}\" Custom_Challenges.txt id:{1} file has an invalid difficulty field: \"{2}\""
                                                ,solarSystem.name
                                                ,oneNewChallenge.id
                                                ,oneNewChallengeData.difficulty
                                            )
                                    );
                            break;
                        }

                        #################### load the steps, may need a recursive method to support MultiStep

                    }

                    if (hasData)
                    {
                        challengesById[oneNewChallenge.id]=oneNewChallenge;
                    }
                    else if (challengesById.ContainsKey(oneNewChallenge.id))
                    {
                        challengesById.Remove(oneNewChallenge.id);
                    }
                }
                catch (_InternalException excp)
                {
                    UnityEngine.Debug.Log(excp.Message);
                    return _result;
                }
                catch (System.Exception excp)
                {
                    UnityEngine.Debug.Log(excp.ToString());
                    return _result;
                }
            }
            __result.Clear();
            foreach (SFS.Logs.Challenge oneChallenge in  challengesById.Values) __result.Add(oneChallenge)
            __result.Sort((SFS.Logs.Challenge a, SFS.Logs.Challenge b) => b.displayPriority.CompareTo(a.displayPriority));
        }
    }
}