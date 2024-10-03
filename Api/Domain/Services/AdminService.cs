using Microsoft.EntityFrameworkCore;
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

        public Admin? GetById(int id)
        {
            return _context.Admins.Where(v => v.Id == id).FirstOrDefault();
        }

        public Admin Include(Admin admin)
        {
            _context.Admins.Add(admin);
            _context.SaveChanges();

            return admin;
        }

        public Admin? Login(LoginDTO loginDTO)
        {
            var adm = _context.Admins.Where(a => a.Email == loginDTO.Email && a.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
        }

        public List<Admin> Todos(int? pagina)
        {
            var query = _context.Admins.AsQueryable();
            int itensPorPagina = 10;

            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
                
            return query.ToList();
        }
    }
}