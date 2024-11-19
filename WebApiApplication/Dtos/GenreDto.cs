using System.ComponentModel.DataAnnotations;

namespace WebApiApplication.Dtos
{
    public class GenreDto
    {
        [MaxLength(100)]
        public string Name { get; set; }
    }
}
