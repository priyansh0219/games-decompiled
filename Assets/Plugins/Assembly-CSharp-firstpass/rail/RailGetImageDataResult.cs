namespace rail
{
	public class RailGetImageDataResult : EventBase
	{
		public string image_data;

		public RailImageDataDescriptor image_data_descriptor = new RailImageDataDescriptor();
	}
}
