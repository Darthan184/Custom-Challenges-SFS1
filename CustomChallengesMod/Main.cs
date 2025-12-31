namespace CustomChallengesMod
{
    public class Main : ModLoader.Mod
    {
        public override string ModNameID => "customchallengesmod";
        public override string DisplayName => "Custom Challenges Support";
        public override string Author => "Darthan";
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        public override string ModVersion => "v1.2.1";
        public override string Description => "Support for custom challenges for custom solar systems";
        public override System.Collections.Generic.Dictionary<string, string> Dependencies { get; } =
            new System.Collections.Generic.Dictionary<string, string> { { "UITools", "1.1.5" } };

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
            CustomChallengesMod.SettingsManager.Load();
            UnityEngine.GameObject.DontDestroyOnLoad((new UnityEngine.GameObject("Custom Challenges-UI").AddComponent<CustomChallengesMod.UI>()).gameObject);

            if (CustomChallengesMod.SettingsManager.settings.debug)
            {
                ModLoader.Helpers.SceneHelper.OnHubSceneLoaded += CustomChallengesMod.UI.ShowGUI;
                ModLoader.Helpers.SceneHelper.OnHubSceneUnloaded += CustomChallengesMod.UI.GUIInActive;
            }
            ModLoader.Helpers.SceneHelper.OnWorldSceneLoaded += CustomChallengesMod.UI.ShowGUI;
            ModLoader.Helpers.SceneHelper.OnWorldSceneUnloaded += CustomChallengesMod.UI.GUIInActive;
        }
    }
}
