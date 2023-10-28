using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ComplexDef : Def
	{
		public List<ComplexRoomDef> roomDefs;

		public List<ComplexThreat> threats;

		public Type workerClass = typeof(ComplexWorker);

		public float roomRewardCrateFactor = 0.5f;

		public float fixedHostileFactionChance = 0.25f;

		public ThingSetMakerDef rewardThingSetMakerDef;

		[Unsaved(false)]
		private ComplexWorker workerInt;

		public ComplexWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (ComplexWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}
	}
}
