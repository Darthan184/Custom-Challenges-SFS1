namespace CustomChallengesMod.CustomSteps
{
    /// <summary>Data for one custom height challenge step</summary>
    public class Step_HeightPlus : SFS.Logs.Step_Height
    {
        public double minMass=0;
        public double maxMass=0;

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string _)
        {
            if (location.Height > (double)this.height)
            {
                if (minMass!=0 || maxMass!=0)
                {
                    double mass = 0;

                    if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                    {
                        mass = rocket.mass.GetMass();
                    }

                    if (mass!=0 && minMass!=0 && mass<minMass) return false;
                    if (mass!=0 && maxMass!=0 && mass>maxMass) return false;
                }
                if (this.checkVelocity)
                {
                    return location.velocity.Mag_MoreThan(20.0);
                }
                return true;
            }
            return false;
        }
    }

    /// <summary>Data for one custom landing challenge step</summary>
    public class Step_LandPlus : SFS.Logs.Step_Land
    {
        public double minMass=0;
        public double maxMass=0;

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string _)
        {
            if (location.planet == base.planet && recorder.tracker.state_Landed && location.velocity.Mag_LessThan(0.1))
            {
                if (minMass!=0 || maxMass!=0)
                {
                    double mass = 0;

                    if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                    {
                        mass = rocket.mass.GetMass();
                    }

                    if (mass!=0 && minMass!=0 && mass<minMass) return false;
                    if (mass!=0 && maxMass!=0 && mass>maxMass) return false;
                }
                return true;
            }
            return false;
        }
    }

    /// <summary>Data for one custom orbit challenge step</summary>
    public class Step_OrbitPlus : SFS.Logs.Step_Orbit
    {
        public double minMass=0;
        public double maxMass=0;

        public override bool IsCompleted(SFS.World.Location location, SFS.Stats.StatsRecorder recorder, ref string _)
        {
            if (location.planet == base.planet && recorder.tracker.state_Orbit == this.orbit)
            {
                if (minMass!=0 || maxMass!=0)
                {
                    double mass = 0;

                    if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                    {
                        mass = rocket.mass.GetMass();
                    }

                    if (mass!=0 && minMass!=0 && mass<minMass) return false;
                    if (mass!=0 && maxMass!=0 && mass>maxMass) return false;
                }
                return true;
            }
            return false;
        }
    }
}
