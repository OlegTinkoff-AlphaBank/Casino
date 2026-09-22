namespace server.DTOs;

public record PlayDTO(int GameId, int UserId, decimal BetAmount);

public record PlayResultDto(bool IsWin, decimal Payout, decimal BalanceAfter, int? WinningOutcomeId);