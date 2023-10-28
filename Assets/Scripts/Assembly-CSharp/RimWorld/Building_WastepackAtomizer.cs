using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_WastepackAtomizer : Building
	{
		private CompAtomizer atomizer;

		private CompPowerTrader powerTrader;

		private Effecter operatingEffecter;

		private Graphic contentsGraphic;

		public CompAtomizer Atomizer
		{
			get
			{
				if (atomizer == null)
				{
					atomizer = GetComp<CompAtomizer>();
				}
				return atomizer;
			}
		}

		private CompPowerTrader PowerTrader
		{
			get
			{
				if (powerTrader == null)
				{
					powerTrader = GetComp<CompPowerTrader>();
				}
				return powerTrader;
			}
		}

		public override void Tick()
		{
			base.Tick();
			if (Atomizer.Empty || !PowerTrader.PowerOn)
			{
				operatingEffecter?.Cleanup();
				operatingEffecter = null;
				return;
			}
			if (operatingEffecter == null)
			{
				operatingEffecter = def.building.wastepackAtomizerOperationEffecter.Spawn();
				operatingEffecter.Trigger(this, new TargetInfo(InteractionCell, base.Map));
			}
			operatingEffecter.EffectTick(this, new TargetInfo(InteractionCell, base.Map));
		}

		public override void Draw()
		{
			base.Draw();
			Vector3 drawPos = DrawPos;
			drawPos.y -= 0.08108108f;
			def.building.wastepackAtomizerBottomGraphic.Graphic.Draw(drawPos, base.Rotation, this);
			Vector3 drawPos2 = DrawPos;
			drawPos2.y -= 3f / 148f;
			def.building.wastepackAtomizerWindowGraphic.Graphic.Draw(drawPos2, base.Rotation, this);
		}
	}
}
