using minimal_api.Domain.Entities;
using minimal_api.infrastructure.Db;
using minimal_api.infrastructure.Interfaces;

namespace minimal_api.Domain.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly DataBaseContext _context;
        public VehicleService(DataBaseContext context)
        {
            _context = context;
        }

        public void Delete(Vehicle vehicle)
        {
            _context.Vehicles.Remove(vehicle);
            _context.SaveChanges();
        }

        public Vehicle? GetById(int id)
        {
            return _context.Vehicles.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Include(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
            _context.SaveChanges();
        }

    
        public void Update(Vehicle vehicle)
        {
            _context.Vehicles.Update(vehicle);
            _context.SaveChanges();
        }

        public List<Vehicle>? Todos(int? pagina = 1, string? nome = null, string? marca = null)
        {
            var query = _context.Vehicles.AsQueryable();
            if (!string.IsNullOrEmpty(nome))
            {
                query = query.Where(v => v.Nome.ToLower().Contains(nome));
            }

            int itensPorPagina = 10;

            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);
                
            return query.ToList();
        }
    }
}