using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public abstract class GeneGizmo_Resource : Gizmo
	{
		protected Gene_Resource gene;

		protected List<IGeneResourceDrain> drainGenes;

		private Texture2D barTex;

		private Texture2D barHighlightTex;

		protected Rect barRect;

		protected bool draggableBar;

		private static bool draggingBar;

		private float targetValuePct;

		private const float Spacing = 8f;

		private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(GenUI.FillableBar_Empty);

		private static readonly Texture2D ResourceTargetTex = SolidColorMaterials.NewSolidColorTexture(Color.white);

		protected virtual float Width => 160f;

		public GeneGizmo_Resource(Gene_Resource gene, List<IGeneResourceDrain> drainGenes, Color barColor, Color barHighlightColor)
		{
			this.gene = gene;
			this.drainGenes = drainGenes;
			barTex = SolidColorMaterials.NewSolidColorTexture(barColor);
			barHighlightTex = SolidColorMaterials.NewSolidColorTexture(barHighlightColor);
			Order = -100f;
			targetValuePct = Mathf.Clamp(gene.targetValue / gene.Max, 0f, gene.Max - gene.MaxLevelOffset);
		}

		public override float GetWidth(float maxWidth)
		{
			return Width;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(8f);
			Widgets.DrawWindowBackground(rect);
			Text.Font = GameFont.Small;
			Rect labelRect = rect2;
			labelRect.height = Text.LineHeight;
			bool mouseOverAnyHighlightableElement = false;
			DrawLabel(labelRect, ref mouseOverAnyHighlightableElement);
			rect2.yMin += labelRect.height + 8f;
			barRect = rect2;
			if (!draggableBar || (!gene.pawn.IsColonistPlayerControlled && !gene.pawn.IsPrisonerOfColony))
			{
				Widgets.FillableBar(barRect, gene.ValuePercent, barTex, EmptyBarTex, doBorder: true);
				if (!gene.def.resourceGizmoThresholds.NullOrEmpty())
				{
					for (int i = 0; i < gene.def.resourceGizmoThresholds.Count; i++)
					{
						float num = gene.def.resourceGizmoThresholds[i];
						Rect position = default(Rect);
						position.x = barRect.x + 3f + (barRect.width - 8f) * num;
						position.y = barRect.y + barRect.height - 9f;
						position.width = 2f;
						position.height = 6f;
						GUI.DrawTexture(position, (gene.Value < num) ? BaseContent.GreyTex : BaseContent.BlackTex);
					}
				}
			}
			else
			{
				Widgets.DraggableBar(barRect, barTex, barHighlightTex, EmptyBarTex, ResourceTargetTex, ref draggingBar, gene.ValuePercent, ref targetValuePct, gene.def.resourceGizmoThresholds, gene.MaxForDisplay / 10);
				targetValuePct = Mathf.Clamp(targetValuePct, 0f, (gene.Max - gene.MaxLevelOffset) / gene.Max);
				gene.SetTargetValuePct(targetValuePct);
			}
			int valueForDisplay = gene.ValueForDisplay;
			string label = string.Concat(arg2: gene.MaxForDisplay, arg0: valueForDisplay, arg1: " / ");
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(barRect, label);
			Text.Anchor = TextAnchor.UpperLeft;
			if (Mouse.IsOver(rect) && !mouseOverAnyHighlightableElement)
			{
				Widgets.DrawHighlight(rect);
				string tip = GetTooltip();
				if (!tip.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect, () => tip, Gen.HashCombineInt(gene.GetHashCode(), 17626387));
				}
			}
			return new GizmoResult(GizmoState.Clear);
		}

		protected virtual void DrawLabel(Rect labelRect, ref bool mouseOverAnyHighlightableElement)
		{
			string text = gene.ResourceLabel.CapitalizeFirst();
			if (Find.Selector.SelectedPawns.Count != 1)
			{
				text = text + " (" + gene.pawn.LabelShort + ")";
			}
			text = text.Truncate(labelRect.width);
			Widgets.Label(labelRect, text);
		}

		protected abstract string GetTooltip();
	}
}
