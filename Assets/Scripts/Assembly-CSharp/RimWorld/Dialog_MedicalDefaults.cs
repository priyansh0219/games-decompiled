using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_MedicalDefaults : Window
	{
		private const float MedicalCareStartX = 170f;

		private const float VerticalGap = 6f;

		private const float HeaderHeight = 30f;

		public override Vector2 InitialSize => new Vector2(346f, 360f);

		public override string CloseButtonText => "OK".Translate();

		public Dialog_MedicalDefaults()
		{
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, 0f, InitialSize.x, 30f), "DefaultMedicineSettings".Translate());
			Text.Font = GameFont.Small;
			inRect.yMin += 40f;
			Text.Anchor = TextAnchor.MiddleLeft;
			Rect rect = new Rect(inRect.x, inRect.y, 170f, 28f);
			Rect rect2 = new Rect(170f, inRect.y, 140f, 28f);
			Widgets.LabelFit(rect, "MedGroupColonist".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.PlaySettings.defaultCareForColonyHumanlike);
			rect.y += 34f;
			rect2.y += 34f;
			Widgets.LabelFit(rect, "MedGroupImprisonedColonist".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.PlaySettings.defaultCareForColonyPrisoner);
			rect.y += 34f;
			rect2.y += 34f;
			if (ModsConfig.IdeologyActive)
			{
				Widgets.LabelFit(rect, "MedGroupEnslavedColonist".Translate());
				MedicalCareUtility.MedicalCareSetter(rect2, ref Find.PlaySettings.defaultCareForColonySlave);
				rect.y += 34f;
				rect2.y += 34f;
			}
			Widgets.LabelFit(rect, "MedGroupColonyAnimal".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.PlaySettings.defaultCareForColonyAnimal);
			rect.y += 34f;
			rect2.y += 34f;
			Widgets.LabelFit(rect, "MedGroupNeutralAnimal".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.PlaySettings.defaultCareForNeutralAnimal);
			rect.y += 34f;
			rect2.y += 34f;
			Widgets.LabelFit(rect, "MedGroupNeutralFaction".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.PlaySettings.defaultCareForNeutralFaction);
			rect.y += 34f;
			rect2.y += 34f;
			Widgets.LabelFit(rect, "MedGroupHostileFaction".Translate());
			MedicalCareUtility.MedicalCareSetter(rect2, ref Find.PlaySettings.defaultCareForHostileFaction);
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
}
