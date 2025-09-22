using System.ComponentModel.DataAnnotations;

namespace PaloCafeInventory.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public Roles Rol { get; set; }
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }

    public enum Roles
    {
        Admin = 1,
        Vendedor = 2,
        Contador = 3
    }
}