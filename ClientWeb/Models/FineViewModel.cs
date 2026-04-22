namespace ClientWeb.Models
{
    public class FineViewModel
    {
        public int Id { get; set; }
        public int BorrowRecordId { get; set; }
        public string? BookTitle { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Số tiền phạt")]
        public decimal Amount { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Đã thanh toán")]
        public bool IsPaid { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Ngày thanh toán")]
        public DateTime? PaidDate { get; set; }
    }
}
