namespace CustomChallengesMod.CustomSteps
{
    public class Utility
    {
        public static bool CheckMass(double minMass,double maxMass)
        {
            if (minMass!=double.NaN || maxMass!=double.NaN)
            {
                if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                {
                    double mass = 0;
                    mass = rocket.mass.GetMass();
                    if (mass==0) return false;
                    if (minMass!=double.NaN && mass<minMass) return false;
                    if (maxMass!=double.NaN && mass>maxMass) return false;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>Data for one extended height challenge step</summary>
    public class Step_HeightExt : SFS.Logs.ChallengeStep
    {
        public double maxHeight=double.NaN;
        public double maxMass=double.NaN;
        public double minHeight=double.NaN;
        public double minMass=double.NaN;

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string _)
        {
            if (maxHeight!=double.NaN && location.Height > maxHeight) return false;
            if (minHeight!=double.NaN && location.Height < minHeight) return false;
            return Utility.CheckMass(minMass,maxMass);
        }
    }

    /// <summary>Data for a one of several challenge steps</summary>
    public class Step_OneOf : SFS.Logs.ChallengeStep
    {
        public System.Collections.Generic.List<SFS.Logs.ChallengeStep> steps;
        private string [] _delim = new string[]{"|a"};

        public int Depth
        { get { return (int)(_delim[0][1])-(int)'a'; } set { _delim[0]="|"+(char)((int)'a'+value);} }

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string progress)
        {
            bool stepCompleted=false;
            string[] stepProgress=new string[steps.Count];

            for (int stepIndex=0; stepIndex<stepProgress.Length; stepIndex++) stepProgress[stepIndex]="";

            if (!string.IsNullOrEmpty(progress))
            {
                string[] stepProgress_New=progress.Split(_delim, System.StringSplitOptions.None);
                for (int stepIndex=0; stepIndex<System.Math.Min(steps.Count,stepProgress_New.Length); stepIndex++)
                {
                    stepProgress[stepIndex]=stepProgress_New[stepIndex];
                }
            }

            for (int stepIndex=0;stepIndex<steps.Count;stepIndex++)
            {
                SFS.Logs.ChallengeStep oneStep = steps[stepIndex];
                if (oneStep.IsEligible(location.planet) && oneStep.IsCompleted(location,recorder,ref stepProgress[stepIndex]))
                {
                    stepCompleted=true;
                }
            }
            progress = string.Join(_delim[0],stepProgress);
            return stepCompleted;
        }

        public bool My_IsEligible(SFS.WorldBase.Planet currentPlanet)
        {
            foreach (SFS.Logs.ChallengeStep oneStep in steps)
            {
                if (oneStep.IsEligible(currentPlanet)) return true;
            }
            return false;
        }

        public override string OnConflict(string a, string b)
        {
            string[] progressA;
            string[] progressB;
            string[] stepProgress = new string[steps.Count];

            if (string.IsNullOrEmpty(a))
            {
                progressA=new string[steps.Count];
            }
            else
            {
                progressA=a.Split(_delim, System.StringSplitOptions.None);
            }

            if (string.IsNullOrEmpty(b))
            {
                progressB=new string[steps.Count];
            }
            else
            {
                progressB=b.Split(_delim, System.StringSplitOptions.None);
            }

            for (int stepIndex=0;stepIndex<steps.Count;stepIndex++)
            {
                if (stepIndex<progressA.Length && stepIndex<progressB.Length)
                {
                    stepProgress[stepIndex]=steps[stepIndex].OnConflict(progressA[stepIndex], progressB[stepIndex]);
                }
                else if (stepIndex<progressA.Length)
                {
                    stepProgress[stepIndex]=progressA[stepIndex];
                }
                else if (stepIndex<progressB.Length)
                {
                    stepProgress[stepIndex]=progressB[stepIndex];
                }
            }
            return string.Join(_delim[0],stepProgress);
        }
    }

    /// <summary>Data for a all of several challenge steps (replacement for multi)</summary>
    public class Step_AllOf : SFS.Logs.ChallengeStep
    {
        public System.Collections.Generic.List<SFS.Logs.ChallengeStep> steps;
        private string [] _delim = new string[]{"|b"};

        public int Depth
        { get { return (int)(_delim[0][1])-(int)'a'; } set { _delim[0]="|"+(char)((int)'a'+value);} }

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string progress)
        {
            string[] stepProgress=new string[steps.Count];
            bool stepCompleted=true;

            for (int stepIndex=0; stepIndex<stepProgress.Length; stepIndex++) stepProgress[stepIndex]="";

            if (!string.IsNullOrEmpty(progress))
            {
                string[] stepProgress_New=progress.Split(_delim, System.StringSplitOptions.None);
                for (int stepIndex=0; stepIndex<System.Math.Min(steps.Count,stepProgress_New.Length); stepIndex++)
                {
                    stepProgress[stepIndex]=stepProgress_New[stepIndex];
                }
            }

            for (int stepIndex=0;stepIndex<steps.Count;stepIndex++)
            {
                SFS.Logs.ChallengeStep oneStep = steps[stepIndex];
                if (stepProgress[stepIndex]=="**") continue;
                if (oneStep.IsEligible(location.planet) && oneStep.IsCompleted(location,recorder,ref stepProgress[stepIndex]))
                {
                    stepProgress[stepIndex]="**";
                }
                else
                {
                    stepCompleted=false;
                }
            }
            progress = string.Join(_delim[0],stepProgress);
            return stepCompleted;
        }

        public bool My_IsEligible(SFS.WorldBase.Planet currentPlanet)
        {
            foreach (SFS.Logs.ChallengeStep oneStep in steps)
            {
                if (oneStep.IsEligible(currentPlanet)) return true;
            }
            return false;
        }

        public override string OnConflict(string a, string b)
        {
            string[] progressA;
            string[] progressB;
            string[] stepProgress = new string[steps.Count];

            if (string.IsNullOrEmpty(a))
            {
                progressA=new string[steps.Count];
            }
            else
            {
                progressA=a.Split(_delim, System.StringSplitOptions.None);
            }

            if (string.IsNullOrEmpty(b))
            {
                progressB=new string[steps.Count];
            }
            else
            {
                progressB=b.Split(_delim, System.StringSplitOptions.None);
            }

            for (int stepIndex=0;stepIndex<steps.Count;stepIndex++)
            {
                if (stepIndex<progressA.Length && stepIndex<progressB.Length)
                {
                    stepProgress[stepIndex]=steps[stepIndex].OnConflict(progressA[stepIndex], progressB[stepIndex]);
                }
                else if (stepIndex<progressA.Length)
                {
                    stepProgress[stepIndex]=progressA[stepIndex];
                }
                else if (stepIndex<progressB.Length)
                {
                    stepProgress[stepIndex]=progressB[stepIndex];
                }
            }
            return string.Join(_delim[0],stepProgress);
        }
    }

    /// <summary>Data for one extended landing challenge step</summary>
    public class Step_LandExt : SFS.Logs.Step_Land
    {
        public double maxMass=double.NaN;
        public double minMass=double.NaN;

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string progress)
        {
            if (!base.IsCompleted(location,recorder,ref progress)) return false;
            return Utility.CheckMass(minMass,maxMass);
        }
    }

    /// <summary>Data for one extended any landmarks step</summary>
    public class Step_Any_LandmarksExt : SFS.Logs.Step_Any_Landmarks
    {
        private string [] _delim = new string[]{"|c"};
        public double maxMass=double.NaN;
        public double minMass=double.NaN;

        public int Depth
        { get { return (int)(_delim[0][1])-(int)'a'; } set { _delim[0]="|"+(char)((int)'a'+value);} }

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string progress)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(progress)) progress= string.Join(",",progress.Split(_delim, System.StringSplitOptions.None));
            result = base.IsCompleted(location,recorder,ref progress);
            if (!string.IsNullOrEmpty(progress)) progress= string.Join(_delim[0],progress.Split(','));

            if (result) result = Utility.CheckMass(minMass,maxMass);
            return result;
        }
        public override string OnConflict(string a, string b)
        {
            System.Collections.Generic.HashSet<string> progressA;
            System.Collections.Generic.HashSet<string> progressB;

            if (string.IsNullOrEmpty(a))
            {
                progressA=new System.Collections.Generic.HashSet<string>();
            }
            else
            {
                progressA=new System.Collections.Generic.HashSet<string>(a.Split(_delim, System.StringSplitOptions.None));
            }

            if (string.IsNullOrEmpty(b))
            {
                progressB=new System.Collections.Generic.HashSet<string>();
            }
            else
            {
                progressB= new System.Collections.Generic.HashSet<string>(b.Split(_delim, System.StringSplitOptions.None));
            }

            progressA.UnionWith(progressB);
            return string.Join(_delim[0],progressA);
        }
    }

    /// <summary>Data for one custom orbit challenge step</summary>
    public class Step_CustomOrbit : SFS.Logs.ChallengeStep
    {
        public double maxApoapsis=double.NaN;
        public double maxEcc=double.NaN;
        public double maxMass=double.NaN;
        public double maxPeriapsis=double.NaN;
        public double maxSma=double.NaN;

        public double minApoapsis=double.NaN;
        public double minEcc=double.NaN;
        public double minMass=double.NaN;
        public double minPeriapsis=double.NaN;
        public double minSma=double.NaN;

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string progress)
        {
            if (location.velocity.Mag_LessThan(1.0)) return false;
            bool success;
            SFS.World.Orbit orbit = SFS.World.Orbit.TryCreateOrbit(location, calculateTimeParameters: false, calculateEncounters: false, out success);
            if (!success) return false;

            if (maxApoapsis!=double.NaN && (orbit.ecc>1 || orbit.apoapsis>maxApoapsis)) return false ;
            if (maxEcc!=double.NaN && orbit.ecc>maxEcc) return false ;
            if (maxPeriapsis!=double.NaN && orbit.periapsis>maxPeriapsis) return false ;
            if (maxSma!=double.NaN && orbit.sma>maxSma) return false ;

            if (minApoapsis!=double.NaN && orbit.apoapsis<minApoapsis) return false ;
            if (minEcc!=double.NaN && orbit.ecc<minEcc) return false ;
            if (minPeriapsis!=double.NaN && orbit.periapsis<minPeriapsis) return false ;
            if (minSma!=double.NaN && orbit.sma<minSma) return false ;
            return Utility.CheckMass(minMass,maxMass);
        }
    }

    /// <summary>Data for one extended orbit challenge step</summary>
    public class Step_OrbitExt : SFS.Logs.Step_Orbit
    {
        public double minMass=double.NaN;
        public double maxMass=double.NaN;

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string progress)
        {
            if (!base.IsCompleted(location,recorder,ref progress)) return false;
            return Utility.CheckMass(minMass,maxMass);
        }
    }
}
