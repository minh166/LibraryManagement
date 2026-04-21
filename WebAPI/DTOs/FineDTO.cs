namespace WebAPI.DTOs
{
    public class FineDTO
    {
        // Response
        public class FineResponseDTO
        {
            public int Id { get; set; }
            public int BorrowRecordId { get; set; }
            public string BookTitle { get; set; }
            public decimal Amount { get; set; }
            public bool IsPaid { get; set; }
            public DateTime? PaidDate { get; set; }
        }
    }
}
