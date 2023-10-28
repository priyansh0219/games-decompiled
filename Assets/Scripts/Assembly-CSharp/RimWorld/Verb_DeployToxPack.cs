using Verse;

namespace RimWorld
{
	public class Verb_DeployToxPack : Verb
	{
		protected override bool TryCastShot()
		{
			return TryDeploy(base.EquipmentSource.TryGetComp<CompReloadable>(), base.EquipmentSource.TryGetComp<CompReleaseGas>());
		}

		public static bool TryDeploy(CompReloadable reloadable, CompReleaseGas releaseGas)
		{
			if (!ModLister.CheckBiotech("Tox packs"))
			{
				return false;
			}
			if (reloadable == null || !reloadable.CanBeUsed || releaseGas == null)
			{
				return false;
			}
			reloadable.UsedOnce();
			releaseGas.StartRelease();
			return true;
		}
	}
}
