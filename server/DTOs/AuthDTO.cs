namespace server.DTOs;

public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Username, string Password);
public record AuthResponseDto(string AccessToken, string RefreshToken);
public record RefreshRequestDto(string RefreshToken);