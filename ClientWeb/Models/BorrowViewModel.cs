using System.ComponentModel.DataAnnotations;

namespace ClientWeb.Models
{
    public class BorrowViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Người mượn")]
        public int UserId { get; set; }
        public string? UserName { get; set; }

        [Display(Name = "Sách")]
        public int BookId { get; set; }
        public string? BookTitle { get; set; }
        public string? Author { get; set; }

        [Display(Name = "Ngày mượn")]
        public DateTime BorrowDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày hết hạn")]
        [Display(Name = "Hạn trả")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Ngày trả")]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Số ngày còn lại")]
        public int DaysLeft { get; set; }

        public FineViewModel? Fine { get; set; }
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string? FullName { get; set; }
    }
}
