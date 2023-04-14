using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EFWeb.Model
{
    // [Table("My_Article")] //nếu ko có dòng này, tên db sẽ lấy theo tên Class
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255)]
        [Required]
        [Column(TypeName = "nvarchar")]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Created { get; set; }

        [Column(TypeName = "ntext")]
        public string Content { get; set; }
    }
}
