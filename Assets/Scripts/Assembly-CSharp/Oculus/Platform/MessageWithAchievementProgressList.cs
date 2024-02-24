using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithAchievementProgressList : Message<AchievementProgressList>
	{
		public MessageWithAchievementProgressList(IntPtr c_message)
			: base(c_message)
		{
		}

		protected override AchievementProgressList GetDataFromMessage(IntPtr c_message)
		{
			return Message.Deserialize<AchievementProgressList>(c_message);
		}

		public override AchievementProgressList GetAchievementProgressList()
		{
			return base.Data;
		}
	}
}
