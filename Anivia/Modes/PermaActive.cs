﻿using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;

using Settings = Anivia.Config.Misc;

namespace Anivia.Modes
{
    public sealed class PermaActive : ModeBase
    {
        bool stackingTear = false;
        int currentSkin = 0;
        bool bought = false;
        int ticks = 0;
        public override bool ShouldBeExecuted()
        {
            // Since this is permaactive mode, always execute the loop
            return true;
        }

        public override void Execute()
        {
            autoBuyStartingItems();
            cleanseMe();
            stackTear();
            stopStackMode();
            skinChanger();
        }

        private void autoBuyStartingItems()
        {
            
            if (bought || ticks / Game.TicksPerSecond < 5)
            {
                ticks++;
                return;
            }

            bought = true;
            if (Settings.autoBuyStartingItems)
            {
                if (Game.MapId == GameMapId.SummonersRift)
                {
                    Shop.BuyItem(ItemId.Dorans_Ring);
                    Shop.BuyItem(ItemId.Health_Potion);
                    Shop.BuyItem(ItemId.Health_Potion);
                    Shop.BuyItem(ItemId.Warding_Totem_Trinket);
                }
            }
        }

        private void cleanseMe()
        {
            if (Settings.cleanseStun && cleanse != null)
            {
                if(Player.HasBuff("PoppyDiplomaticImmunity") || Player.HasBuff("MordekaiserChildrenOfTheGrave") || Player.HasBuff("FizzMarinerDoom") || Player.HasBuff("VladimirHemoplague") || 
                        Player.HasBuff("zedulttargetmark") || Player.HasBuffOfType(BuffType.Suppression) || Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Flee) || Player.HasBuffOfType(BuffType.Blind) || 
                        Player.HasBuffOfType(BuffType.Polymorph) || Player.HasBuffOfType(BuffType.Snare) || Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Taunt))
                {
                    if (Player.Instance.CountEnemiesInRange(1000) >= Settings.cleanseEnemies)
                    {
                        if (cleanse.IsReady())
                        {
                            cleanse.Cast();
                        }
                    }
                }
            }
        }

        private void skinChanger()
        {
            if (Settings.skinId != currentSkin)
            {
                Player.Instance.SetSkinId(Settings.skinId);
                this.currentSkin = Settings.skinId;
            }
        }

        private void stopStackMode()
        {
            if (!Settings.tearStack || !this.stackingTear)
            {
                return;
            }
            if (!Player.Instance.IsInShopRange() && Player.Instance.Spellbook.GetSpell(SpellSlot.R).ToggleState == 2)
            {
                if (R.IsReady() && R.IsLearned)
                {
                    R.Cast(Player.Instance);
                    this.stackingTear = false;
                }
            }
        }

        private void stackTear()
        {
            if (Player.Instance.Spellbook.GetSpell(SpellSlot.R).ToggleState == 2)
                return;

            if (60 > Player.Instance.ManaPercent)
            {
                return;
            }
            if (!Settings.tearStack || this.stackingTear || !Player.Instance.IsInShopRange() || Game.MapId == GameMapId.HowlingAbyss)
            {
                return;
            }
            InventorySlot[] inv = Player.Instance.InventoryItems;
            foreach (var item in inv)
            {
                if (item.Id == ItemId.Archangels_Staff || item.Id == ItemId.Archangels_Staff_Crystal_Scar || item.Id == ItemId.Tear_of_the_Goddess || item.Id == ItemId.Tear_of_the_Goddess_Crystal_Scar)
                {
                    if (item.Charges < 700)
                    {
                        if (R.IsReady() && R.IsLearned)
                        {
                            R.Cast(Player.Instance);
                            this.stackingTear = true;
                        }
                    }
                }
            }

        }

        internal static void autoLevelSkills(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (Settings.autolevelskills)
            {
                if (!sender.IsMe || args.Level > 17)
                {
                    return;
                }
                int[] leveler = new int[] { 1, 3, 3, 2, 3, 4, 3, 1, 3, 1, 4, 1, 1, 2, 2, 4, 2, 2 };
                int skill = leveler[Player.Instance.Level];

                if (skill == 1)
                    Player.Instance.Spellbook.LevelSpell(SpellSlot.Q);
                else if (skill == 2)
                    Player.Instance.Spellbook.LevelSpell(SpellSlot.W);
                else if (skill == 3)
                    Player.Instance.Spellbook.LevelSpell(SpellSlot.E);
                else if (skill == 4)
                    Player.Instance.Spellbook.LevelSpell(SpellSlot.R);
                else
                    return;
            }
        }

        internal static void Dash_OnDash(Obj_AI_Base sender, Dash.DashEventArgs e)
        {
            Spell.Skillshot W = SpellManager.W;
            if (Settings.antiDash && W.IsReady() && sender.IsValid && sender.IsEnemy && !sender.IsDead && !sender.IsInvulnerable && !sender.IsZombie && sender.IsInRange(Player.Instance, W.Range))
            {
                if (Player.Instance.Distance(e.EndPos) < Player.Instance.Distance(e.StartPos))
                    W.Cast(sender);
                else if (Settings.antiDashOffensive)
                    W.Cast(sender);
            }
        }

        internal static void antiGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            Spell.Skillshot W = SpellManager.W;
            if (Settings.antiDash && W.IsReady() && sender.IsValid && sender.IsEnemy && !sender.IsDead && !sender.IsInvulnerable && !sender.IsZombie && e.End.IsInRange(Player.Instance, W.Range))
            {
                if (Player.Instance.Distance(e.End) < Player.Instance.Distance(e.End))
                    W.Cast(e.End);
                else if (Settings.antiDashOffensive)
                    W.Cast(e.End);
            }
        }
    }
}