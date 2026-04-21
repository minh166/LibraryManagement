using System.ComponentModel.DataAnnotations;

namespace WebAPI.DTOs
{
    public class BookDTO
    {
        // Request - Tạo/Sửa sách
        public class BookRequestDTO
        {
            [Required]
            public string Title { get; set; }

            [Required]
            public string Author { get; set; }

            public string? Description { get; set; }

            [Required]
            public int CategoryId { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
            public int TotalQuantity { get; set; }
        }

        // Response - Trả về cho client
        public class BookResponseDTO
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public string? Description { get; set; }
            public string CategoryName { get; set; }
            public int TotalQuantity { get; set; }
            public int AvailableQuantity { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
