using System.ComponentModel.DataAnnotations;

namespace ClientWeb.Models
{
    public class BookViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sách")]
        [Display(Name = "Tên sách")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên tác giả")]
        [Display(Name = "Tác giả")]
        public string Author { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thể loại")]
        [Display(Name = "Thể loại")]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        [Display(Name = "Tổng số lượng")]
        public int TotalQuantity { get; set; }

        [Display(Name = "Còn lại")]
        public int AvailableQuantity { get; set; }

        [Display(Name = "Ngày thêm")]
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
