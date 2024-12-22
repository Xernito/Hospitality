using System.Linq;
using System.Collections.Generic;
using Hospitality.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using FoodUtility = RimWorld.FoodUtility;
using GuestUtility = Hospitality.Utilities.GuestUtility;

namespace Hospitality;

public class JobGiver_ScroungeFood : ThinkNode_JobGiver
{
    public override float GetPriority(Pawn pawn)
    {
        if (!pawn.IsArrivedGuest(out _)) return 0;

        var need = pawn.needs.food;
        if (need == null) return 0;

        if (pawn.needs.food.CurCategory < HungerCategory.Starving && FoodUtility.ShouldBeFedBySomeone(pawn)) return 0;

        var requiresFoodFactor = GuestUtility.GetRequiresFoodFactor(pawn);
        return requiresFoodFactor * 6;
    }

    public override Job TryGiveJob(Pawn guest)
    {
        if (guest.needs.food == null) return null;

        var guestComp = guest.CompGuest();
        if (guestComp == null) return null;

        if (GenTicks.TicksGame < guestComp.lastFoodCheckTick) return null;
        guestComp.lastFoodCheckTick = GenTicks.TicksGame + 500; // Recheck ever x ticks

        var canManipulateTools = guest.RaceProps.ToolUser && guest.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
        var food = canManipulateTools ? BestFoodInInventory(guest, guest, out var dummy) : null;
        if (food != null) return null;

        var pressure = guest.needs.food.CurCategory switch
        {
            HungerCategory.Fed => -25,
            HungerCategory.Hungry => 25,
            HungerCategory.UrgentlyHungry => 50,
            HungerCategory.Starving => 75,
            _ => 0
        };

        var maxStealOpinion = -pressure;
        if (guest.story.traits.HasTrait(TraitDefOf.Greedy)) maxStealOpinion += 25;
        if (guest.story.traits.HasTrait(TraitDefOf.Jealous)) maxStealOpinion += 25;
        if (guest.story.traits.HasTrait(TraitDefOf.Kind)) maxStealOpinion -= 100;
        if (guest.story.traits.HasTrait(TraitDefOf.Wimp)) maxStealOpinion -= 50;
        var target = FindTarget(guest, maxStealOpinion);
        var swipe = target?.Awake() == false;
        //Log.Message($"{guest.LabelCap} tried to find scroungable food. Found {target?.Label}. pressure = {pressure} maxStealOpinion = {maxStealOpinion} swipe = {swipe}");
        if (target == null) return null;

        food = BestFoodInInventory(target, guest, out var mood);
        if (food == null) return null;
        var amount = GetAmount(food);
        return new Job(swipe ? InternalDefOf.SwipeFood : InternalDefOf.ScroungeFood, target, food) { overeat = swipe, count = amount }; // overeat stores swiping
    }

    private static int GetAmount(Thing thing)
    {
        if (thing == null || thing.stackCount < 1) return 0;
        return Mathf.Max(thing.stackCount / 2, 1);
    }

    private static Pawn FindTarget(Pawn guest, int maxStealOpinion)
    {
        var lord = guest.GetLord();
        var targetPawn = TryFindGroupPawn(guest, maxStealOpinion, lord);
        if (targetPawn != null) return targetPawn;
        targetPawn = guest.MapHeld.lordManager.lords.Where(l => l != lord).Select(l => TryFindGroupPawn(guest, maxStealOpinion, lord)).FirstOrDefault();
        if (targetPawn != null) return targetPawn;
        targetPawn = TryFindPawn(guest, maxStealOpinion, guest.MapHeld.mapPawns.pawnsSpawned);
        return targetPawn;
    }

    private static Pawn TryFindGroupPawn(Pawn guest, int maxStealOpinion, Lord lord)
    {
        return TryFindPawn(guest, maxStealOpinion, lord.ownedPawns);
    }

    private static Pawn TryFindPawn(Pawn guest, int maxStealOpinion, IEnumerable<Pawn> pawns)
    {
        Pawn bestPawn = null;
        foreach (Pawn p in pawns)
        {
            int factor = QualifiesFactor(p, guest, maxStealOpinion);
            if (factor == 0)
                continue;
            if (factor == 2)
                return p;
            bestPawn = p;
        }
        return bestPawn;
    }

    // 0 - no, 1 - usable but bad food, 2 - usable
    private static int QualifiesFactor(Pawn target, Pawn guest, int maxStealOpinion)
    {
        if (target == null || guest == null) return 0;
        if (target == guest) return 0;
        if (target.inventory == null) return 0;
        if (target.relations == null) return 0;
        if (target.InAggroMentalState) return 0;
        if (target.HostileTo(guest)) return 0;

        var awake = target.Awake();
        if (guest.relations != null)
        {
            if (!awake && guest.relations.OpinionOf(target) > maxStealOpinion) return 0;
        }

        if (target.relations != null)
        {
            var minAwakeOpinion = 0;
            if (target.story?.traits != null)
            {
                if (target.story.traits.HasTrait(TraitDefOf.Kind)) minAwakeOpinion -= 35;
                if (target.story.traits.HasTrait(TraitDefOf.Greedy)) minAwakeOpinion += 50;
            }

            if (awake && target.relations.OpinionOf(guest) < minAwakeOpinion) return 0;
        }

        var food = BestFoodInInventory(target, guest, out var mood);
        if (food != null)
            return mood >= 0 ? 2 : 1;
        return 0;
    }

    internal static Thing BestFoodInInventory(Pawn holder, Pawn eater, out float mood)
    {
        mood = -100;
        if (holder.inventory == null) return null;

        var innerContainer = holder.inventory.innerContainer;
        Thing usableThing = null;
        for (var i = 0; i < innerContainer.Count; i++)
        {
            var thing = innerContainer.innerList[i];
            if (thing.def.IsNutritionGivingIngestible && thing.IngestibleNow && eater.WillEat(thing, eater)
                && (int)thing.def.ingestible.preferability >= (int)FoodPreferability.RawBad && !thing.def.IsDrug)
            {
                float thingMood = FoodUtility.MoodFromIngesting(eater, thing, thing.def);
                if (thingMood >= 0)
                {
                    mood = thingMood;
                    return thing;
                }
                if (thingMood > mood)
                {
                    mood = thingMood;
                    usableThing = thing;
                }
            }
        }

        return usableThing;
    }
}