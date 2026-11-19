namespace Backend.Models.Racing;

public sealed record LobbyParticipantResponse(Guid UserId, string DisplayName);
public sealed record LobbyResponse(Guid RaceId, Guid? HostUserId, Guid CurrentUserId,
    int MaxParticipants, IReadOnlyList<LobbyParticipantResponse> Participants);
