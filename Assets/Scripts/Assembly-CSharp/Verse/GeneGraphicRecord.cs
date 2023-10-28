namespace Verse
{
	public struct GeneGraphicRecord
	{
		public Graphic graphic;

		public Graphic rottingGraphic;

		public Gene sourceGene;

		public GeneGraphicRecord(Graphic graphic, Graphic rottingGraphic, Gene sourceGene)
		{
			this.graphic = graphic;
			this.rottingGraphic = rottingGraphic;
			this.sourceGene = sourceGene;
		}
	}
}
