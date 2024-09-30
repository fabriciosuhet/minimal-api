using minimal_api.Domain.Entities;
using minimal_api.infrastructure.Db;
using minimal_api.infrastructure.Interfaces;
using MinimalApi.DTOs;

namespace minimal_api.Domain.Services
{
    public class AdminService : IAdminService
    {

        private readonly DataBaseContext _context;
        public AdminService(DataBaseContext context)
        {
            _context = context;
        }
        public Admin? Login(LoginDTO loginDTO)
        {
            var adm = _context.Admins.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
           
            
        }
    }
}