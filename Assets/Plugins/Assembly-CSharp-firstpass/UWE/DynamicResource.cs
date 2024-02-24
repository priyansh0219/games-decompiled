namespace UWE
{
	public abstract class DynamicResource
	{
		public virtual bool IsManaged()
		{
			return true;
		}

		public abstract float GetImportance();

		public abstract bool IsLoaded();

		public abstract float GetSizeMBs();

		public abstract void StartLoad();

		public abstract void StartUnload();

		public abstract bool IsBusy();
	}
}
