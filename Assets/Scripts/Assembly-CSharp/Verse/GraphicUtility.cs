using System;
using RimWorld;

namespace Verse
{
	public static class GraphicUtility
	{
		public static Graphic ExtractInnerGraphicFor(this Graphic outerGraphic, Thing thing, int? indexOverride = null)
		{
			if (outerGraphic is Graphic_RandomRotated graphic_RandomRotated)
			{
				return ResolveGraphicInner(graphic_RandomRotated.SubGraphic);
			}
			return ResolveGraphicInner(outerGraphic);
			Graphic ResolveGraphicInner(Graphic g)
			{
				if (g is Graphic_Random graphic_Random)
				{
					if (indexOverride.HasValue)
					{
						return graphic_Random.SubGraphicAtIndex(indexOverride.Value);
					}
					return graphic_Random.SubGraphicFor(thing);
				}
				if (g is Graphic_Appearances graphic_Appearances)
				{
					return graphic_Appearances.SubGraphicFor(thing);
				}
				if (g is Graphic_Genepack graphic_Genepack)
				{
					return graphic_Genepack.SubGraphicFor(thing);
				}
				if (g is Graphic_MealVariants graphic_MealVariants)
				{
					return graphic_MealVariants.SubGraphicFor(thing);
				}
				return g;
			}
		}

		public static Graphic_Linked WrapLinked(Graphic subGraphic, LinkDrawerType linkDrawerType)
		{
			switch (linkDrawerType)
			{
			case LinkDrawerType.None:
				return null;
			case LinkDrawerType.Basic:
				return new Graphic_Linked(subGraphic);
			case LinkDrawerType.CornerFiller:
				return new Graphic_LinkedCornerFiller(subGraphic);
			case LinkDrawerType.Transmitter:
				return new Graphic_LinkedTransmitter(subGraphic);
			case LinkDrawerType.TransmitterOverlay:
				return new Graphic_LinkedTransmitterOverlay(subGraphic);
			case LinkDrawerType.Asymmetric:
				return new Graphic_LinkedAsymmetric(subGraphic);
			default:
				throw new ArgumentException();
			}
		}
	}
}
