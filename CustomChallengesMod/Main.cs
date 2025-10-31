namespace MultiLaunchpadMod
{
    public class Main : ModLoader.Mod
    {
        public override string ModNameID => "customchallengesmod";
        public override string DisplayName => "Custom Challenges Support";
        public override string Author => "Darthan";
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        public override string ModVersion => "v1.0";
        public override string Description => "Support for custom challenges for custom solar systems";

//~         public System.Collections.Generic.Dictionary<string, SFS.IO.FilePath> UpdatableFiles =>
//~             new System.Collections.Generic.Dictionary<string, SFS.IO.FilePath>()
//~                 {
//~                     {
//~                         "https://github.com/Darthan184/Custom-Challenges-SFS1/releases/latest/CustomChallengesMod.dll"
//~                         , new SFS.IO.FolderPath(ModFolder).ExtendToFile("CustomChallengesMod.dll")
//~                     }
//~                 };


        public static ModLoader.Mod main;
        public static SFS.IO.FolderPath modFolder;

        // This initializes the patcher. This is required if you use any Harmony patches.
        static HarmonyLib.Harmony patcher;

        // This method runs before anything from the game is loaded. This is where you should apply your patches, as shown below.
        public override void Early_Load()
        {
            main = this;
            modFolder = new SFS.IO.FolderPath(ModFolder);
            patcher = new HarmonyLib.Harmony(ModNameID);
            patcher.PatchAll();
        }

        // This tells the loader what to run when your mod is loaded.
        public override void Load()
        {
        }
    }
}
