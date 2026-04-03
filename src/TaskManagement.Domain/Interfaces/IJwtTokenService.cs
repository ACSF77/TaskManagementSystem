using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
