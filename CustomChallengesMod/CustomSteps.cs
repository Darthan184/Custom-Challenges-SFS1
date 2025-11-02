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
