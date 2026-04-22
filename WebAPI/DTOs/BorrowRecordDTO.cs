using System.ComponentModel.DataAnnotations;
using static WebAPI.DTOs.FineDTO;

namespace WebAPI.DTOs
{
    public class BorrowRecordDTO
    {
        // Request - Tạo phiếu mượn
        public class BorrowRequestDTO
        {
            [Required]
            public int UserId { get; set; }

            [Required]
            public int BookId { get; set; }

            [Required]
            public DateTime DueDate { get; set; }
        }

        // Request - Gia hạn
        public class ExtendBorrowDTO
        {
            [Required]
            public DateTime NewDueDate { get; set; }
        }

        // Response
        public class BorrowResponseDTO
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string BookTitle { get; set; }
            public string Author { get; set; }
            public DateTime BorrowDate { get; set; }
            public DateTime DueDate { get; set; }
            public DateTime? ReturnDate { get; set; }
            public string Status { get; set; }
            public int DaysLeft { get; set; }
            public FineResponseDTO? Fine { get; set; }
        }
    }
}
