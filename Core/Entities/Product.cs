namespace Core.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int Price { get; set; }
        public int Quanity { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
