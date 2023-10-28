using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MainTabWindow_Inspect : MainTabWindow, IInspectPane
	{
		private Type openTabType;

		private float recentHeight;

		private static IntVec3 lastSelectCell;

		private Gizmo mouseoverGizmo;

		private Gizmo lastMouseOverGizmo;

		private Thing lastSelectedThing;

		private Vector2 lastTabSize;

		private const float GuiltyTexSize = 26f;

		public Type OpenTabType
		{
			get
			{
				return openTabType;
			}
			set
			{
				openTabType = value;
			}
		}

		public float RecentHeight
		{
			get
			{
				return recentHeight;
			}
			set
			{
				recentHeight = value;
			}
		}

		protected override float Margin => 0f;

		public override Vector2 RequestedTabSize => InspectPaneUtility.PaneSizeFor(this);

		private List<object> Selected => Find.Selector.SelectedObjects;

		private Thing SelThing => Find.Selector.SingleSelectedThing;

		private Zone SelZone => Find.Selector.SelectedZone;

		private int NumSelected => Find.Selector.NumSelected;

		public float PaneTopY => (float)UI.screenHeight - 165f - 35f;

		public bool AnythingSelected => NumSelected > 0;

		public Gizmo LastMouseoverGizmo => lastMouseOverGizmo;

		public bool ShouldShowSelectNextInCellButton
		{
			get
			{
				if (NumSelected == 1)
				{
					if (Find.Selector.SelectedZone != null)
					{
						return Find.Selector.SelectedZone.ContainsCell(lastSelectCell);
					}
					return true;
				}
				return false;
			}
		}

		public bool ShouldShowPaneContents => NumSelected == 1;

		public IEnumerable<InspectTabBase> CurTabs
		{
			get
			{
				if (Find.ScreenshotModeHandler.Active)
				{
					return null;
				}
				if (NumSelected == 1)
				{
					if (SelThing != null && (SelThing.def.inspectorTabsResolved != null || (SelThing is IStorageGroupMember storageGroupMember && storageGroupMember.DrawStorageTab)))
					{
						return SelThing.GetInspectTabs();
					}
					if (SelZone != null)
					{
						return SelZone.GetInspectTabs();
					}
				}
				else if (Selected.Count > 1 && Selected.All((object s) => s is IStorageGroupMember))
				{
					return (Selected.First() as Thing).GetInspectTabs();
				}
				return null;
			}
		}

		public MainTabWindow_Inspect()
		{
			closeOnAccept = false;
			closeOnCancel = false;
			drawInScreenshotMode = false;
			soundClose = SoundDefOf.TabClose;
		}

		public override void ExtraOnGUI()
		{
			base.ExtraOnGUI();
			InspectPaneUtility.ExtraOnGUI(this);
			if (AnythingSelected && Find.DesignatorManager.SelectedDesignator != null)
			{
				Find.DesignatorManager.SelectedDesignator.DoExtraGuiControls(0f, PaneTopY);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			InspectPaneUtility.InspectPaneOnGUI(inRect, this);
			if (lastSelectedThing != SelThing)
			{
				SetInitialSizeAndPosition();
				lastSelectedThing = SelThing;
			}
			else if (RequestedTabSize != lastTabSize)
			{
				SetInitialSizeAndPosition();
				lastTabSize = RequestedTabSize;
			}
		}

		public string GetLabel(Rect rect)
		{
			return InspectPaneUtility.AdjustedLabelFor(Selected, rect);
		}

		public void DrawInspectGizmos()
		{
			InspectGizmoGrid.DrawInspectGizmoGridFor(Selected, out mouseoverGizmo);
		}

		public void DoPaneContents(Rect rect)
		{
			InspectPaneFiller.DoPaneContentsFor((ISelectable)Find.Selector.FirstSelectedObject, rect);
		}

		public void DoInspectPaneButtons(Rect rect, ref float lineEndWidth)
		{
			if (NumSelected != 1)
			{
				return;
			}
			Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
			if (singleSelectedThing == null)
			{
				return;
			}
			float num = rect.width - 48f;
			Widgets.InfoCardButton(num, 0f, Find.Selector.SingleSelectedThing);
			lineEndWidth += 24f;
			Pawn p;
			if ((p = singleSelectedThing as Pawn) == null)
			{
				return;
			}
			if (p.playerSettings != null && p.playerSettings.UsesConfigurableHostilityResponse)
			{
				num -= 24f;
				HostilityResponseModeUtility.DrawResponseButton(new Rect(num, 0f, 24f, 24f), p, paintable: false);
				lineEndWidth += 24f;
			}
			if ((p.Faction == Faction.OfPlayer && p.RaceProps.Animal && p.RaceProps.hideTrainingTab) || (ModsConfig.BiotechActive && p.IsColonyMech))
			{
				num -= 30f;
				TrainingCardUtility.DrawRenameButton(new Rect(num, 0f, 30f, 30f), p);
				lineEndWidth += 30f;
			}
			if (p.guilt != null && p.guilt.IsGuilty)
			{
				num -= 26f;
				Rect rect2 = new Rect(num, 0f, 26f, 26f);
				GUI.DrawTexture(rect2, TexUI.GuiltyTex);
				TooltipHandler.TipRegion(rect2, () => p.guilt.Tip, 6321223);
				lineEndWidth += 26f;
			}
		}

		public void SelectNextInCell()
		{
			if (NumSelected != 1)
			{
				return;
			}
			Selector selector = Find.Selector;
			if (selector.SelectedZone == null || selector.SelectedZone.ContainsCell(lastSelectCell))
			{
				if (selector.SelectedZone == null)
				{
					lastSelectCell = selector.SingleSelectedThing.PositionHeld;
				}
				selector.SelectNextAt(map: (selector.SingleSelectedThing == null) ? selector.SelectedZone.Map : selector.SingleSelectedThing.MapHeld, c: lastSelectCell);
			}
		}

		public override void WindowUpdate()
		{
			base.WindowUpdate();
			InspectPaneUtility.UpdateTabs(this);
			lastMouseOverGizmo = mouseoverGizmo;
			if (mouseoverGizmo != null)
			{
				mouseoverGizmo.GizmoUpdateOnMouseover();
			}
		}

		public void CloseOpenTab()
		{
			openTabType = null;
		}

		public void Reset()
		{
			openTabType = null;
		}
	}
}
