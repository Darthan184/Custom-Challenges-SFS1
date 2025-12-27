namespace CustomChallengesMod
{
    /// <summary>Data for one custom challenge stored in a JSON array in
    /// C:\Program Files (x86)\Steam\steamapps\common\Spaceflight Simulator\Spaceflight Simulator Game\Spaceflight Simulator_Data\Custom Solar Systems\[[world name]]\Custom_Challenges.txt
    /// for downrange/height/periapsis/apoapsis/sma values, if the last character is a digit it is altitude in meters, otherwise it should be one of:
    /// "k" - for km altitude or distance
    /// "m" - for Mm altitude or distance
    /// "g" - for Gm altitude or distance
    /// "t" - for Tm altitude or distance
    /// "l" - for light-years altitude or distance
    /// planet-relative units:
    /// "a" - for atmosphere height altitude or distance (if the planet has no atmosphere will use a maximum of max terrain height and timewarp height
    /// "r" - for planetary radius altitude or distance
    /// "s" - SOI multiple distance from center (values should be <1) - not useful for distance?
    /// </summary>
    [System.Serializable]
    public class CustomChallengesData
    {
        /// <summary>Filter information to be used when multiple planets can be selected by one custom challenge or step</summary>
        [System.Serializable]
        public class FilterInfo
        {
            /// <summary>Only include if the planet 'is significant' flag has this value, if omitted, is not checked</summary>
            public bool? isSignificant=null;

            /// <summary>Only include if the planet 'has satellites' flag has this value, if omitted, is not checked</summary>
            public bool? hasSatellites=null;

            /// <summary>Only include if the planet 'has terrain' flag has this value, if omitted, is not checked</summary>
            public bool? hasTerrain=null;

            /// <summary>Only include if the planet 'logs landed' flag has this value, if omitted, is not checked</summary>
            public bool? logsLanded=null;

            /// <summary>Only include if the planet 'logs takeoff' flag has this value, if omitted, is not checked</summary>
            public bool? logsTakeoff=null;

            /// <summary>Only include if the planet 'logs atmosphere' flag has this value, if omitted, is not checked</summary>
            public bool? logsAtmosphere=null;

            /// <summary>Only include if the planet 'logs orbit' flag has this value, if omitted, is not checked</summary>
            public bool? logsOrbit=null;

            /// <summary>Only include if the planet 'logs crash' flag has this value, if omitted, is not checked</summary>
            public bool? logsCrash=null;

            /// <summary>Exclude planets with these names, if omitted, no planets are excluded by name</summary>
            public string[] exclude=null;

            /// <summary>
            /// Only include planets with a primary having these names, if omitted, primaries are not checked
            /// Suffix a name with * to indicate any primary in the chain of primaries. e.g "Proxima Centauri*" indicates any body
            /// in the "Proxima Centauri" system
            /// </summary>
            public string[] primaries=null;
        }

        /// <summary>Information for one step</summary>
        [System.Serializable]
        public class Step
        {
            /// <summary>The name of the planet where this step is to be accomplished, not used for stepType="Multi" or "Any"</summary>
            public string planetName="";

            /// <summary>
            /// filter to be used if the planetName is "{sat}".
            ///</summary>
            public CustomChallengesMod.CustomChallengesData.FilterInfo filter = null;

            /// <summary>
            /// The type of step. Possible values:
            /// "Multi" - multiple steps in any order (buggy - SFS seems to forget the progess in some cases)
            /// "Any" - any one of the specified steps
            /// "Any_Landmarks" - a numbe of landmarks in ay order (buggy - SFS seems to forget the progess in some cases)
            /// "Downrange" - used if landed a minimum distance from the current(?) launch pad . Effect on planets without a current launchpad is unclear.
            /// "Height" - used for altitude reached.
            /// "Impact" - impact at a minumum velocity, unclear how this works
            /// "Land" - land on this planet
            /// "Orbit" - orbit this planet using SFS orbit classifications
            /// "CustomOrbit" - orbit this planet with the specified orbital parameters
            /// </summary>
            public string stepType="";

            /// <summary>
            /// Used for stepType="Multi" or "Any", the steps tha apply to this challenge
            /// "Multi":the list of steps that all need to be accomplished (in any order)
            /// "Any":the list of steps, one of which must be accomplished
            /// </summary>
            public Step[] steps = null;

            /// <summary>
            /// only used for stepType="Any_Landmarks", the minimum number of landmarks that need to be landed on.
            /// </summary>
            public int count=0;

            /// <summary>
            /// Only used for stepType="Downrange", the mimimum distance from the launch site
            /// </summary>
            public string downrange="";

            /// <summary>
            /// Used for stepType="Height","Land","Orbit","CustomOrbit","Any_Landmarks".
            /// If true rocket must have engines or boosters.
            /// If false rocket must not have engines or boosters.
            /// If omitted or null is not checked.
            /// </summary>
            public bool? hasEngines=null;

            /// <summary>
            /// Only used for stepType="Height". The minimum altitude to be reached (useful for launches)
            /// </summary>
            public string minHeight="";

            /// <summary>
            /// Only used for stepType="Height". The maximum altitude to be reached (useful for flybys)
            /// </summary>
            public string maxHeight="";

            /// <summary>
            /// Only used for stepType="Impact". The mimimum velocity at impact in m/s.
            /// </summary>
            public int impactVelocity=0;

            /// <summary>
            /// Used for stepType="Height","Land","Orbit","CustomOrbit","Any_Landmarks". The minimum rocket mass in tonnes. N.B. If already in orbit
            /// (or landed) docking additional rockets can meet the challenge.
            /// </summary>
            public double minMass=double.NaN;

            /// <summary>
            /// Used for stepType="Height","Land","Orbit","CustomOrbit". The maximum rocket mass in tonnes. With "Height" and a low
            /// value can be used to specify a maximum launch mass.
            /// </summary>
            public double maxMass=double.NaN;

            /// <summary>
            /// Only used for stepType="Orbit". The type of orbit the needs to be reached. Possible values:
            /// not each condition is checked in the following order the first matching one is counted
            /// "None" - landed - no sure if this is usfull
            /// "Esc" - apoapsis>SOI - could be used to detect a flyby?
            /// "Sub", - suborbital, periapsis below surface
            /// "High", - periapsis > 1.5 radius above surface
            /// "Trans", - apoapsis > 0.5 radius above surface
            /// "Low",  - anything else
            /// </summary>
            public string orbitType="";

            /// <summary>Only used for stepType="CustomOrbit".The maximum apoapsis</summary>
            public string maxApoapsis="";

            /// <summary>Only used for stepType="CustomOrbit".The maximum eccentricity</summary>
            public double maxEcc=double.NaN;

            /// <summary>Only used for stepType="CustomOrbit".The maximum periapsis</summary>
            public string maxPeriapsis="";

            /// <summary>Only used for stepType="CustomOrbit".The maximum semi-major axis</summary>
            public string maxSma="";

            /// <summary>Only used for stepType="CustomOrbit".The minimum apoapsis</summary>
            public string minApoapsis="";

            /// <summary>Only used for stepType="CustomOrbit".The minimum eccentricity</summary>
            public double minEcc=double.NaN;

            /// <summary>Only used for stepType="CustomOrbit".The minimum periapsis</summary>
            public string minPeriapsis="";

            /// <summary>Only used for stepType="CustomOrbit".The minimum semi-major axis</summary>
            public string minSma="";

        }

        /// <summary>
        /// The identifier for this challenge. Land and return challenges should start with "Land_" for consistency with standard
        /// challenges. N.B. is case-sensitive.
        ///</summary>
        public string id = "";

        /// <summary>
        /// filter to be used if the id contains "{planet}".
        ///</summary>
        public CustomChallengesMod.CustomChallengesData.FilterInfo filter = null;

        /// <summary>
        /// indicates which of the icons should be used, omit or leave blank to indicate that an existing challenges should be
        /// deleted. If suffixed with '.png' will load a file from Custom_Challenge_Icons/ . Otherwise a standard SFS Icon will be
        /// used, one of:
        /// "firstFlight", "10Km", "30Km", "50Km", "Downrange", "Reach_Orbit", "Orbit_High", "Capture", "Tour", "Crash",
        /// "Land_One_Way", "Land_Return"
        ///</summary>
        public string icon = "";

        /// <summary>The difficulty mode that this challenge is enabled for: "all", "normal", "hard", "realistic"</summary>
        public string difficulty = "all";

        /// <summary>
        /// indicated how to sort this challenge, is a small signed integer, higher numbers appear at the top of the list.
        ///</summary>
        public int priority=0;

        /// <summary>
        /// The title to be used, N.B. not automatically translated (the in-game ones are translated). A sub-string like
        /// [[3A]] or [[0.5R:Moon]] , specifying planet-relative units will be replaced with a value in m, km, Mm, Gm, Tm or ly
        /// the planet defaults to the one in ownerName
        ///</summary>
        public string title="";

        /// <summary>
        /// The description to be used, N.B. not automatically translated (the in-game ones are translated).
        /// </summary>
        public string description="";

        /// <summary>
        /// The name of the planet that is the 'owner' of this challenge. This specifies the planet the challenge appears under
        /// </summary>
        public string ownerName="";

        /// <summary>
        /// The challenge difficulty indicator to be used (will be translated). Possible values:
        /// Easy, Medium, Hard, Extreme
        ///</summary>
        public string challengeDifficulty="";

        /// <summary>
        /// Indicates that return safely is needed. If the custom system includes a planet called 'Earth' will add a land on Earth step at the end.
        /// It is unclear if this has any effect it there is not a planet called Earth but the value is records in internal data structures.
        ///</summary>
        public bool returnSafely=false;

        /// <summary>
        /// The steps needed to complete this challenge in the order they need to be accomplished in. N.B. all the pre-defined
        /// challenges only specify one step - the 'Tour' challenges use special 'multi step' types that indicates they can be done
        /// in any order - these are all buggy. "returnSafely" is used to indicate a 'Land on Earth' step is expected at the end.
        /// I haven't spotted SFS forgetting the progress for multiple (sequential) steps, but the vanilla challenges have a maximum
        /// of two steps with the second being 'Land on Earth'
        ///</summary>
        public Step[] steps = null;
    }
}
