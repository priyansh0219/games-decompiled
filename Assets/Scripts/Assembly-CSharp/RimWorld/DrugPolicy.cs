using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class DrugPolicy : IExposable, ILoadReferenceable
	{
		public int uniqueId;

		public string label;

		public DrugPolicyDef sourceDef;

		private List<DrugPolicyEntry> entriesInt;

		public int Count => entriesInt.Count;

		public DrugPolicyEntry this[int index]
		{
			get
			{
				return entriesInt[index];
			}
			set
			{
				entriesInt[index] = value;
			}
		}

		public DrugPolicyEntry this[ThingDef drugDef]
		{
			get
			{
				for (int i = 0; i < entriesInt.Count; i++)
				{
					if (entriesInt[i].drug == drugDef)
					{
						return entriesInt[i];
					}
				}
				throw new ArgumentException();
			}
		}

		public DrugPolicy()
		{
		}

		public DrugPolicy(int uniqueId, string label)
		{
			this.uniqueId = uniqueId;
			this.label = label;
			InitializeIfNeeded();
		}

		public void InitializeIfNeeded(bool overwriteExisting = true)
		{
			if (overwriteExisting)
			{
				if (entriesInt != null)
				{
					return;
				}
				entriesInt = new List<DrugPolicyEntry>();
			}
			List<ThingDef> thingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
			int i;
			for (i = 0; i < thingDefs.Count; i++)
			{
				if (thingDefs[i].category == ThingCategory.Item && thingDefs[i].IsDrug && (overwriteExisting || !entriesInt.Any((DrugPolicyEntry x) => x.drug == thingDefs[i])))
				{
					DrugPolicyEntry drugPolicyEntry = new DrugPolicyEntry();
					drugPolicyEntry.drug = thingDefs[i];
					drugPolicyEntry.allowedForAddiction = true;
					entriesInt.Add(drugPolicyEntry);
				}
			}
			entriesInt.SortBy((DrugPolicyEntry e) => e.drug.GetCompProperties<CompProperties_Drug>().listOrder);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref uniqueId, "uniqueId", 0);
			Scribe_Values.Look(ref label, "label");
			Scribe_Collections.Look(ref entriesInt, "drugs", LookMode.Deep);
			Scribe_Defs.Look(ref sourceDef, "sourceDef");
			if (Scribe.mode == LoadSaveMode.PostLoadInit && entriesInt != null)
			{
				if (entriesInt.RemoveAll((DrugPolicyEntry x) => x == null || x.drug == null) != 0)
				{
					Log.Error("Some DrugPolicyEntries were null after loading.");
				}
				InitializeIfNeeded(overwriteExisting: false);
			}
		}

		public string GetUniqueLoadID()
		{
			return "DrugPolicy_" + label + uniqueId;
		}
	}
}
