using System.Collections.Concurrent;
using System.Threading.Channels;
using StatefulService.Models;

namespace StatefulService.Services;

public class ExternalPartyRegistry
{
    private readonly ConcurrentDictionary<string, ExternalPartyRecord> _externalParties;

    public ExternalPartyRegistry()
    {
        _externalParties = new();
    }

    public ExternalPartyRecord? Get(string id)
    {
        if (_externalParties.TryGetValue(id, out ExternalPartyRecord? externalParty))
        {
            return externalParty;
        }
        return null;
    }

    public ExternalPartyRecord Add(string id)
    {
        return _externalParties.GetOrAdd(
            id, new ExternalPartyRecord(Channel.CreateUnbounded<object>()));
    }

    public void Remove(string id)
    {
        _externalParties.TryRemove(id, out ExternalPartyRecord? _);
    }
}
