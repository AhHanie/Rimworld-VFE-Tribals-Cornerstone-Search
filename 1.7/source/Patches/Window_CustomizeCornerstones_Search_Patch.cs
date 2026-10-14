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
    public static class Window_CustomizeCornerstones_Search_Patch
    {
        private const float RowHeight = 24f;
        private const float RightMargin = 15f;
        private const float SearchWidth = 260f;
        private const float LabelWidth = 55f;
        private const float LabelGap = 4f;
        private const float ClearButtonWidth = 24f;
        private const float Gap = 6f;

        private static string _query = string.Empty;
        private static string _cachedQuery = null;
        private static readonly List<CornerstoneDef> CachedSource = new List<CornerstoneDef>();
        private static List<CornerstoneDef> _cachedResults = null;

        [HarmonyPrefix]
        [HarmonyInfix(typeof(Widgets), nameof(Widgets.BeginScrollView))]
        public static void DrawSearchBar(Rect outRect, ref Vector2 scrollPosition)
        {
            string previousQuery = _query;

            Rect searchRect = new Rect(
                outRect.xMax - RightMargin - SearchWidth,
                outRect.y - RowHeight,
                SearchWidth,
                RowHeight);
            Rect textRect = new Rect(searchRect.x, searchRect.y, searchRect.width - ClearButtonWidth - Gap, searchRect.height);
            Rect labelRect = new Rect(textRect.x - LabelGap - LabelWidth, searchRect.y, LabelWidth, searchRect.height);
            Rect clearRect = new Rect(textRect.xMax + Gap, searchRect.y, ClearButtonWidth, searchRect.height);

            GameFont previousFont = Text.Font;
            TextAnchor previousAnchor = Text.Anchor;

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(labelRect, "VFETribalsSearch.SearchCornerstones".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            _query = Widgets.TextField(textRect, _query);

            if (!string.IsNullOrEmpty(_query))
            {
                if (Widgets.ButtonText(clearRect, "X"))
                {
                    _query = string.Empty;
                    UI.UnfocusCurrentControl();
                }
                TooltipHandler.TipRegion(clearRect, "VFETribalsSearch.ClearSearch".Translate());
            }

            Text.Font = previousFont;
            Text.Anchor = previousAnchor;

            if (!string.Equals(previousQuery, _query, StringComparison.Ordinal))
            {
                scrollPosition = Vector2.zero;
            }
        }

        [HarmonyPostfix]
        [HarmonyInfix(typeof(Window_CustomizeCornerstones), "cornerstoneDefs", InnerTargetKind.FieldRead)]
        public static List<CornerstoneDef> FilterCornerstones(List<CornerstoneDef> __result)
        {
            if (__result == null || string.IsNullOrWhiteSpace(_query))
            {
                _cachedQuery = null;
                _cachedResults = null;
                CachedSource.Clear();
                return __result;
            }

            string query = _query.Trim();
            if (_cachedResults == null
                || !string.Equals(query, _cachedQuery, StringComparison.Ordinal)
                || !CachedSource.SequenceEqual(__result))
            {
                _cachedQuery = query;
                CachedSource.Clear();
                CachedSource.AddRange(__result);
                _cachedResults = __result.Where(def => Matches(def, query)).ToList();
            }

            return _cachedResults;
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
                && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
