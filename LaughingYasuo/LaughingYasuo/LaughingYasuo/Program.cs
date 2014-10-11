using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace LaughingYasuo
{
    internal class Program
    {
        public const string ChampionName = "Yasuo";

        public static Orbwalking.Orbwalker Orbwalker;

        public static List<Spell> SpellList = new List<Spell>();

        public static Spell Q, W, E, R;

        private static SpellSlot igniteSlot, smiteSlot, exhaustSlot, barrierSlot, flashSlot;

        public static bool WWActive = false;

        public static Menu Config;

        private static Obj_AI_Hero Player;

        static void Main(string[] args)
        {
            LeagueSharp.Game.PrintChat("Are you ready to be a Laughing " + ChampionName + "?");
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }


        private static void Game_OnGameLoad(EventArgs args)
        {
            Player = ObjectManager.Player;

            if (Player.BaseSkinName != ChampionName) return;

            Q = new Spell(SpellSlot.Q, 475);
            W = new Spell(SpellSlot.W, 400);
            E = new Spell(SpellSlot.E, 475);
            R = new Spell(SpellSlot.R, 1200);

            igniteSlot = Player.GetSpellSlot("SummonerDot");
            smiteSlot = Player.GetSpellSlot("SummonerSmite");
            exhaustSlot = Player.GetSpellSlot("SummonerExhaust");
            barrierSlot = Player.GetSpellSlot("SummonerShield");
            flashSlot = Player.GetSpellSlot("SummonerFlash");

            Q.SetSkillshot(0.25f, 75f, 1500f, false, SkillshotType.SkillshotLine);
            E.SetTargetted(0.25f, 1500f);
            R.SetTargetted(0.25f, 1500f);

            SpellList.AddRange(new[] { Q, W, E, R });

            Config = new Menu("Laughing" + ChampionName, ChampionName, true);

            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            SimpleTs.AddToMenu(targetSelectorMenu);
            Config.AddSubMenu(targetSelectorMenu);

            Config.AddSubMenu(new Menu("OrbWalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));

            Config.AddSubMenu(new Menu("Combo", "Combo"));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(true));
            Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));

            Config.AddSubMenu(new Menu("Farm", "Farm"));
            Config.SubMenu("Farm").AddItem(new MenuItem("UseE-QFarm", "Use Q Farm").SetValue(true));
            //Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
            Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Config.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
            //Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
            //Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
            //Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
            //Config.SubMenu("JungleFarm").AddItem(new MenuItem("AutoSmite", "Auto Smite!").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));
            //Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

            //Config.AddSubMenu(new Menu("Misc", "Misc"));
            //Config.SubMenu("Misc").AddItem(new MenuItem("AutoI", "Auto Ignite").SetValue(true));          
// Re-Add Later            Config.SubMenu("Misc").AddItem(new MenuItem("UseItems", "Use Items").SetValue(true));

            Config.AddSubMenu(new Menu("Drawings", "Drawings"));
            Config.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
            Config.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

            Config.AddToMainMenu();

            Game.OnGameUpdate += Game_OnGameUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            CustomEvents.Game.OnGameEnd += Game_OnGameEnd;

            Game.PrintChat("<font color=\"#00BFFF\">Laughing " + ChampionName + " -</font> <font color=\"#FFFFFF\">Loaded!</font>");

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
                if (Config.Item("LaneClearActive").GetValue<bool>())
                {
                    LaneClear();
                }
            }

        }

        private static void Combo()
        {
            int qRange = Config.Item("RangeQ").GetValue<Slider>().Value;

            var qTarget = SimpleTs.GetTarget(Q.Range + Q.Width, SimpleTs.DamageType.Physical);

            var qMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All,
                MinionTeam.NotAlly);

            //if (qTarget != null && Config.Item("UseItems").GetValue<bool>())
            //{
            //    UseItems(qTarget);
            //}

            if (!WWActive)
            {
                if (qTarget != null && Config.Item("UseQCombo").GetValue<bool>() && Q.IsReady() && Player.Distance(qTarget) <= qRange)
                {
                    PredictionOutput qPred = Q.GetPrediction(qTarget);
                    if (qPred.Hitchance >= HitChance.High)
                        Q.Cast(qPred.CastPosition);
                }

                if (qTarget != null && Config.Item("UseQCombo").GetValue<bool>() && Q.IsReady() &&
                    Player.Distance(qTarget) > (qRange*2))
                {
                    foreach (var nminion in qMinions)
                    {
                        Q.Cast(nminion);
                    }
                }

            }

            if (qTarget != null && Config.Item("UseECombo").GetValue<bool>() && E.IsReady() &&
                Player.Distance(qTarget) <= qRange)
            {
                E.Cast(qTarget);
            }

            if (qTarget != null && Config.Item("UseRCombo").GetValue<bool>() && R.IsReady() && qTarget.Health < 1000f)
            {
                R.Cast(qTarget);
            }

            if (qTarget != null && Config.Item("UseWCombo").GetValue<bool>() && W.IsReady() && qTarget.IsAutoAttacking &&
                !qTarget.IsMelee())
            {
                W.Cast(Player.ServerPosition);
            }

            if (qTarget != null && Config.Item("UseQCombo").GetValue<bool>() && Q.IsReady() && Player.Distance(qTarget) < Q.Range)
            {
                Q.Cast(qTarget);
            }

        }

        private static void LaneClear()
        {
            var lMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All,
                MinionTeam.NotAlly);

            if (lMinions != null && Config.Item("UseE-QFarm").GetValue<bool>())
            {
                foreach (var tMinion in lMinions)
                {
                    E.Cast(tMinion);
                    if (!WWActive)
                    {
                        Q.Cast(tMinion);
                    }
                }
            }
        }

        //public static void UseItems(Obj_AI_Hero vTarget)
        //{
        //    if (vTarget != null)
        //    {
        //        foreach (MenuItem menuItem in TargetedItems.Items)
        //        {
        //            var useItem = TargetedItems.Item(menuItem.Name).GetValue<bool>();
        //            if (useItem)
        //            {
        //                var itemID = Convert.ToInt16(menuItem.Name.ToString().Substring(4, 4));
        //                if (Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null)
        //                    Items.UseItem(itemID, vTarget);
        //            }
        //        }

        //        foreach (MenuItem menuItem in NoTargetedItems.Items)
        //        {
        //            var useItem = NoTargetedItems.Item(menuItem.Name).GetValue<bool>();
        //            if (useItem)
        //            {
        //                var itemID = Convert.ToInt16(menuItem.Name.ToString().Substring(4, 4));
        //                if (Items.HasItem(itemID) && Items.CanUseItem(itemID) && GetInventorySlot(itemID) != null)
        //                    Items.UseItem(itemID);
        //            }
        //        }
        //    }
        //}

        //private static InventorySlot GetInventorySlot(int ID)
        //{
        //    return ObjectManager.Player.InventoryItems.FirstOrDefault(item => (item.Id == (ItemId)ID && item.Stacks >= 1) || (item.Id == (ItemId)ID && item.Charges >= 1));
        //}


        private static void Game_OnGameEnd(EventArgs args)
        {
            Game.Say("/all GG");
            Game.Say("/dance");
        }

    }


    }
