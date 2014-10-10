#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

#endregion

namespace HyperTeemo
{
    internal class Program
    {
        public const string ChampionName = "Teemo";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        private static SpellSlot IgniteSlot;
        private static SpellSlot ExhaustSlot;

        public static Menu Config;

        private static Obj_AI_Hero Player;

        public static int LevelCompletedLast = 0;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 580);
            W = new Spell(SpellSlot.W, 275f);
            E = new Spell(SpellSlot.E, Player.AttackRange);
            R = new Spell(SpellSlot.R, 225f);

            IgniteSlot = Player.GetSpellSlot("SummonerDot");
            ExhaustSlot = Player.GetSpellSlot("SummonerExhaust");
            
            E.SetTargetted(0.25f, 85f);

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
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoR", "Auto R Constantly").SetValue(false));
            Config.SubMenu("Misc").AddItem(new MenuItem("AutoLevel", "Auto Level").SetValue(true));

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

                if (Config.Item("AutoR").GetValue<StringList>().SelectedIndex > 0)
                    AutoUlt();

                if (Config.Item("AutoLevel").GetValue<bool>())
                    AutoLevel();

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

        private static void AutoLevel()
        {
            if (LevelCompletedLast < Player.Level)
            {
                LevelCompletedLast = Player.Level;

                switch (Player.Level)
                {
                    case 1:
                        Player.Spellbook.LevelUpSpell(SpellSlot.E);
                        break;
                    case 2:
                        Player.Spellbook.LevelUpSpell(SpellSlot.Q);
                        break;
                    case 3:
                        Player.Spellbook.LevelUpSpell(SpellSlot.E);
                        break;
                    case 4:
                        Player.Spellbook.LevelUpSpell(SpellSlot.W);
                        break;
                    case 5:
                        Player.Spellbook.LevelUpSpell(SpellSlot.E);
                        break;
                    case 6:
                        Player.Spellbook.LevelUpSpell(SpellSlot.R);
                        break;
                    case 7:
                        Player.Spellbook.LevelUpSpell(SpellSlot.E);
                        break;
                    case 8:
                        Player.Spellbook.LevelUpSpell(SpellSlot.Q);
                        break;
                    case 9:
                        Player.Spellbook.LevelUpSpell(SpellSlot.E);
                        break;
                    case 10:
                        Player.Spellbook.LevelUpSpell(SpellSlot.Q);
                        break;
                    case 11:
                        Player.Spellbook.LevelUpSpell(SpellSlot.R);
                        break;
                    case 12:
                        Player.Spellbook.LevelUpSpell(SpellSlot.Q);
                        break;
                    case 13:
                        Player.Spellbook.LevelUpSpell(SpellSlot.Q);
                        break;
                    case 14:
                        Player.Spellbook.LevelUpSpell(SpellSlot.R);
                        break;
                    case 15:
                        Player.Spellbook.LevelUpSpell(SpellSlot.W);
                        break;
                    case 16:
                        Player.Spellbook.LevelUpSpell(SpellSlot.W);
                        break;
                }
            }
        }

        private static void AutoUlt()
        {
            if (R.IsReady() && Config.Item("AutoR").GetValue<bool>()) 
            {
                R.Cast(Player.Position);
            }
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
            var qTarget = SimpleTs.GetTarget(Q.Range, SimpleTs.DamageType.Magical);
            var eTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var wTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Magical);
            var rTarget = SimpleTs.GetTarget(R.Range, SimpleTs.DamageType.Magical);
                        
            if (Config.Item("UseWCombo").GetValue<bool>() && (W.IsReady() && wTarget != null || qTarget != null && W.IsReady() && Q.IsReady()))
            {
                W.Cast();
            }
            if (qTarget != null && Config.Item("UseQCombo").GetValue<bool>() && Q.IsReady())
            {
                Q.Cast(qTarget);
            }
            if (rTarget != null && Config.Item("UseRCombo").GetValue<bool>() && R.IsReady())
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
