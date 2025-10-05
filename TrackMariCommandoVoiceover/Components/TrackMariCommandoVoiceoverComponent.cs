using System;
using System.Collections.Generic;
using BaseVoiceoverLib;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace TrackMariCommandoVoiceover.Components
{
    public class TrackMariCommandoVoiceoverComponent : BaseVoiceoverComponent
    {
        public static NetworkSoundEventDef nseShout, nseBlock, nseDisappoint, nseCommonSkill, nseTacticalAction;

        //for voiceline binds
        public static NetworkSoundEventDef nseHurt, nseTitle, nseThanks, nseEx1, nseEx2, nseEx3, nseExL1, nseExL2, nseExL3, nseExN1, nseExN2, nseExN3, nseExNL1, nseExNL2, nseExNL3;

        private float levelCooldown = 0f;
        private float blockedCooldown = 0f;
        private float lowHealthCooldown = 0f;
        private float shrineFailCooldown = 0f;
        private float specialCooldown = 0f;

        private bool acquiredScepter = false;

        protected override void Start()
        {
            base.Start();
            if (inventory && inventory.GetItemCount(scepterIndex) > 0) acquiredScepter = true;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (levelCooldown > 0f) levelCooldown -= Time.fixedDeltaTime;
            if (blockedCooldown > 0f) blockedCooldown -= Time.fixedDeltaTime;
            if (lowHealthCooldown > 0f) lowHealthCooldown -= Time.fixedDeltaTime;
            if (shrineFailCooldown > 0f) shrineFailCooldown -= Time.fixedDeltaTime;
            if (specialCooldown > 0f) specialCooldown -= Time.fixedDeltaTime;
        }

        public override void PlayDamageBlockedServer()
        {
            if (!NetworkServer.active || blockedCooldown > 0f) return;
            bool played = TryPlayNetworkSound(nseBlock, 1.8f, false);
            if (played) blockedCooldown = 60f;
        }

        public override void PlayDeath()
        {
            TryPlaySound("Play_TrackMariCommando_Defeat", 4f, true);
        }

        public override void PlayHurt(float percentHPLost)
        {
            if (percentHPLost >= 0.1f)
            {
                TryPlaySound("Play_TrackMariCommando_Hurt", 0f, false);
            }
        }

        public override void PlayLevelUp()
        {
            if (levelCooldown > 0f) return;
            if (TryPlaySound("Play_TrackMariCommando_Levelup", 5.3f, false)) levelCooldown = 60f;
        }

        public override void PlayLowHealth()
        {
            if (lowHealthCooldown > 0f) return;
            bool playedSound;

            int rand = UnityEngine.Random.Range(0, 3);
            switch (rand)
            {
                case 0:
                    playedSound = TryPlaySound("Play_TrackMariCommando_Memorial_1_1", 5.3f, false);
                    break;
                case 1:
                    playedSound = TryPlaySound("Play_TrackMariCommando_Memorial_2_2", 3.5f, false);
                    break;
                default:
                    playedSound = TryPlaySound("Play_TrackMariCommando_EX_3", 2f, false);
                    break;
            }
                
            if (playedSound) lowHealthCooldown = 60f;
        }

        public override void PlaySpawn()
        {
            TryPlaySound("Play_TrackMariCommando_Spawn", 2.35f, true);
        }
        

        public override void PlayTeleporterFinish()
        {
            int rand = UnityEngine.Random.Range(0, 4);
            switch (rand)
            {
                case 0:
                    TryPlaySound("Play_TrackMariCommando_Victory_1", 3f, false);
                    break;
                case 1:
                    TryPlaySound("Play_TrackMariCommando_Victory_2", 4f, false);
                    break;
                case 2:
                    TryPlaySound("Play_TrackMariCommando_Victory_3", 3.2f, false);
                    break;
                default:
                    TryPlaySound("Play_TrackMariCommando_Victory_4", 2.25f, false);
                    break;
            }
        }

        public override void PlayTeleporterStart()
        {
            int rand = UnityEngine.Random.Range(0, 7);
            switch (rand)
            {
                case 0:
                    TryPlaySound("Play_TrackMariCommando_Normal_CommonSkill", 1.6f, false);
                    break;
                case 1:
                    TryPlaySound("Play_TrackMariCommando_Normal_EX_1", 1.2f, false);
                    break;
                case 2:
                    TryPlaySound("Play_TrackMariCommando_Normal_EX_2", 1.7f, false);
                    break;
                case 3:
                    TryPlaySound("Play_TrackMariCommando_Normal_EX_3", 1.3f, false);
                    break;
                case 4:
                    TryPlaySound("Play_TrackMariCommando_Normal_EX_Level_1", 2.2f, false);
                    break;
                case 5:
                    TryPlaySound("Play_TrackMariCommando_Normal_EX_Level_2", 2f, false);
                    break;
                default:
                    TryPlaySound("Play_TrackMariCommando_Normal_EX_Level_3", 2.2f, false);
                    break;
            }
        }

        public override void PlayUtilityAuthority(GenericSkill skill)
        {
            TryPlayNetworkSound(nseShout, 0f, false);
        }

        public override void PlayVictory()
        {
            TryPlaySound("Play_TrackMariCommando_Memorial_5", 24.5f, true);
        }

        protected override void Inventory_onItemAddedClient(ItemIndex itemIndex)
        {
            base.Inventory_onItemAddedClient(itemIndex);
            if (scepterIndex != ItemIndex.None && itemIndex == scepterIndex)
            {
                PlayAcquireScepter();
            }
            else
            {
                ItemDef id = ItemCatalog.GetItemDef(itemIndex);
                if (id == RoR2Content.Items.Squid || id == RoR2Content.Items.Plant || id == RoR2Content.Items.SlowOnHit)
                {
                    PlayBadItem();
                }
                else if (id && id.deprecatedTier == ItemTier.Tier3)
                {
                    PlayAcquireLegendary();
                }
            }
        }

        public override void PlaySpecialAuthority(GenericSkill skill)
        {
            if (specialCooldown > 0f) return;
            bool played = false;
            if (Util.CheckRoll(50f))
            {
                played = TryPlayNetworkSound(nseCommonSkill, 1.9f, false);
            }
            else
            {
                played = TryPlayNetworkSound(nseTacticalAction, 1.3f, false);
            }
            if (played) specialCooldown = 20f;
        }

        public void PlayAcquireScepter()
        {
            if (acquiredScepter) return;
            TryPlaySound("Play_TrackMariCommando_ExWeapon_Get", 8.5f, true);
            acquiredScepter = true;
        }

        public void PlayBadItem()
        {
            TryPlaySound("Play_TrackMariCommando_Covered", 1f, false);
        }

        public void PlayAcquireLegendary()
        {
            int rand = UnityEngine.Random.Range(0, 4);
            switch (rand)
            {
                case 0:
                    TryPlaySound("Play_TrackMariCommando_Relationship_1", 5.9f, false);
                    break;
                case 1:
                    TryPlaySound("Play_TrackMariCommando_Relationship_2", 6.1f, false);
                    break;
                case 2:
                    TryPlaySound("Play_TrackMariCommando_Relationship_3", 5.5f, false);
                    break;
                default:
                    TryPlaySound("Play_TrackMariCommando_Relationship_4", 4.6f, false);
                    break;
            }
        }

        public override void PlayShrineOfChanceFailServer()
        {
            if (shrineFailCooldown > 0f) return;
            if (Util.CheckRoll(15f))
            {
                bool played = TryPlayNetworkSound(nseDisappoint, 1f, false);
                if (played) shrineFailCooldown = 60f;
            }
        }

        protected override void CheckInputs()
        {
            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnTitle))
            {
                TryPlayNetworkSound(nseTitle, 1.1f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnThanks))
            {
                TryPlayNetworkSound(nseThanks, 2.75f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnHurt))
            {
                TryPlayNetworkSound(nseHurt, 0.1f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnTactical))
            {
                TryPlayNetworkSound(nseTacticalAction, 1.3f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnCommon))
            {
                TryPlayNetworkSound(nseCommonSkill, 1.9f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnEx1))
            {
                TryPlayNetworkSound(nseEx1, 1.7f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnEx2))
            {
                TryPlayNetworkSound(nseEx2, 2.1f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnEx1))
            {
                TryPlayNetworkSound(nseEx3, 2f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnExL1))
            {
                TryPlayNetworkSound(nseExL1, 1.6f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnExL2))
            {
                TryPlayNetworkSound(nseExL2, 2.2f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnExL3))
            {
                TryPlayNetworkSound(nseExL3, 2.9f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnExN1))
            {
                TryPlayNetworkSound(nseExN1, 1.25f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnExN2))
            {
                TryPlayNetworkSound(nseExN2, 1.7f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnExN1))
            {
                TryPlayNetworkSound(nseExN3, 1.3f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnExNL1))
            {
                TryPlayNetworkSound(nseExNL1, 2.2f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnExNL2))
            {
                TryPlayNetworkSound(nseExNL2, 2f, false);
                return;
            }

            if (BaseVoiceoverLib.Utils.GetKeyPressed(TrackMariCommandoVoiceoverPlugin.btnExNL3))
            {
                TryPlayNetworkSound(nseExNL3, 2.2f, false);
                return;
            }
        }

        public override bool ComponentEnableVoicelines()
        {
            return TrackMariCommandoVoiceoverPlugin.enableVoicelines.Value;
        }
    }
}
