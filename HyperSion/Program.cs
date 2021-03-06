﻿#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace HyperSion
{
    internal class Program
    {
        public const string ChampionName = "Sion";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q, W, E, R; 

        private static SpellSlot IgniteSlot, ExhaustSlot;


        public static Menu Config;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 580f);
            W + new Spell(SpellSlot.W, 500f);
            E = new Spell(SpellSlot.E, 800f); 
            R = new Spell(SpellSlot.R, 7000f);

            IgniteSlot = Player.GetSpellSlot("SummonerDot"), ExhaustSlot = Player.GetSpellSlot("SummonerExhaust");

            E.SetSkillshot(0.25f, 70f, 200, true, SkillshotType.SkillshotLine), Q.SetCharged("ScionQ", "ScionQ", 580, 580, 1.2f);

            SpellList.AddRange(new[] { Q, W, E, R });

            Config = new Menu("Hyper" + ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));                      
            Config.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));            

            Config.AddSubMenu(new Menu("Harass", "Harass"));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseQHarass", "Use Q").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseWHarass", "Use W").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("UseEHarass", "Use E").SetValue(true));
            Config.SubMenu("Harass").AddItem(new MenuItem("ManaHarass", "Dont Harass if mana < %").SetValue(new Slider(40, 100, 0)));
            Config.SubMenu("Harass").AddItem(new MenuItem("harassToggle", "Use Harass (toggle)").SetValue<KeyBind>(new KeyBind('T', KeyBindType.Toggle)));
            Config.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));

            Config.AddSubMenu(new Menu("Misc", "Misc"));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoI", "Auto Ignite").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoEx", "Auto Exhaust").SetValue(true));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoUnderT", "Combo Under MyTower").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("gapClose", "Q on Gapclosers").SetValue(false));            

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(false, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(true, Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            

            Game.PrintChat("<font color=\"#00BFFF\">Fed" + ChampionName + " -</font> <font color=\"#FFFFFF\">Loaded!</font>");

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var spell in SpellList)
            {
                var menuItem = Config.Item(spell.Slot + "Range").GetValue<Circle>();
                if (menuItem.Active)
                {
                    Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
                }
            }
        }
        
        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return;

            if (Config.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                if (Config.Item("HarassActive").GetValue<KeyBind>().Active)
                    Harass();                

                if (Config.Item("harassToggle").GetValue<KeyBind>().Active)
                    ToggleHarass();

                if (Config.Item("AutoUnderT").GetValue<bool>())
                    AutoUnderTower();                

                if (Config.Item("AutoI").GetValue<bool>())
                    AutoIgnite();

                if (Config.Item("AutoEx").GetValue<bool>())
                    AutoExhaust();
            }
        }

        private static void AutoIgnite()
        {
            var iTarget = SimpleTs.GetTarget(600, SimpleTs.DamageType.True);
            var Idamage = ObjectManager.Player.GetSummonerSpellDamage(iTarget, Damage.SummonerSpell.Ignite) * 0.9;

            if (IgniteSlot == SpellSlot.Unknown || Player.SummonerSpellbook.CanUseSpell(IgniteSlot) != SpellState.Ready ||
                !(iTarget.Health < Idamage)) return;
            Player.SummonerSpellbook.CastSpell(IgniteSlot, iTarget);
        }

        private static void AutoExhaust()
        {
            var iTarget = SimpleTs.GetTarget(600, SimpleTs.DamageType.True);

            if (ExhaustSlot == SpellSlot.Unknown || Player.SummonerSpellbook.CanUseSpell(ExhaustSlot) != SpellState.Ready ||
                !(iTarget.Health > (Q.GetDamage(iTarget, 0) * 4))) return;

            Player.SummonerSpellbook.CastSpell(ExhaustSlot, iTarget);
        }



        private static void AutoUnderTower()
        {
            var wTarget = SimpleTs.GetTarget(W.Range + W.Width, SimpleTs.DamageType.Magical);

            if (Utility.UnderTurret(wTarget, false) && Q.IsReady())
            {
                Q.Cast(wTarget);
            }
        }

        private static void Combo()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Physical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);
            var wTarget = SimpleTs.GetTarget(W.Range, SimpleTs.DamageType.Physical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Physical);
                        
            if (Config.Item("UseWCombo").GetValue<bool>() && (W.IsReady() && wTarget != null || qTarget != null && W.IsReady() && Q.IsReady()))
            {
                W.Cast(Player.Position);
            }
            if (qTarget != null && Config.Item("UseQCombo").GetValue<bool>() && Q.IsReady())
            {
                Q.Cast(qTarget);
            }
            if (rTarget != null && Config.Item("UseECombo").GetValue<bool>() && R.IsReady())
            {
                R.Cast(rTarget);
            }

        }        

        private static void Harass()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);

            if (eTarget != null && Config.Item("UseWHarass").GetValue<bool>() && W.IsReady() && E.IsReady())
            {                
                W.Cast();
            }
            if (qTarget != null && Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady())
            {
                Q.Cast();
            }
            if (qTarget != null && Config.Item("UseRHarass").GetValue<bool>() && R.IsReady())
            {
                R.Cast();
            }
        }

        private static void ToggleHarass()
        {
            var qTarget = SimpleTs.GetTarget(Q.Range + Q.Width, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range + E.Width, SimpleTs.DamageType.Magical);

            if (qTarget != null && Config.Item("UseQHarass").GetValue<bool>() && Q.IsReady())
            {
                W.Cast();
            }
            if (eTarget != null && Config.Item("UseEHarass").GetValue<bool>() && E.IsReady())
            {
                Q.Cast(eTarget);
            }
            if (qTarget != null && Config.Item("UseRHarass").GetValue<bool>() && R.IsReady())
            {
                R.Cast();
            }
        }        

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!Config.Item("gapClose").GetValue<bool>()) return;

            if (!gapcloser.Sender.IsValidTarget(400f))
            {
                return;
            }
            if (gapcloser.Sender.IsValidTarget(300))
            {
                Q.Cast(gapcloser.Sender);
            }
            
            W.Cast();
        }
    }
}
