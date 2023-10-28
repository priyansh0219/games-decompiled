using RimWorld;

namespace Verse
{
	public class Dialog_RenameAnimalPen : Dialog_Rename
	{
		private readonly Map map;

		private readonly CompAnimalPenMarker marker;

		public Dialog_RenameAnimalPen(Map map, CompAnimalPenMarker marker)
		{
			this.map = map;
			this.marker = marker;
			curName = marker.label;
		}

		protected override AcceptanceReport NameIsValid(string name)
		{
			AcceptanceReport result = base.NameIsValid(name);
			if (!result.Accepted)
			{
				return result;
			}
			if (name != marker.label && map.animalPenManager.GetPenNamed(name) != null)
			{
				return "NameIsInUse".Translate();
			}
			return true;
		}

		protected override void SetName(string name)
		{
			marker.label = name;
		}
	}
}
