using System;
using System.Collections.Generic;

namespace WebAPI.Models;

public partial class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string? Description { get; set; }

    public int CategoryId { get; set; }

    public int TotalQuantity { get; set; }

    public int AvailableQuantity { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();

    public virtual Category Category { get; set; } = null!;
}
