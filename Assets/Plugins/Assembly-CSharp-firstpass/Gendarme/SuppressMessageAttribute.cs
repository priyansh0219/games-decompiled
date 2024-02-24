using System;

namespace Gendarme
{
	[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
	public sealed class SuppressMessageAttribute : Attribute
	{
		private string category;

		private string checkId;

		public string Category => category;

		public string CheckId => checkId;

		public SuppressMessageAttribute(string _category, string _checkId)
		{
			category = _category;
			checkId = _checkId;
		}
	}
}
