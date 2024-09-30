using minimal_api.Domain.Entities;
using MinimalApi.DTOs;

namespace minimal_api.infrastructure.Interfaces
{
    public interface IAdminService
    {
        Admin? Login(LoginDTO loginDTO);
    }
}