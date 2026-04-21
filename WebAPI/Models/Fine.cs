using System;
using System.Collections.Generic;

namespace WebAPI.Models;

public partial class Fine
{
    public int Id { get; set; }

    public int BorrowRecordId { get; set; }

    public decimal Amount { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaidDate { get; set; }

    public virtual BorrowRecord BorrowRecord { get; set; } = null!;
}
