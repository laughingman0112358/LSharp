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
using Color = System.Drawing.Color;

namespace LaughingYasuo
{
    internal class Program
    {
        private const string ChampionName = "Yasuo";

        public static Orbwalking.Orbwalker Orbwalker;

        private static List<Spell> SpellList = new List<Spell>();

        public static Spell E;

 //Reinitialize Later
        //public static Spell Q, W, R;

 //Future Use
        //private static SpellSlot igniteSlot, smiteSlot, exhaustSlot, barrierSlot, flashSlot;

//Use to see whether Yasuo's Whirlwind is active or not
        //public static bool WWActive = false;  

        public static Menu YasuoMenu;

        private static Obj_AI_Hero Player;

        private static void Main(string[] args)
        {
                CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }


        private static void Game_OnGameLoad(EventArgs args)
        {
                Player = ObjectManager.Player;

                //if (Player.BaseSkinName != ChampionName) return;

//Reinitialize Later
                //Q = new Spell(SpellSlot.Q, 475f);
                //W = new Spell(SpellSlot.W, 400f);
                E = new Spell(SpellSlot.E, 475f);
                //R = new Spell(SpellSlot.R, 1200f);

 //Future Uses  
            //igniteSlot = Player.GetSpellSlot("SummonerDot"); //smiteSlot = Player.GetSpellSlot("SummonerSmite"); //exhaustSlot = Player.GetSpellSlot("SummonerExhaust"); //barrierSlot = Player.GetSpellSlot("SummonerShield"); //flashSlot = Player.GetSpellSlot("SummonerFlash");

//ReInitialize Later
                //Q.SetSkillshot(0.25f, 75f, 1500f, false, SkillshotType.SkillshotLine);
                //W.SetSkillshot(0.25f, 300f, 750f, false, SkillshotType.SkillshotCircle);
                E.SetTargetted(0.25f, 1500f);
                //R.SetTargetted(0.25f, 1500f);
//
                SpellList.AddRange(new[] { /*Q, W,*/ E/*, R */});  //Reinitialize as I add spells

                YasuoMenu = new Menu("Laughing" + ChampionName, ChampionName, true);

//Who needs target selection anyways?? It does work however for testing I dont need it to interfere.
                //var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
                //SimpleTs.AddToMenu(targetSelectorMenu);
                //YasuoMenu.AddSubMenu(targetSelectorMenu);

                YasuoMenu.AddSubMenu(new Menu("OrbWalking", "Orbwalking"));
                Orbwalker = new Orbwalking.Orbwalker(YasuoMenu.SubMenu("Orbwalking"));

                YasuoMenu.AddSubMenu(new Menu("Combo", "Combo"));
                //Config.SubMenu("Combo").AddItem(new MenuItem("UseQCombo", "Use Q").SetValue(true));
                //Config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));
                YasuoMenu.SubMenu("Combo").AddItem(new MenuItem("UseECombo", "Use E").SetValue(false));
                //Config.SubMenu("Combo").AddItem(new MenuItem("UseRCombo", "Use R").SetValue(true));

                //Config.AddSubMenu(new Menu("Farm", "Farm"));
                //Config.SubMenu("Farm").AddItem(new MenuItem("UseE-QFarm", "Use Q Farm").SetValue(true));
                //Config.SubMenu("Farm").AddItem(new MenuItem("FreezeActive", "Freeze!").SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                //Config.SubMenu("Farm").AddItem(new MenuItem("LaneClearActive", "LaneClear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

                //Config.AddSubMenu(new Menu("JungleFarm", "JungleFarm"));
                //Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseQJFarm", "Use Q").SetValue(true));
                //Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseWJFarm", "Use W").SetValue(true));
                //Config.SubMenu("JungleFarm").AddItem(new MenuItem("UseEJFarm", "Use E").SetValue(true));
                //Config.SubMenu("JungleFarm").AddItem(new MenuItem("AutoSmite", "Auto Smite!").SetValue<KeyBind>(new KeyBind('J', KeyBindType.Toggle)));
                //Config.SubMenu("JungleFarm").AddItem(new MenuItem("JungleFarmActive", "JungleFarm!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));

                //Config.AddSubMenu(new Menu("Misc", "Misc"));
                //Config.SubMenu("Misc").AddItem(new MenuItem("AutoI", "Auto Ignite").SetValue(true));          
                // Re-Add Later            Config.SubMenu("Misc").AddItem(new MenuItem("UseItems", "Use Items").SetValue(true));

//Reinitialize for Drawings
                //YasuoMenu.AddSubMenu(new Menu("Drawings", "Drawings"));
                //YasuoMenu.SubMenu("Drawings").AddItem(new MenuItem("QRange", "Q range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                //YasuoMenu.SubMenu("Drawings").AddItem(new MenuItem("WRange", "W range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                //YasuoMenu.SubMenu("Drawings").AddItem(new MenuItem("ERange", "E range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));
                //YasuoMenu.SubMenu("Drawings").AddItem(new MenuItem("RRange", "R range").SetValue(new Circle(true, System.Drawing.Color.FromArgb(255, 255, 255, 255))));

                YasuoMenu.AddToMainMenu();

                Game.PrintChat("<font color=\"#00BFFF\">Laughing " + ChampionName + " -</font> <font color=\"#FFFFFF\">Loaded!</font>"); 

                Game.OnGameUpdate += Game_OnGameUpdate;
//Reinitialize for Drawings
                //Drawing.OnDraw += Drawing_OnDraw;
//Event for Game ending
                //CustomEvents.Game.OnGameEnd += Game_OnGameEnd; 

                   
        }

        /// <summary>
        /// As of now, Drawing is WORKING - 10/11
        /// </summary>
        //private static void Drawing_OnDraw(EventArgs args)
        //{
        //    foreach (var spell in SpellList)
        //    {
        //        var menuItem = YasuoMenu.Item(spell.Slot + "Range").GetValue<Circle>();
        //        if (menuItem.Active)
        //        {
        //            Utility.DrawCircle(Player.Position, spell.Range, menuItem.Color);
        //        }
        //    }
        //}

        private static void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead) return; // will not do anything else in this function if your champion, Player, Is Dead. 

            //if (Player.HasBuff("Tempest"))
            //{
            //    WWActive = true;
            //}

            if (YasuoMenu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                var qTarget = SimpleTs.GetTarget(E.Range, SimpleTs.DamageType.Physical);

                if (qTarget != null && YasuoMenu.Item("UseECombo").GetValue<bool>() && E.IsReady() &&
                    Player.Distance(qTarget) <= 400f)
                    {
                        E.Cast(qTarget);
                    }
            }

            //else
            //{
            //    if (YasuoMenu.Item("LaneClearActive").GetValue<bool>())
            //    {
            //        LaneClear();
            //    }
            //    //if (Config.Item("Harass").GetValue<bool>())
            //    //{
            //    //    Harass();
            //    //}
            //}

        }

        //private static void Harass()
        //{
           
        //}

        //private static void Combo()
        //{
            //var qTarget = SimpleTs.GetTarget(E.Range + E.Width, SimpleTs.DamageType.Physical);

            //var qMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All,
            //    MinionTeam.NotAlly);

            //if (qTarget != null && Config.Item("UseItems").GetValue<bool>())
            //{
            //    UseItems(qTarget);
            //}

            //if (!WWActive)
            //{
            //    if (qTarget != null && YasuoMenu.Item("UseQCombo").GetValue<bool>() && Q.IsReady() && Player.Distance(qTarget) <= qRange)
            //    {
            //        PredictionOutput qPred = Q.GetPrediction(qTarget);
            //        if (qPred.Hitchance >= HitChance.High)
            //            Q.Cast(qPred.CastPosition);
            //    }

            //    if (qTarget != null && YasuoMenu.Item("UseQCombo").GetValue<bool>() && Q.IsReady() &&
            //        Player.Distance(qTarget) > (qRange*2))
            //    {
            //        foreach (var nminion in qMinions)
            //        {
            //            Q.Cast(nminion);
            //        }
            //    }

            //}

            //if (qTarget != null && YasuoMenu.Item("UseECombo").GetValue<bool>() && E.IsReady() &&
            //    Player.Distance(qTarget) <= 400f)
            //{
            //    E.Cast(qTarget);
            //}

            //if (qTarget != null && YasuoMenu.Item("UseRCombo").GetValue<bool>() && R.IsReady() && qTarget.Health < 1000f)
            //{
            //    R.Cast(qTarget);
            //}

            //if (qTarget != null && YasuoMenu.Item("UseWCombo").GetValue<bool>() && W.IsReady() && qTarget.IsAutoAttacking &&
            //    !qTarget.IsMelee())
            //{
            //    W.Cast(Player.ServerPosition);
            //}

            //if (qTarget != null && YasuoMenu.Item("UseQCombo").GetValue<bool>() && Q.IsReady() && Player.Distance(qTarget) < Q.Range)
            //{
            //    Q.Cast(qTarget);
            //}

        //}

        //private static void LaneClear()
        //{
        //    var lMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range + Q.Width, MinionTypes.All,
        //        MinionTeam.NotAlly);

        //    if (lMinions != null && YasuoMenu.Item("UseE-QFarm").GetValue<bool>())
        //    {
        //        foreach (var tMinion in lMinions)
        //        {
        //            E.Cast(tMinion);
        //            if (!WWActive)
        //            {
        //                Q.Cast(tMinion);
        //            }
        //        }
        //    }
        //}
    }
}
