using System.Collections.Generic;

namespace UWE
{
	public class DynamicResourceManager<R> where R : DynamicResource
	{
		private enum QueryType
		{
			MostImportantUnloaded = 0,
			LeastImportantLoaded = 1
		}

		public float budgetMBs;

		public List<R> resources { get; private set; }

		public R busyResource { get; private set; }

		public float loadedMBs { get; private set; }

		public DynamicResourceManager(float budgetMBs)
		{
			this.budgetMBs = budgetMBs;
			resources = new List<R>();
			loadedMBs = 0f;
			busyResource = null;
		}

		private R Query(QueryType type)
		{
			int num = -1;
			for (int i = 0; i < resources.Count; i++)
			{
				if (!resources[i].IsManaged())
				{
					continue;
				}
				if (type == QueryType.MostImportantUnloaded)
				{
					if (!resources[i].IsLoaded() && (num == -1 || resources[i].GetImportance() > resources[num].GetImportance()))
					{
						num = i;
					}
				}
				else if (resources[i].IsLoaded() && (num == -1 || resources[i].GetImportance() < resources[num].GetImportance()))
				{
					num = i;
				}
			}
			if (num == -1)
			{
				return null;
			}
			return resources[num];
		}

		public void Step()
		{
			if (busyResource != null)
			{
				if (busyResource.IsBusy())
				{
					return;
				}
				busyResource = null;
			}
			loadedMBs = 0f;
			for (int i = 0; i < resources.Count; i++)
			{
				if (resources[i].IsLoaded())
				{
					loadedMBs += resources[i].GetSizeMBs();
				}
			}
			if (loadedMBs < budgetMBs)
			{
				R val = Query(QueryType.MostImportantUnloaded);
				if (val == null)
				{
					return;
				}
				if (val.GetSizeMBs() + loadedMBs <= budgetMBs)
				{
					val.StartLoad();
					busyResource = val;
					return;
				}
				R val2 = Query(QueryType.LeastImportantLoaded);
				if (val2 != null && val2.GetImportance() < val.GetImportance())
				{
					val2.StartUnload();
					busyResource = val2;
				}
			}
			else
			{
				R val3 = Query(QueryType.LeastImportantLoaded);
				if (val3 != null)
				{
					val3.StartUnload();
					busyResource = val3;
				}
			}
		}
	}
}
