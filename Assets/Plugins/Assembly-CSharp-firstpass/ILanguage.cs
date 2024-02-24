public interface ILanguage
{
	bool Contains(string key);

	string Get(string key);

	string GetFormat(string key, params object[] args);
}
