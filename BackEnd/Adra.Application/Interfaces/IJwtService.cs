using Adra.Application.DTOs;
using Adra.Core.Entities;

namespace Adra.Application.Interfaces;

public interface IJwtService
{
    LoginResponse GenerateToken(User user, List<string> roles);
}
