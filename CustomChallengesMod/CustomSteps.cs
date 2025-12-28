namespace CustomChallengesMod.CustomSteps
{
    public class Utility
    {
        public static bool CheckMass(double minMass,double maxMass)
        {
            if (!double.IsNaN(minMass) || !double.IsNaN(maxMass))
            {
                if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                {
                    double mass = 0;
                    mass = rocket.mass.GetMass();
                    if (mass==0) return false;
                    if (!double.IsNaN(minMass) && mass<minMass) return false;
                    if (!double.IsNaN(maxMass) && mass>maxMass) return false;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public static bool CheckEngines(bool? hasEngines)
        {
            if (hasEngines!=null)
            {
                if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                {
                    if
                        (
                            rocket.partHolder.HasModule<SFS.Parts.Modules.EngineModule>()
                            || rocket.partHolder.HasModule<SFS.Parts.Modules.BoosterModule>()
                        )
                    {
                        return (bool)hasEngines;
                    }
                    else
                    {
                        return !(bool)hasEngines;
                    }
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
        public bool? hasEngines=null;
        public double maxHeight=double.NaN;
        public double maxMass=double.NaN;
        public double minHeight=double.NaN;
        public double minMass=double.NaN;

        public static CustomChallengesMod.CustomSteps.Step_HeightExt Create(SFS.Logs.Step_Height oneStep)
        {
            CustomChallengesMod.CustomSteps.Step_HeightExt result=new CustomChallengesMod.CustomSteps.Step_HeightExt();
            result.planet = oneStep.planet;
            result.minHeight = oneStep.height;
            return result;
        }

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string _)
        {
            if (!double.IsNaN(maxHeight) && location.Height > maxHeight) return false;
            if (!double.IsNaN(minHeight) && location.Height < minHeight) return false;
            return (Utility.CheckMass(minMass,maxMass) && Utility.CheckEngines(hasEngines));
        }

        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append("Height");
            if (planet!=null) result.AppendFormat(", planet:\"{0}\"", planet.codeName);
            if (hasEngines!=null) result.AppendFormat(", hasEngines:{0}", hasEngines);
            if (!double.IsNaN(minHeight)) result.AppendFormat(", minHeight:{0:N0}m", minHeight);
            if (!double.IsNaN(maxHeight)) result.AppendFormat(", maxHeight:{0:N0}m", maxHeight);
            if (!double.IsNaN(minMass)) result.AppendFormat(", minMass:{0:N0}t", minMass);
            if (!double.IsNaN(maxMass)) result.AppendFormat(", maxMass:{0:N0}t", maxMass);
            return result.ToString();
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
            string[] stepProgress=ParseProgress(progress);

            for (int stepIndex=0;stepIndex<steps.Count;stepIndex++)
            {
                SFS.Logs.ChallengeStep oneStep = steps[stepIndex];
                if (oneStep.IsEligible(location.planet) && oneStep.IsCompleted(location,recorder,ref stepProgress[stepIndex]))
                {
                    stepCompleted=true;
                }
            }

            if (stepCompleted)
            {
                progress = null;
            }
            else
            {
                progress = string.Join(_delim[0],stepProgress);
            }

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

        public string[] ParseProgress(string progress)
        {
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

            return stepProgress;
        }

        public override string ToString()
        {
            return ToStringExt();
        }

        public string ToStringExt(int depth=1,string progress="")
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            string[] progressArray= null;

            result.Append("Any");

            if (!string.IsNullOrEmpty(progress))
            {
                if (CustomChallengesMod.UI.Debug)
                {
                    result.AppendFormat(" (in progress \"{0}\")",progress);
                }
                else
                {
                    result.Append(" (in progress)");
                }
                progressArray=ParseProgress(progress);
            }

            for (int stepIndex = 0; stepIndex<steps.Count; stepIndex++)
            {
                SFS.Logs.ChallengeStep oneStep = steps[stepIndex];
                string stepProgress="";
                if (progressArray!=null && progressArray.Length>stepIndex) stepProgress=progressArray[stepIndex];

                result.AppendLine();

                for (int i=0;i<=depth;i++)  result.Append("... ");

                if (oneStep is CustomChallengesMod.CustomSteps.Step_OneOf oneOneOfStep)
                {
                    if (stepProgress!="")
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneOneOfStep.ToStringExt(depth: depth+1,progress: stepProgress));
                    }
                    else
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneOneOfStep.ToStringExt(depth: depth+1));
                    }
                }
                else if(oneStep is CustomChallengesMod.CustomSteps.Step_AllOf oneAllOfStep)
                {
                    if (stepProgress!="")
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneAllOfStep.ToStringExt(depth: depth+1, progress: stepProgress));
                    }
                    else
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneAllOfStep.ToStringExt(depth:depth+1));
                    }
                }
                else if(oneStep is CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt oneAny_LandmarkStep)
                {
                    if (stepProgress!="")
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneAny_LandmarkStep.ToStringExt(progress:stepProgress));
                    }
                    else
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneStep);
                    }
                }
                else
                {
                    if (stepProgress!="")
                    {
                        result.AppendFormat("{0:D}: {1} (progress: \"{2}\"", stepIndex+1 , oneStep, stepProgress);
                    }
                    else
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneStep);
                    }
                }
            }
            return result.ToString();
        }
    }

    /// <summary>Data for a all of several challenge steps (replacement for multi)</summary>
    public class Step_AllOf : SFS.Logs.ChallengeStep
    {
        public System.Collections.Generic.List<SFS.Logs.ChallengeStep> steps;
        private string [] _delim = new string[]{"|a"};

        public static CustomChallengesMod.CustomSteps.Step_AllOf Create(SFS.Logs.MultiStep oneStep)
        {
            CustomChallengesMod.CustomSteps.Step_AllOf result=new CustomChallengesMod.CustomSteps.Step_AllOf();
            result.Depth = 0;
            result.steps = new  System.Collections.Generic.List<SFS.Logs.ChallengeStep>();

            foreach (SFS.Logs.ChallengeStep oneSubStep in oneStep.steps)
            {
                if (oneSubStep is SFS.Logs.Step_Downrange oneStep_Downrange)
                {
                    result.steps.Add(CustomChallengesMod.CustomSteps.Step_Downrange.Create(oneStep_Downrange));
                }
                else if (oneSubStep is SFS.Logs.Step_Height oneStep_Height)
                {
                    result.steps.Add(CustomChallengesMod.CustomSteps.Step_HeightExt.Create(oneStep_Height));
                }
                else if (oneSubStep is SFS.Logs.Step_Impact oneStep_Impact)
                {
                    result.steps.Add(CustomChallengesMod.CustomSteps.Step_Impact.Create(oneStep_Impact));
                }
                else if (oneSubStep is SFS.Logs.Step_Land oneStep_Land)
                {
                    result.steps.Add(CustomChallengesMod.CustomSteps.Step_LandExt.Create(oneStep_Land));
                }
                else if (oneSubStep is SFS.Logs.Step_Orbit oneStep_Orbit)
                {
                    result.steps.Add(CustomChallengesMod.CustomSteps.Step_OrbitExt.Create(oneStep_Orbit));
                }
            }
            return result;
        }

        public int Depth
        { get { return (int)(_delim[0][1])-(int)'a'; } set { _delim[0]="|"+(char)((int)'a'+value);} }

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string progress)
        {
            bool stepCompleted=true;
            string[] stepProgress=ParseProgress(progress);

            for (int stepIndex=0;stepIndex<steps.Count;stepIndex++)
            {
                SFS.Logs.ChallengeStep oneStep = steps[stepIndex];
                if (stepProgress[stepIndex]!="**")
                {
                    if (oneStep.IsEligible(location.planet) && oneStep.IsCompleted(location,recorder,ref stepProgress[stepIndex]))
                    {
                        stepProgress[stepIndex]="**";
                    }
                    else
                    {
                        stepCompleted=false;
                    }
                }
            }

            if (stepCompleted)
            {
                progress = null;
            }
            else
            {
                progress = string.Join(_delim[0],stepProgress);
            }

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

        public string[] ParseProgress(string progress)
        {
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

            return stepProgress;
        }

        public override string ToString()
        {
            return ToStringExt();
        }

        public string ToStringExt(int depth=1,string progress="")
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            string[] progressArray= null;

            result.Append("Multi");

            if (!string.IsNullOrEmpty(progress))
            {
                if (CustomChallengesMod.UI.Debug)
                {
                    result.AppendFormat(" (in progress \"{0}\")",progress);
                }
                else
                {
                    result.Append(" (in progress)");
                }
                progressArray=ParseProgress(progress);
            }

            for (int stepIndex = 0; stepIndex<steps.Count; stepIndex++)
            {
                SFS.Logs.ChallengeStep oneStep = steps[stepIndex];
                System.Text.StringBuilder prefix = new System.Text.StringBuilder();
                string stepProgress="";

                if (progressArray!=null && progressArray.Length>stepIndex) stepProgress=progressArray[stepIndex];

                result.AppendLine();

                for (int i=0;i<=depth;i++)  prefix.Append("... ");
                result.Append(prefix.ToString());

                if (oneStep is CustomChallengesMod.CustomSteps.Step_OneOf oneOneOfStep)
                {
                    if (stepProgress=="**")
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneOneOfStep.ToStringExt(depth:depth+1));
                        result.AppendLine();
                        result.AppendFormat("{0} (complete)",prefix);
                    }
                    else if (stepProgress!="")
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneOneOfStep.ToStringExt(depth:depth+1,progress:stepProgress));
                    }
                    else
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneOneOfStep.ToStringExt(depth:depth+1));
                    }
                }
                else if(oneStep is CustomChallengesMod.CustomSteps.Step_AllOf oneAllOfStep)
                {
                    if (stepProgress=="**")
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneAllOfStep.ToStringExt(depth:depth+1));
                        result.AppendLine();
                        result.AppendFormat("{0} (complete)",prefix);
                    }
                    else if (stepProgress!="")
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneAllOfStep.ToStringExt(depth: depth+1,progress:stepProgress));
                    }
                    else
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneAllOfStep.ToStringExt(depth:depth+1));
                    }
                }
                else if(oneStep is CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt oneAny_LandmarkStep)
                {
                    if (stepProgress=="**")
                    {
                        result.AppendFormat("{0:D}: {1} (complete)", stepIndex+1 , oneStep);
                    }
                    else if (stepProgress!="")
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneAny_LandmarkStep.ToStringExt(progress: stepProgress));
                    }
                    else
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneStep);
                    }
                }
                else
                {
                    if (stepProgress=="**")
                    {
                        result.AppendFormat("{0:D}: {1} (complete)", stepIndex+1 , oneStep);
                    }
                    else if (stepProgress!="")
                    {
                        result.AppendFormat("{0:D}: {1} (progress: \"{2}\")", stepIndex+1 , oneStep, stepProgress);
                    }
                    else
                    {
                        result.AppendFormat("{0:D}: {1}", stepIndex+1 , oneStep);
                    }
                }
            }
            return result.ToString();
        }
    }

    /// <summary>Data for one extended landing challenge step</summary>
    public class Step_LandExt : SFS.Logs.Step_Land
    {
        public bool? hasEngines=null;
        public double maxMass=double.NaN;
        public double minMass=double.NaN;

        public static CustomChallengesMod.CustomSteps.Step_LandExt Create(SFS.Logs.Step_Land oneStep)
        {
            CustomChallengesMod.CustomSteps.Step_LandExt result=new CustomChallengesMod.CustomSteps.Step_LandExt();
            result.planet = oneStep.planet;
            return result;
        }

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string progress)
        {
            if (!base.IsCompleted(location,recorder,ref progress)) return false;
            return (Utility.CheckMass(minMass,maxMass) && Utility.CheckEngines(hasEngines));
        }

        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append("Land");
            if (planet!=null) result.AppendFormat(", planet:\"{0}\"", planet.codeName);
            if (hasEngines!=null) result.AppendFormat(", hasEngines:{0}", hasEngines);
            if (!double.IsNaN(minMass)) result.AppendFormat(", minMass:{0:N0}t", minMass);
            if (!double.IsNaN(maxMass)) result.AppendFormat(", maxMass:{0:N0}t", maxMass);
            return result.ToString();
        }
    }

    /// <summary>Data for one extended any landmarks step</summary>
    public class Step_Any_LandmarksExt : SFS.Logs.Step_Any_Landmarks
    {
        private string [] _delim = new string[]{"|a"};
        public bool? hasEngines=null;
        public double maxMass=double.NaN;
        public double minMass=double.NaN;

        public static CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt Create(SFS.Logs.Step_Any_Landmarks oneStep)
        {
            CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt result=new CustomChallengesMod.CustomSteps.Step_Any_LandmarksExt();
            result.planet = oneStep.planet;
            result.count = oneStep.count;
            return result;
        }

        public int Depth
        { get { return (int)(_delim[0][1])-(int)'a'; } set { _delim[0]="|"+(char)((int)'a'+value);} }

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string progress)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(progress)) progress= string.Join(",",ParseProgress(progress));

            result = base.IsCompleted(location,recorder,ref progress);

            if (!string.IsNullOrEmpty(progress)) progress= string.Join(_delim[0],progress.Split(','));

            if (result) result = (Utility.CheckMass(minMass,maxMass) && Utility.CheckEngines(hasEngines));
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

        public string[] ParseProgress(string progress)
        {
            return progress.Split(_delim, System.StringSplitOptions.None);
        }

        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append("Any_Landmarks");
            if (planet!=null) result.AppendFormat(", planet:\"{0}\"", planet.codeName);
            result.AppendFormat(", count:{0}", count);
            if (hasEngines!=null) result.AppendFormat(", hasEngines:{0}", hasEngines);
            if (!double.IsNaN(minMass)) result.AppendFormat(", minMass:{0:N0}t", minMass);
            if (!double.IsNaN(maxMass)) result.AppendFormat(", maxMass:{0:N0}t", maxMass);
            return result.ToString();
        }

        public string ToStringExt(string progress="")
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append("Any_Landmarks");
            if (planet!=null) result.AppendFormat(", planet:\"{0}\"", planet.codeName);
            result.AppendFormat(", count:{0}", count);
            if (hasEngines!=null) result.AppendFormat(", hasEngines:{0}", hasEngines);
            if (!double.IsNaN(minMass)) result.AppendFormat(", minMass:{0:N0}t", minMass);
            if (!double.IsNaN(maxMass)) result.AppendFormat(", maxMass:{0:N0}t", maxMass);
            if (!string.IsNullOrEmpty(progress))
            {
                string[] progressArray=ParseProgress(progress);
                System.Text.StringBuilder formattedProgress = new System.Text.StringBuilder();
                foreach(string value in progressArray)
                {
                    if (formattedProgress.Length>0) formattedProgress.Append(",");
                    formattedProgress.AppendFormat("\"{0}\"", value);
                }
                result.AppendFormat(" (progress:{0})", formattedProgress);
            };
            return result.ToString();
        }
    }

    /// <summary>Data for one custom orbit challenge step</summary>
    public class Step_CustomOrbit : SFS.Logs.ChallengeStep
    {
        public bool? hasEngines=null;

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

            if (!double.IsNaN(maxApoapsis) && (orbit.ecc>1 || orbit.apoapsis>maxApoapsis)) return false ;
            if (!double.IsNaN(maxEcc) && orbit.ecc>maxEcc) return false ;
            if (!double.IsNaN(maxPeriapsis) && orbit.periapsis>maxPeriapsis) return false ;
            if (!double.IsNaN(maxSma) && orbit.sma>maxSma) return false ;

            if (!double.IsNaN(minApoapsis) && orbit.apoapsis<minApoapsis) return false ;
            if (!double.IsNaN(minEcc) && orbit.ecc<minEcc) return false ;
            if (!double.IsNaN(minPeriapsis) && orbit.periapsis<minPeriapsis) return false ;
            if (!double.IsNaN(minSma) && orbit.sma<minSma) return false ;
            return (Utility.CheckMass(minMass,maxMass) && Utility.CheckEngines(hasEngines));
        }

        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append("CustomOrbit");
            if (planet!=null) result.AppendFormat(", planet:\"{0}\"", planet.codeName);
            if (!double.IsNaN(minApoapsis)) result.AppendFormat(", minApoapsis:{0:N0}m", minApoapsis);
            if (!double.IsNaN(maxApoapsis)) result.AppendFormat(", maxApoapsis:{0:N0}m", maxApoapsis);
            if (!double.IsNaN(minEcc)) result.AppendFormat(", minEcc:{0}", minEcc);
            if (!double.IsNaN(maxEcc)) result.AppendFormat(", maxEcc:{0}", maxEcc);
            if (hasEngines!=null) result.AppendFormat(", hasEngines:{0}", hasEngines);
            if (!double.IsNaN(minMass)) result.AppendFormat(", minMass:{0:N0}t", minMass);
            if (!double.IsNaN(maxMass)) result.AppendFormat(", maxMass:{0:N0}t", maxMass);
            if (!double.IsNaN(minPeriapsis)) result.AppendFormat(", minPeriapsis:{0:N0}m", minPeriapsis);
            if (!double.IsNaN(maxPeriapsis)) result.AppendFormat(", maxPeriapsis:{0:N0}m", maxPeriapsis);
            if (!double.IsNaN(minSma)) result.AppendFormat(", minSma:{0:N0}m", minSma);
            if (!double.IsNaN(maxSma)) result.AppendFormat(", maxSma:{0:N0}m", maxSma);
            return result.ToString();
        }
    }

    /// <summary>Data for one extended orbit challenge step</summary>
    public class Step_OrbitExt : SFS.Logs.Step_Orbit
    {
        public bool? hasEngines=null;
        public double minMass=double.NaN;
        public double maxMass=double.NaN;

        public static CustomChallengesMod.CustomSteps.Step_OrbitExt Create(SFS.Logs.Step_Orbit oneStep)
        {
            CustomChallengesMod.CustomSteps.Step_OrbitExt result=new CustomChallengesMod.CustomSteps.Step_OrbitExt();
            result.planet = oneStep.planet;
            result.orbit = oneStep.orbit;
            return result;
        }

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string progress)
        {
            if (orbit==SFS.Stats.StatsRecorder.Tracker.State_Orbit.Esc && location.planet.SOI==double.PositiveInfinity)
            {
                bool success;
                SFS.World.Orbit orbit = SFS.World.Orbit.TryCreateOrbit(location, calculateTimeParameters: false, calculateEncounters: false, out success);
                if (!success || orbit.apoapsis!=double.PositiveInfinity) return false;
            }
            else
            {
                if (!base.IsCompleted(location,recorder,ref progress)) return false;
            }
            return (Utility.CheckMass(minMass,maxMass) && Utility.CheckEngines(hasEngines));
        }

        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append("Orbit");
            if (planet!=null) result.AppendFormat(", planet:\"{0}\"", planet.codeName);
            result.AppendFormat(", orbit:{0}", orbit);
            if (hasEngines!=null) result.AppendFormat(", hasEngines:{0}", hasEngines);
            if (!double.IsNaN(minMass)) result.AppendFormat(", minMass:{0:N0}t", minMass);
            if (!double.IsNaN(maxMass)) result.AppendFormat(", maxMass:{0:N0}t", maxMass);
            return result.ToString();
        }
    }

    /// <summary>Data for downrange challenge step</summary>
    public class Step_Downrange : SFS.Logs.Step_Downrange
    {
        public static CustomChallengesMod.CustomSteps.Step_Downrange Create(SFS.Logs.Step_Downrange oneStep)
        {
            CustomChallengesMod.CustomSteps.Step_Downrange result=new CustomChallengesMod.CustomSteps.Step_Downrange();
            result.planet = oneStep.planet;
            result.downrange = oneStep.downrange;
            return result;
        }

        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append("Downrange");
            if (planet!=null) result.AppendFormat(", planet:\"{0}\"", planet.codeName);
            result.AppendFormat(", downrange:{0:N0}m", downrange);
            return result.ToString();
        }
    }

    /// <summary>Data for impact challenge step</summary>
    public class Step_Impact : SFS.Logs.Step_Impact
    {
        public static CustomChallengesMod.CustomSteps.Step_Impact Create(SFS.Logs.Step_Impact oneStep)
        {
            CustomChallengesMod.CustomSteps.Step_Impact result=new CustomChallengesMod.CustomSteps.Step_Impact();
            result.planet = oneStep.planet;
            result.impactVelocity = oneStep.impactVelocity;
            return result;
        }

        public override string ToString()
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            result.Append("Impact");
            if (planet!=null) result.AppendFormat(", planet:\"{0}\"", planet.codeName);
            result.AppendFormat(", impactVelocity:{0:N0}m/s", impactVelocity);
            return result.ToString();
        }
    }
}
