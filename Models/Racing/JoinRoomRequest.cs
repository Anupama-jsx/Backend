using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Racing;

public sealed class JoinRoomRequest
{
    [Required]
    [StringLength(64)]
    public string RoomCode { get; init; } = string.Empty;
}
