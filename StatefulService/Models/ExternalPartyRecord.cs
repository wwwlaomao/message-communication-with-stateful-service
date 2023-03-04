using System.Threading.Channels;

namespace StatefulService.Models;

public record ExternalPartyRecord(Channel<object> Channel);
