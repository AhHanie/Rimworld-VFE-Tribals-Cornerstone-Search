using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using VFETribals;

namespace VFETribalsSearch.Patches
{
    [HarmonyPatch(typeof(Window_CustomizeCornerstones), nameof(Window_CustomizeCornerstones.DoWindowContents))]
    public static class Window_CustomizeCornerstones_DoWindowContents_Patch
    {
        private static readonly AccessTools.FieldRef<Window_CustomizeCornerstones, List<CornerstoneDef>> CornerstoneDefs =
            AccessTools.FieldRefAccess<Window_CustomizeCornerstones, List<CornerstoneDef>>("cornerstoneDefs");

        private static readonly AccessTools.FieldRef<Window_CustomizeCornerstones, Vector2> ScrollPos =
            AccessTools.FieldRefAccess<Window_CustomizeCornerstones, Vector2>("scrollPos");

        private static readonly System.Reflection.MethodInfo FillCornerstoneDefsMethod =
            AccessTools.Method(typeof(Window_CustomizeCornerstones), "FillCornerstoneDefs");

        private static string _query = string.Empty;
        private static string _cachedQuery = null;
        private static List<CornerstoneDef> _cachedResults = null;

        private static void Prefix(Window_CustomizeCornerstones __instance, Rect inRect, ref List<CornerstoneDef> __state)
        {
            __state = CornerstoneDefs(__instance);

            string previousQuery = _query;
            DrawSearchBar(__instance, inRect);

            if (!string.Equals(previousQuery, _query, StringComparison.Ordinal))
            {
                ScrollPos(__instance) = Vector2.zero;
            }

            if (!string.IsNullOrWhiteSpace(_query) && __state != null)
            {
                if (!string.Equals(_query, _cachedQuery, StringComparison.Ordinal))
                {
                    _cachedQuery = _query;
                    _cachedResults = __state.Where(def => Matches(def, _query)).ToList();
                }
                CornerstoneDefs(__instance) = _cachedResults;
            }
            else
            {
                _cachedQuery = null;
                _cachedResults = null;
            }
        }

        private static void Postfix(Window_CustomizeCornerstones __instance, List<CornerstoneDef> __state)
        {
            RestoreOriginalList(__instance, __state, refill: true);
        }

        private static void RestoreOriginalList(Window_CustomizeCornerstones instance, List<CornerstoneDef> originalDefs, bool refill)
        {
            if (originalDefs == null)
            {
                return;
            }

            CornerstoneDefs(instance) = originalDefs;

            if (refill)
            {
                FillCornerstoneDefsMethod?.Invoke(instance, null);
            }
        }

        private static void DrawSearchBar(Window_CustomizeCornerstones instance, Rect inRect)
        {
            const float leftMargin = 15f;
            const float headerY = 190f;
            const float height = 24f;
            const float searchWidth = 260f;
            const float clearButtonWidth = 24f;
            const float gap = 6f;

            var searchRect = new Rect(
                inRect.width - leftMargin - searchWidth,
                headerY,
                searchWidth,
                height);

            const float labelWidth = 55f;
            const float labelGap = 4f;

            var textRect = new Rect(searchRect.x, searchRect.y, searchRect.width - clearButtonWidth - gap, searchRect.height);

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleRight;
            var labelRect = new Rect(textRect.x - labelGap - labelWidth, searchRect.y, labelWidth, searchRect.height);
            Widgets.Label(labelRect, "VFETribalsSearch.SearchCornerstones".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            _query = Widgets.TextField(textRect, _query);

            var clearRect = new Rect(textRect.xMax + gap, searchRect.y, clearButtonWidth, searchRect.height);
            if (!string.IsNullOrEmpty(_query))
            {
                if (Widgets.ButtonText(clearRect, "X"))
                {
                    _query = string.Empty;
                    GUI.FocusControl(string.Empty);
                }
                TooltipHandler.TipRegion(clearRect, "VFETribalsSearch.ClearSearch".Translate());
            }
        }

        private static bool Matches(CornerstoneDef def, string query)
        {
            return Contains(def.label, query)
                || Contains(def.LabelCap.RawText, query)
                || Contains(def.description, query)
                || Contains(def.blurb, query)
                || Contains(def.defName, query);
        }

        private static bool Contains(string value, string query)
        {
            return !string.IsNullOrEmpty(value)
                && !string.IsNullOrWhiteSpace(query)
                && value.IndexOf(query.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
