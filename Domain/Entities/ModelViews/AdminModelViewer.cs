using minimal_api.Domain.Enums;

namespace minimal_api.Domain.Entities.ModelViews
{
    public record AdminModelViewer
    {
        public int Id { get; set; } = default!;
        public string? Email { get; set; } = default!;
        public string? Perfil { get; set; } = default!;
    }
}