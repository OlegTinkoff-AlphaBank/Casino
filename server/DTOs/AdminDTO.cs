namespace server.DTOs;

public record AdminAddBalanceDto(string Username, decimal Amount);
public record CreateGameDto(string Name, decimal MinBet, decimal MaxBet, decimal GameRate);
public record CreateGameOutcomeDto(int GameId, decimal Multiplier, decimal Weight);