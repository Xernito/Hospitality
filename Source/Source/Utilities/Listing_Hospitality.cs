﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Dmo;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using static UnityEngine.GraphicsBuffer;

namespace Hospitality
{
    [StaticConstructorOnStartup]
    public class Listing_Hospitality : Listing_Standard
    {

        public float CustomSliderLabel(string label, float val, float min, float max, float labelPct = 0.5f, string tooltip = null, string label2 = null, string rightLabel = null, string leftLabel = null, float roundTo = -1f)
        {
            Rect rect = base.GetRect(30f, 1f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect.LeftPart(labelPct), label);
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rect.LeftPart(labelPct), tooltip);
            }
            Text.Anchor = TextAnchor.UpperLeft;
            float result = CustomWidget.HorizontalSlider(rect.RightPart(1f - labelPct), val, min, max, true, label2, leftLabel, rightLabel, roundTo);
            base.Gap(this.verticalSpacing);
            return result;
        }
        public int CustomSliderLabelInt(string label, int val, int min, int max, float labelPct = 0.5f, string tooltip = null, string label2 = null, string rightLabel = null, string leftLabel = null, float roundTo = -1f)
        {
            Rect rect = base.GetRect(30f, 1f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rect.LeftPart(labelPct), label);
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rect.LeftPart(labelPct), tooltip);
            }
            Text.Anchor = TextAnchor.UpperLeft;
            float result = CustomWidget.HorizontalSlider(rect.RightPart(1f - labelPct), val, min, max, true, label2, leftLabel, rightLabel, roundTo);
            base.Gap(this.verticalSpacing);
            return (int)result;
        }
        public void CustomCheckboxLabeled(string label, ref bool checkOn, string tooltip = null, float height = 0f, float labelPct = 1f)
        {
            float height2 = (height != 0f) ? height : Text.CalcHeight(label, base.ColumnWidth * labelPct);
            Rect rect = base.GetRect(height2, labelPct);
            rect.width = Math.Min(rect.width + 24f, base.ColumnWidth);
            if (this.BoundingRectCached == null || rect.Overlaps(this.BoundingRectCached.Value))
            {
                if (!tooltip.NullOrEmpty())
                {
                    if (Mouse.IsOver(rect))
                    {
                        Widgets.DrawHighlight(rect);
                    }
                    TooltipHandler.TipRegion(rect, tooltip);
                }
                CustomWidget.CheckboxLabeled(rect, label, ref checkOn, false, null, null, false, false);
            }
            base.Gap(this.verticalSpacing);
        }
        public (int minValue, int maxValue) CustomSliderLabelIntRange(string label, int minValue, int maxValue, int min, int max, float labelPct = 0.5f, string tooltip = null, string label2 = null, string minLabel = null, string maxLabel = null, float roundTo = -1f)
        {
            TextAnchor originalAnchor = Text.Anchor;

            try
            {
                Rect rect = base.GetRect(30f, 1f);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect.LeftPart(labelPct), label);
                if (tooltip != null)
                {
                    TooltipHandler.TipRegion(rect.LeftPart(labelPct), tooltip);
                }
                Rect sliderRect = rect.RightPart(1f - labelPct);
                float floatMinValue = minValue;
                float floatMaxValue = maxValue;
                (floatMinValue, floatMaxValue) = CustomWidget.HorizontalRangeSlider(sliderRect, floatMinValue, floatMaxValue, min, max, true, label2, minLabel, maxLabel);
                minValue = Mathf.RoundToInt(floatMinValue);
                maxValue = Mathf.RoundToInt(floatMaxValue);
                return (minValue, maxValue);
            }
            finally
            {
                // Ensure the alignment is always reset, even if an exception occurs
                Text.Anchor = originalAnchor;
            }
        }
    }
}
