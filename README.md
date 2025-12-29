# Custom-Challenges-SFS1

The mod adds support for custom challenges to custom solar systems. In addition it:

* displays 'in progress' challenges with step information in a closable window in the world scene.
* for vanilla challenges 'return safely' adds a step for 'land on home world' (world with initial launch pad) even when the homeworld is not 'Earth'.
* for custom challenges 'return safely' will not automatically add a 'land on home world' step, you are free to add a step for a different world to land on (and recover) or not add a land step (in which case you can recover on any world). N.B. in order to be able recover on a world other than the homeword, you will need the [Multi Launchpad mod](https://github.com/Darthan184/Multi-Launchpad-SFS1/releases) and select the launchpad to the world to recover on before landing.

This mod (should) fix the bugs in the challenge system in SFS - the Moon Tour and Mars Tour challenges should now work correctly (i.e. even with docking and loading from a quicksave). This will apply to the vanilla challlenges even if a "Custom_Challenges.txt" file is not provided. I've tested this, but the error cases were a bit difficult to identify, other bugs may still lurk.

To specify custom challenges you need to add a Custom\_Challenges.txt file and possibly a Custom\_Challenge\_Icons directory if you want to add your own challenge icons:

![File Directory](images/directory.png)

The file is a JSON array of challenge definitions each of which also contains a JSON array of steps of various types to be completed in the specified order. There are also two special step type - "Multi" - that contains an array of steps that may be completed in any order and "Any" that contains an array of steps any or which may be completed.

An example of a Custom\_Challenges.txt. Note Custom\_Challenges.txt in the ![CC\_SampleWorld  file](CC_SampleWorld.zip) gives more examples:

```
[
    {
        "id":"NearEarth_Tour"
        ,"icon": "flyby.png"
        ,"priority":0
        ,"title":"Moon and asteroid flyby"
        ,"description":"Flyby both the Moon and the captured asteroid"
        ,"ownerName":"Earth"
        ,"challengeDifficulty": "Hard"
        ,"returnSafely": false
        ,"steps":
            [
                {
                    "stepType":"Multi"
                    ,"steps":
                        [
                            {
                                "planetName":"Moon"
                                ,"stepType":"Orbit"
                                ,"orbitType":"Esc"
                            }
                            ,{
                                "planetName":"Near_Earth_Asteroid"
                                ,"stepType":"Orbit"
                                ,"orbitType":"Esc"
                            }
                        ]
                }
            ]
    }
    ,{
        "id":"Moon_CloseFlyby"
        ,"icon": "flyby.png"
        ,"priority":1
        ,"title":"Moon flyby within [[2A]]"
        ,"description":"Flyby the moon within [[2A]]"
        ,"ownerName":"Moon"
        ,"challengeDifficulty": "Medium"
        ,"returnSafely": false
        ,"steps":
            [
                {
                    "planetName":"Moon"
                    ,"stepType":"Height"
                    ,"maxHeight": "2A"
                }
            ]
    }
    ,{
        "id":"Moon_Land_Heavy"
        ,"icon": "Land_One_Way"
        ,"priority":-1
        ,"title":"Heavy Payload To The Moon"
        ,"description":"Land at least 500 tonnes on the Moon"
        ,"ownerName":"Moon"
        ,"difficulty":"Normal"
        ,"challengeDifficulty": "Hard"
        ,"returnSafely": false
        ,"steps":
            [
                {
                    "planetName":"Moon"
                    ,"stepType":"Land"
                    ,"minMass": 500
                }
            ]
    }
```

Details for each field:

__Challenge__

"id" : {string value} (required)
* The identifier for this challenge. N.B. is case-sensitive.
* If the same id as a vanilla challenge is supplied; this challenge will replace the vanilla challenge.
* If the same id as a vanilla challenge is supplied; this challenge will replace the vanilla challenge.
* If only the id field is supplied and it is same id as a vanilla challenge; the vanilla challenge will be removed (for this world only).
* If the id contains "{planet}", a challenge for every planet that matches the supplied filter will be created and have an ownerName of the planet name.
* vanilla challenge ids:
    * Liftoff\_0
    * Reach\_10km
    * Reach\_30km
    * Reach\_Downrange
    * Reach\_Orbit
    * Orbit\_High
    * Moon\_Orbit
    * Moon\_Tour
    * Asteroid\_Crash
    * Mars\_Tour
    * Venus\_One\_Way
    * Venus\_Landing
    * Mercury\_One\_Way
    * Mercury\_Landing
    * Land\_{planet name} Where {planet name} is the name of a planet for other land and return safely challenges. N.B. Only these challenges are automatically created for custom systems by SFS.

"icon" : {string value} (default "")
* indicates which of the icons should be used, omit or leave blank to indicate that an existing challenge should be deleted. If suffixed with '.png' will load a file from Custom_Challenge_Icons/ . Otherwise a standard SFS Icon will be used, one of:
*  "firstFlight", "10Km", "30Km", "50Km", "Downrange", "Reach\_Orbit", "Orbit\_High", "Capture", "Tour", "Crash","Land\_One\_Way", "Land\_Return"

"difficulty" : {string value} (default "all")
*  The difficulty mode that this challenge is enabled for: "all", "normal", "hard", "realistic"

"filter" : {object with filter information} (default no filter used)
* Fillter to be used if the id contains "{planet}". Ignored unless the id contains "{planet}". Details are below.

"priority" : {int value} (default 0)
* indicates how to sort this challenge, is a small signed integer, higher numbers appear at the top of the list.

"title" : {string value} (required if icon is supplied)
* The title to be used, N.B. not automatically translated (the in-game ones are translated). A sub-string like \[\[3A\]\] or \[\[0.5R:Moon\]\] , specifying planet-relative units will be replaced with a value in m, km, Mm, Gm, Tm or ly. The planet defaults to the one in ownerName. If the id contains "{planet}", a sub-string of "{planet}" will be replaced by the planet name and a sub-string of "{primary}" the name of the primary of the planet.


"description" : {string value} (required if icon is supplied)
* The description to be used, N.B. not automatically translated (the in-game ones are translated). If the id contains "{planet}", a sub-string of "{planet}" will be replaced by the planet name and a sub-string of "{primary}" the name of the primary of the planet.

"ownerName" : {string value} (required if icon is supplied and the id does not contain "{planet}")
* The name of the planet that is the 'owner' of this challenge. This specifies the planet the challenge appears under.

"challengeDifficulty" : {string value} (required if icon is supplied)
* The challenge difficulty indicator to be used (will be translated). Possible values: Easy, Medium, Hard, Extreme

"returnSafely" : {bool value} (default false)
* Indicates that return safely is needed. Indicates that the completed challenge should appear in the challenge log when the rocket is recovered and may indicate that the challenge should only be recorded once recovered. You will have to experiment with this. If false the challenge completed message appears as soon as the criteria at met, and the challanges completiong is recorded at the same time.

"steps" : {array of step values} (default null)
* The steps needed to complete this challenge in the order they need to be accomplished in. N.B. all the pre-defined challenges only specify one step - the 'Tour' challenges use special 'multi step' types that indicates they can be done in any order - these are all buggy. "returnSafely" is used to indicate a 'Land on Earth' step is expected at the end. I haven't spotted SFS forgetting the progress for multiple (sequential) steps, but the vanilla challenges have a maximum of two steps with the second being 'Land on Earth'.

---

__Filter__

(all filter criteria must match)

"isSignificant" : {bool value} (ignored if null)
* Only include if the planet 'is significant' flag has this value, if omitted, is not checked

"hasLandmarks" : {bool value} (ignored if null)
* Only include if the planet 'has any landmarks' has this value, if omitted, is not checked

"hasSatellites" : {bool value} (ignored if null)
* Only include if the planet 'has satellites' flag has this value, if omitted, is not checked

"hasTerrain" : {bool value} (ignored if null)
* Only include if the planet 'has terrain' flag has this value, if omitted, is not checked

"logsLanded" : {bool value} (ignored if null)
* Only include if the planet 'logs landed' flag has this value, if omitted, is not checked

"logsTakeoff" : {bool value} (ignored if null)
* Only include if the planet 'logs takeoff' flag has this value, if omitted, is not checked

"logsAtmosphere" : {bool value} (ignored if null)
* Only include if the planet 'logs atmosphere' flag has this value, if omitted, is not checked

"logsOrbit" : {bool value} (ignored if null)
* Only include if the planet 'logs orbit' flag has this value, if omitted, is not checked

"logsCrash" : {bool value} (ignored if null)
* Only include if the planet 'logs crash' flag has this value, if omitted, is not checked

"exclude" : {string array value} (ignored if null)
* Exclude planets with these names, if omitted, no planets are excluded by name

"primaries" : {string array value} (ignored if null)
* Only include planets with a primary having these names, if omitted, primaries are not checked. Suffix a name with * to indicate any primary in the chain of primaries. e.g "Proxima Centauri*" indicates any body in the "Proxima Centauri" system

---

__Step__

"planetName" : {string value} (required)
* The name of the planet where this step is to be accomplished, not used for stepType="Multi" or "Any"
* the following special values may be used
    * "{sat}" generate a step for every satellite of the challenge owner
    * "{planet}" the challenge owner
    * "{primary}" the primary of the challenge owner

"filter" : {object with filter information} (default no filter used)
* Filter to be used if the planetName is "{sat}". Ignored unless the planetName is "{sat}", not used for stepType="Multi" or "Any"

"stepType" : {string value} (required)
* The type of step. Possible values:
* "Multi" - all of the specified steps in any order
    * does not support the "Impact" step type as a sub-step.
    
* "Any" - any one of the specified steps
    * does not support the "Impact" step type as a sub-step.

* "Any_Landmarks" - a number of landmarks in any order
* "Downrange" - used if landed a minimum distance from the current(?) launch pad . Effect on planets without a current launchpad is unclear.
* "Height" - used for altitude reached.
* "Impact" - impact at a minimum velocity, unclear how this works
* "Land" - land on this planet
* "Orbit" - orbit this planet using SFS orbit classifications
* "CustomOrbit" - orbit this planet with the specified orbital parameters. N.B. this does not appear to work for the Sun for some reason - will need expermentation.

"steps" : {array of step values} (default null)
* Used for stepType="Multi" or "Any", the steps thar apply to this challenge
* "Multi":the list of steps that all need to be accomplished (in any order)
* "Any":the list of steps, one of which must be accomplished

"count" : {int value} (default 0)
* Only used for stepType="Any_Landmarks", the minimum number of landmarks that need to be landed on. If there are fewer landmarks on the planet indicates all landmarks.

"downrange" : {string value with units} (default "")
* Only used for stepType="Downrange", the mimimum distance from the launch site.

"hasEngines" : {bool value} (default null)
* Used for stepType="Height","Land","Orbit","CustomOrbit", "Any_Landmarks". Test for the presence of engines or boosters.
    * If true, rocket must have engines or boosters. (Probably not useful)
    * If false, rocket must not have engines or boosters. This can be used to test for a released payload. N.B. the payload needs to be the current rocket to detect this.
    * If omitted or null is not checked.

"impactVelocity" : {int value} (default 0)
* Only used for stepType="Impact". The mimimum velocity at impact in m/s.

"minHeight" : {string value with units} (default "")
* Only used for stepType="Height". The minimum altitude to be reached  (use a low value, e.g. 100, to detect a launch)

"maxHeight" : {string value with units} (default "")
* Only used for stepType="Height". The maximum altitude to be reached  (useful for flybys)

"minMass" : {double value} (default double.NaN)
* Used for stepType="Height","Land","Orbit","CustomOrbit", "Any_Landmarks". The minimum rocket mass in tonnes.
    * If already in orbit (or landed) docking additional rockets can meet the challenge.
    * For "Any_Landmarks" the rocket mass is checked for ***all*** landings

"maxMass" : {double value} (default double.NaN)
* Used for stepType="Height","Land","Orbit","CustomOrbit", "Any_Landmarks". The maximum rocket mass in tonnes.
    * With "Height" and a low value can be used to specify a maximum launch mass.
    * For "Any_Landmarks" the rocket mass is checked for ***all*** landings

"orbitType" : {string value} (required if stepType="Orbit")
* Only used for stepType="Orbit". The type of orbit the needs to be reached. Note, each condition is checked in the following order and the first matching one is counted. Possible values:
*  "None" - landed - not sure if this is usefull
*  "Esc" - apoapsis>SOI - could also be used to detect a flyby
*  "Sub" - suborbital, periapsis below surface
*  "High" - periapsis > 1.5 radius above surface
*  "Trans"- apoapsis > 0.5 radius above surface
*  "Low"  - anything else

"maxApoapsis" : {string value with units} (default "")
* Only used for stepType="CustomOrbit".The maximum apoapsis.

"maxEcc" : {double value} (default double.NaN)
* Only used for stepType="CustomOrbit".The maximum eccentricity.

"maxPeriapsis" : {string value with units} (default "")
* Only used for stepType="CustomOrbit".The maximum periapsis.

"maxSma" : {string value with units} (default "")
* Only used for stepType="CustomOrbit".The maximum semi-major axis.

"minApoapsis" : {string value with units} (default "")
* Only used for stepType="CustomOrbit".The minimum apoapsis.

"minEcc" : {double value} (default double.NaN - indicates ignore this)
* Only used for stepType="CustomOrbit".The minimum eccentricity. Does not seem to be able to detect eccentricities >=1 .

"minPeriapsis" : {string value with units} (default "")
* Only used for stepType="CustomOrbit".The minimum periapsis.

"minSma" : {string value with units} (default "")
* Only used for stepType="CustomOrbit".The minimum semi-major axis.

{string value with units} (used for downrange/height/periapsis/apoapsis/sma values), if the last character is a digit the value is the altitude/distance in meters, otherwise it should be one of:
* "k" - for km altitude or distance
* "m" - for Mm altitude or distance
* "g" - for Gm altitude or distance
* "t" - for Tm altitude or distance
* "l" - for light-years altitude or distance

planet-relative units:
* "a" - for atmosphere height altitude or distance (if the planet has no atmosphere will use a maximum of max terrain height and timewarp height)
* "r" - for planetary radius altitude or distance
* "s" - SOI multiple distance from center (values should be <1) - not useful for distances?

## Settings

The settings.txt file in the mod directory (C:\Program Files (x86)\Steam\steamapps\common\Spaceflight Simulator\Spaceflight Simulator Game\Mods\CustomChallengesMod on steam) looks like this:

```
{
  "debug": false,
  "windowPosition": {
    "x": 915,
    "y": 915
  }
}
```

This file should only be amended when SFS is not running.

if "debug" is true: all challenges and steps can be listed in the hub screen and the world scene and can be filtered by Complete or
Incomplete challenges. Additionally on the World scene they can be filtered by in progress.

if "debug" is false: only in progress challenges and steps will be shown on the World scene.

