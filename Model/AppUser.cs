using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFWeb.Model
{
    public class AppUser : IdentityUser
    {
        [Column(TypeName = "nvarchar")]
        [StringLength(400)]
        public string? HomeAddress { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Column(TypeName = "nvarchar")]
        [StringLength(20)]
        public string? Gender { get; set; }
    }
}
