namespace CustomChallengesMod
{
    [System.Serializable]
    public class CustomChallengesData
    {
        [System.Serializable]
        public class Step
        {
            /// <summary>The name of the planet where this step is to be accomplished</summary>
            public string planetName="";

            /// <summary>
            /// The type of step. Possible values:
            /// "Multi" - multiple steps in any order (buggy)
            /// "Any_Landmarks" - a numbe of landmarks in ay order (buggy)
            /// "Downrange" - used if landed a minimum distance from the current(?) launch pad . Effect on planets without a current launchpad is unclear.
            /// "Height" - used for minimum altitude reached.
            /// "Impact" - impact at a minumum velocity, unclear how this works
            /// "Land" - land on this planet
            /// "Orbit" - orbit this planet
            /// </summary>
            public string stepType="";

            /// <summary>
            /// Only used for stepType="Multi", the list of steps that need to be accomplished in any order. In the pre-defined
            /// challenges these stem all have type land - unclue if any other setp type could be used and, in ay case, the
            /// implementation of this is buggy
            /// </summary>
            public Step[] steps = null;

            /// <summary>
            /// only used for stepType="Any_Landmarks", the minimum number of landmarks that need to be landed on
            /// </summary>
            public int count=0;

            /// <summary>
            /// Only used for stepType="Downrange" .the mimimum distance from the launch site in metres
            /// </summary>
            public int downrange=0;

            /// <summary>
            /// Only used for stepType="Height". The mimimum altitude to be reached in km.
            /// </summary>
            public int height=0;

            /// <summary>
            /// Only used for stepType "Height". If true, the velocity must ne at least 20m/s at this altitude. Is always false for the
            /// pre-defined challenges.
            /// </summary>
            public bool checkVelocity=false;

            /// <summary>
            /// Only used for stepType="Impact". The mimimum velocity at impact in m/s.
            /// </summary>
            public int impactVelocity=0;

            /// <summary>
            /// Only used for stepType="Orbit". The type of orbit the needs to be reached. Possible values:
            /// not each condition is checked in the following order the first matching one is counted
            /// "None" - landed - no sure if this is usfull
            /// "Esc" - apopapsis>SOI - could be used to detect a flyby?
            /// "Sub", - suborbital, periapsis below surface
            /// "High", - periapsis > 1.5 radius above surface
            /// "Trans", - apoapsis > 0.5 radius above surface
            /// "Low",  - anything else
            /// </summary>
            public string orbitType="";
        }

        /// <summary>
        /// The identifier for this challenge. Land and return challenges should start with "Land_" for consistency with standard
        /// challenges. N.B. is case-sensitive.
        ///</summary>
        public string id = "";

        /// <summary>
        /// indicates which of the icons should be used, omit or leave blank to indicate that an existing challenges should be
        /// deleted. Possible values:
        /// "firstFlight", "10Km", "30Km", "50Km", "Downrange", "Reach_Orbit", "Orbit_High", "Capture", "Tour", "Crash",
        /// "UnmannedLanding", "MannedLanding"
        ///</summary>
        public string icon = "";

        /// <summary>
        /// indicated how to sort this challenge, is a small signed integer, higher numbers appear at the top of the list
        ///</summary>
        public int priority=0;

        /// <summary>The title to be used, N.B. not automatically translated (the in-game ones are translated)
        public string title="";

        /// <summary>The description to be used, N.B. not automatically translated (the in-game ones are translated)
        public string description="";

        /// <summary>
        /// The difficulty indicator to be used (will be translated). Possible values:
        /// Easy, Medium, Hard, Extreme
        ///</summary>
        public string difficulty="";

        /// <summary>
        /// Indicates that return safely is needed. If the custom system includes a planet called 'Earth' will add a land on Earth step at the end.
        /// It is unclear if this has any effect it there is not a planet called Earth but the value is records in internal data structures.
        ///</summary>
        public bool returnSafely=false;

        /// <summary>
        /// The steps needed to complete this challenge in the order they need to be accomplished in. N.B. all the pre-defined
        /// challenges only specify one step - the 'Tour' challenges use special 'multi step' types that indicates they can be done
        /// in any order - these are all buggy. "returnSafely" is used to indicate a 'Land on Earth' step is expected at the end.
        ///</summary>
        public Step[] steps = null;
    }
}
