namespace AuthWithAdmin.Server.AuthHelpers;

public class InMemoryTokenBlacklistService: ITokenBlacklistService
{
    private readonly HashSet<string> _blacklist = new HashSet<string>();

    public void AddToBlacklist(string token)
    {
        _blacklist.Add(token);
    }

    public bool IsBlacklisted(string token)
    {
        return _blacklist.Contains(token);
    }

}