using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hospitality.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using System.Reflection;
using GuestUtility = Hospitality.Utilities.GuestUtility;

namespace Hospitality
{
    public class Mod_Hospitality : Mod
    {
        public static ModSettings_Hospitality settings;
        //private static readonly List<Action> tickActions = new();
        public Mod_Hospitality(ModContentPack content) : base(content)
        {
            settings = GetSettings<ModSettings_Hospitality>();
            Harmony harmony = new (this.Content.PackageIdPlayerFacing);
            harmony.PatchAll();
            PostLoad();

        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
        }
        public override string SettingsCategory()
        {
            return "Hospitality";
        }
        public override void WriteSettings()
        {
            base.WriteSettings();
        }

        public static void PostLoad()
        {
            LongEventHandler.ExecuteWhenFinished(() => DefsUtility.CheckForInvalidDefs());
        }

        //public override void Tick(int currentTick)
        //{
        //    foreach (var action in tickActions)
        //    {
        //        action();
        //    }
        //    tickActions.Clear();
        //}

        //public static void RegisterTickAction(Action action)
        //{
        //    tickActions.Add(action);
        //}

        public static void SettingsChanged()
        {
            ToggleTabIfNeeded();
            UpdateMainButtonIcon();
            
        }

        public static void ToggleTabIfNeeded()
        {
            DefDatabase<MainButtonDef>.GetNamed("Guests").buttonVisible = !ModSettings_Hospitality.disableGuestsTab;
        }

        public static void UpdateMainButtonIcon()
        {
            var mainButtonDef = DefDatabase<MainButtonDef>.GetNamed("Guests");
            mainButtonDef.iconPath = ModSettings_Hospitality.useIcon ? "UI/Buttons/MainButtons/IconHospitality" : null;
            if (mainButtonDef.iconPath == null) mainButtonDef.icon = null;
        }
    }
}
