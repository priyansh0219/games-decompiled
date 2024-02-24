using System;
using Oculus.Platform.Models;

namespace Oculus.Platform
{
	public class MessageWithProduct : Message<Product>
	{
		public MessageWithProduct(IntPtr c_message)
			: base(c_message)
		{
		}

		protected override Product GetDataFromMessage(IntPtr c_message)
		{
			return Message.Deserialize<Product>(c_message);
		}

		public override Product GetProduct()
		{
			return base.Data;
		}
	}
}
