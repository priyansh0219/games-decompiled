using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithUserProof : Message<UserProof>
	{
		public MessageWithUserProof(IntPtr c_message)
			: base(c_message)
		{
		}

		protected override UserProof GetDataFromMessage(IntPtr c_message)
		{
			return Message.Deserialize<UserProof>(c_message);
		}

		public override UserProof GetUserProof()
		{
			return base.Data;
		}
	}
}
