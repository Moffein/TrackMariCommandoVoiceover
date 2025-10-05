using BepInEx;
using UnityEngine;
using RoR2;
using BepInEx.Configuration;
using UnityEngine.AddressableAssets;
using TrackMariCommandoVoiceover.Modules;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Collections.Generic;
using Rewired.UI.ControlMapper;
using System.Runtime.CompilerServices;
using System;
using BaseVoiceoverLib;
using TrackMariCommandoVoiceover.Components;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace TrackMariCommandoVoiceover
{
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.Moffein.BaseVoiceoverLib", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("com.Schale.TrackMariCommandoVoiceover", "TrackMariCommandoVoiceover", "1.0.0")]
    public class TrackMariCommandoVoiceoverPlugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> enableVoicelines;
        public static bool playedSeasonalVoiceline = false;
        public static AssetBundle assetBundle;
        public static SurvivorDef survivorDef = Addressables.LoadAssetAsync<SurvivorDef>("RoR2/Base/Commando/Commando.asset").WaitForCompletion();
        public static ConfigEntry<KeyboardShortcut> btnTitle, btnThanks, btnHurt, btnEx1, btnEx2, btnEx3, btnExL1, btnExL2, btnExL3, btnExN1, btnExN2, btnExN3, btnExNL1, btnExNL2, btnExNL3, btnCommon, btnTactical;

        public void Awake()
        {
            Files.PluginInfo = this.Info;
            RoR2.RoR2Application.onLoad += OnLoad;
            new Content().Initialize();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TrackMariCommandoVoiceover.trackmaricommandovoiceoverbundle"))
            {
                assetBundle = AssetBundle.LoadFromStream(stream);
            }

            SoundBanks.Init();

            InitNSE();

            enableVoicelines = base.Config.Bind<bool>(new ConfigDefinition("Settings", "Enable Voicelines"), true, new ConfigDescription("Enable voicelines when using the Track Mari Commando Skin."));
            enableVoicelines.SettingChanged += EnableVoicelines_SettingChanged;

            btnHurt = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Hurt"), KeyboardShortcut.Empty);
            btnThanks = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Thanks"), KeyboardShortcut.Empty);
            btnCommon = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Common Skill"), KeyboardShortcut.Empty);
            btnTactical = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Tactical Action"), KeyboardShortcut.Empty);
            btnTitle = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Blue Archive"), KeyboardShortcut.Empty);
            btnEx1 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "EX 1"), KeyboardShortcut.Empty);
            btnEx2 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "EX 2"), KeyboardShortcut.Empty);
            btnEx3 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "EX 3"), KeyboardShortcut.Empty);
            btnExL1 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "EX Level 1"), KeyboardShortcut.Empty);
            btnExL2 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "EX Level 2"), KeyboardShortcut.Empty);
            btnExL3 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "EX Level 3"), KeyboardShortcut.Empty);
            btnExN1 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Normal EX 1"), KeyboardShortcut.Empty);
            btnExN2 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Normal EX 2"), KeyboardShortcut.Empty);
            btnExN3 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Normal EX 3"), KeyboardShortcut.Empty);
            btnExNL1 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Normal EX Level 1"), KeyboardShortcut.Empty);
            btnExNL2 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Normal EX Level 2"), KeyboardShortcut.Empty);
            btnExNL3 = base.Config.Bind<KeyboardShortcut>(new ConfigDefinition("Keybinds", "Normal EX Level 3"), KeyboardShortcut.Empty);

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions"))
            {
                RiskOfOptionsCompat();
            }
        }

        private void EnableVoicelines_SettingChanged(object sender, EventArgs e)
        {
            RefreshNSE();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void RiskOfOptionsCompat()
        {
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.CheckBoxOption(enableVoicelines));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnTitle));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnThanks));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnTactical));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnCommon));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnHurt));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnEx1));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnEx2));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnEx3));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnExL1));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnExL2));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnExL3));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnExN1));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnExN2));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnExN3));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnExNL1));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnExNL2));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.KeyBindOption(btnExNL3));

            RiskOfOptions.ModSettingsManager.SetModIcon(assetBundle.LoadAsset<Sprite>("trackmariicon"));
           
        }

        private void OnLoad()
        {
            SkinDef mariSkin = null;
            SkinDef[] skins = SkinCatalog.FindSkinsForBody(BodyCatalog.FindBodyIndex("CommandoBody"));
            foreach (SkinDef skinDef in skins)
            {
                if (skinDef.name == "DefTrackMariCommando")
                {
                    mariSkin = skinDef;
                    break;
                }
            }

            if (!mariSkin)
            {
                Debug.LogError("TrackMariCommandoVoiceover: Track Mari Commando SkinDef not found. Voicelines will not work!");
            }
            else
            {
                VoiceoverInfo voiceoverInfo = new VoiceoverInfo(typeof(TrackMariCommandoVoiceoverComponent), mariSkin, "CommandoBody");
                voiceoverInfo.selectActions += TrackMariSelect;
            }

            RefreshNSE();
        }

        private void TrackMariSelect(GameObject mannequinObject)
        {
            if (!enableVoicelines.Value) return;

            bool played = false;
            if (!playedSeasonalVoiceline)
            {
                if ((System.DateTime.Today.Month == 1 && System.DateTime.Today.Day == 1) || System.DateTime.Today.Month == 12 && System.DateTime.Today.Day == 31)
                {
                    Util.PlaySound("Play_TrackMariCommando_Lobby_Newyear", mannequinObject);
                    played = true;
                }
                else if (System.DateTime.Today.Month == 9 && System.DateTime.Today.Day == 12)
                {
                    Util.PlaySound("Play_TrackMariCommando_Lobby_bday", mannequinObject);
                    played = true;
                }
                else if (System.DateTime.Today.Month == 10 && System.DateTime.Today.Day == 31)
                {
                    Util.PlaySound("Play_TrackMariCommando_Lobby_Halloween", mannequinObject);
                    played = true;
                }
                else if (System.DateTime.Today.Month == 12 && (System.DateTime.Today.Day == 24 || System.DateTime.Today.Day == 25))
                {
                    Util.PlaySound("Play_TrackMariCommando_Lobby_xmas", mannequinObject);
                    played = true;
                }

                if (played)
                {
                    playedSeasonalVoiceline = true;
                }
            }
            if (!played)
            {
                if (Util.CheckRoll(5f))
                {
                    Util.PlaySound("Play_TrackMariCommando_Title", mannequinObject);
                }
                else
                {
                    Util.PlaySound("Play_TrackMariCommando_Lobby", mannequinObject);
                }
            }
        }

        private void InitNSE()
        {
            TrackMariCommandoVoiceoverComponent.nseShout = RegisterNSE("Play_TrackMariCommando_Shout");
            TrackMariCommandoVoiceoverComponent.nseBlock = RegisterNSE("Play_TrackMariCommando_Block");
            TrackMariCommandoVoiceoverComponent.nseDisappoint = RegisterNSE("Play_TrackMariCommando_Covered");
            TrackMariCommandoVoiceoverComponent.nseTacticalAction = RegisterNSE("Play_TrackMariCommando_TacticalAction");
            TrackMariCommandoVoiceoverComponent.nseCommonSkill = RegisterNSE("Play_TrackMariCommando_CommonSkill");

            //For binds
            TrackMariCommandoVoiceoverComponent.nseHurt = RegisterNSE("Play_TrackMariCommando_Hurt");
            TrackMariCommandoVoiceoverComponent.nseThanks = RegisterNSE("Play_TrackMariCommando_Recovery");
            TrackMariCommandoVoiceoverComponent.nseTitle = RegisterNSE("Play_TrackMariCommando_Title");

            TrackMariCommandoVoiceoverComponent.nseEx1 = RegisterNSE("Play_TrackMariCommando_EX_1");
            TrackMariCommandoVoiceoverComponent.nseEx2 = RegisterNSE("Play_TrackMariCommando_EX_2");
            TrackMariCommandoVoiceoverComponent.nseEx3 = RegisterNSE("Play_TrackMariCommando_EX_3");

            TrackMariCommandoVoiceoverComponent.nseExL1 = RegisterNSE("Play_TrackMariCommando_EX_Level_1");
            TrackMariCommandoVoiceoverComponent.nseExL2 = RegisterNSE("Play_TrackMariCommando_EX_Level_2");
            TrackMariCommandoVoiceoverComponent.nseExL3 = RegisterNSE("Play_TrackMariCommando_EX_Level_3");

            TrackMariCommandoVoiceoverComponent.nseExN1 = RegisterNSE("Play_TrackMariCommando_Normal_EX_1");
            TrackMariCommandoVoiceoverComponent.nseExN2 = RegisterNSE("Play_TrackMariCommando_Normal_EX_2");
            TrackMariCommandoVoiceoverComponent.nseExN3 = RegisterNSE("Play_TrackMariCommando_Normal_EX_3");

            TrackMariCommandoVoiceoverComponent.nseExNL1 = RegisterNSE("Play_TrackMariCommando_Normal_EX_Level_1");
            TrackMariCommandoVoiceoverComponent.nseExNL2 = RegisterNSE("Play_TrackMariCommando_Normal_EX_Level_2");
            TrackMariCommandoVoiceoverComponent.nseExNL3 = RegisterNSE("Play_TrackMariCommando_Normal_EX_Level_3");
        }

        private NetworkSoundEventDef RegisterNSE(string eventName)
        {
            NetworkSoundEventDef nse = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
            nse.eventName = eventName;
            Content.networkSoundEventDefs.Add(nse);
            nseList.Add(new NSEInfo(nse));
            return nse;
        }

        public void RefreshNSE()
        {
            foreach (NSEInfo nse in nseList)
            {
                nse.ValidateParams();
            }
        }

        public static List<NSEInfo> nseList = new List<NSEInfo>();
        public class NSEInfo
        {
            public NetworkSoundEventDef nse;
            public uint akId = 0u;
            public string eventName = string.Empty;

            public NSEInfo(NetworkSoundEventDef source)
            {
                this.nse = source;
                this.akId = source.akId;
                this.eventName = source.eventName;
            }

            private void DisableSound()
            {
                nse.akId = 0u;
                nse.eventName = string.Empty;
            }

            private void EnableSound()
            {
                nse.akId = this.akId;
                nse.eventName = this.eventName;
            }

            public void ValidateParams()
            {
                if (this.akId == 0u) this.akId = nse.akId;
                if (this.eventName == string.Empty) this.eventName = nse.eventName;

                if (!enableVoicelines.Value)
                {
                    DisableSound();
                }
                else
                {
                    EnableSound();
                }
            }
        }
    }
}
