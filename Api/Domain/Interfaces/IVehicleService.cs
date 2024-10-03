using minimal_api.Domain.Entities;
using MinimalApi.DTOs;

namespace minimal_api.infrastructure.Interfaces
{
    public interface IVehicleService
    {
        List<Vehicle>? Todos(int? pagina = 1, string? nome = null, string? marca = null);
        Vehicle? GetById(int id);
        void Include(Vehicle vehicle);
        void Update(Vehicle vehicle);
        void Delete(Vehicle vehicle);
    }
}