using Verse;

namespace RimWorld
{
	public abstract class Building_CorpseCasket : Building_Casket, IStoreSettingsParent, IHaulDestination
	{
		protected StorageSettings storageSettings;

		public virtual bool StorageTabVisible => !HasCorpse;

		public bool HasCorpse => Corpse != null;

		public Corpse Corpse
		{
			get
			{
				for (int i = 0; i < innerContainer.Count; i++)
				{
					if (innerContainer[i] is Corpse result)
					{
						return result;
					}
				}
				return null;
			}
		}

		public StorageSettings GetStoreSettings()
		{
			return storageSettings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return def.building.fixedStorageSettings;
		}

		public void Notify_SettingsChanged()
		{
		}

		public override void PostMake()
		{
			base.PostMake();
			storageSettings = new StorageSettings(this);
			if (def.building.defaultStorageSettings != null)
			{
				storageSettings.CopyFrom(def.building.defaultStorageSettings);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref storageSettings, "storageSettings", this);
		}
	}
}
