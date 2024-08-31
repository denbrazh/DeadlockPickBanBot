using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeadlockPickBanBot.Database;

public class Heroes
{
    public int HeroId { get; set; }
    [Required] public string HeroName { get; set; } = null!;
}

public class Players
{
    public int PlayerId { get; set; }
    [Required] public string PlayerName { get; set; } = null!;
    [Required] public string TgUsername { get; set; } = null!;
    [Required] public string TeamId { get; set; } = null!;
    public string PlayedMatches { get; set; } = null!; // TODO: как лучше хранить и обрабатывать историю матчей?
}

public class Teams
{
    public int TeamId { get; set; }
    [Required] public string OurTeamId { get; set; } = null!; // GUID
    [Required] public string TeamName { get; set; } = null!;
    [Required] public string TeamRoster { get; set; } = null!;
    // TODO: создать команду Team Reserve для челов в резерве
}

public class MatchHistory
{
    public int MatchId { get; set; }
    [Required] public string Winner { get; set; } = null!;
    [Required] public int TgChatId { get; set; }
    [Required] public DateTime MatchDate { get; set; }
}

public class MatchShedule
{
    public int Id { get; set; }
    [Required] public DateTime MatchDate { get; set; }
    [Required] public string Team1Id { get; set; } = null!;
    [Required] public string Team2Id { get; set; } = null!;
}

public class PickBanSchemas
{
    public int PickBanSchemaId { get; set; }
    public string PickBanSchema { get; set; } = null!;
}

public class PickBanHistory
{
    public int PickBanSessionId { get; set; }
    [Required] public int PickBanSchemaId { get; set; }
    [Required] public string OurMatchId { get; set; } = null!;
    [Required] public string PickBanSession { get; set; } = null!;
}

public class Lobby
{
    public int LobbyId { get; set; }
    public int TgChatId { get; set; }
    public int PickBanSchemaId { get; set; }
    public string Team1Id { get; set; } = null!;
    public string Team2Id { get; set; } = null!;
}
