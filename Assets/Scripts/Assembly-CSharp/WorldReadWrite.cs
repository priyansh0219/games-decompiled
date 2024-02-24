public interface WorldReadWrite
{
	Int3 GetMins();

	Int3 GetMaxs();

	byte Get(int x, int y, int z);

	void Set(int x, int y, int z, byte type);

	bool CanWrite(int x, int y, int z);

	bool CanWrite(Int3 mins, Int3 maxs);
}
