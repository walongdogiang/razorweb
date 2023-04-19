using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFWeb.Model
{
    // [Table("My_Article")] //nếu ko có dòng này, tên db sẽ lấy theo tên Class
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255, MinimumLength = 5, ErrorMessage = "Length must be greater than {2} characters.")]
        [Required(ErrorMessage = "{0} is null")]
        [Column(TypeName = "nvarchar")]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required(ErrorMessage = "{0} is null")]
        [DataType(DataType.Date)]
        [DisplayName("Created Date")]
        public DateTime Created { get; set; }

        [Column(TypeName = "ntext")]
        [DisplayName("Content")]
        public string? Content { get; set; }
    }
}
