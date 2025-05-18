using System.Linq;

namespace ChapeauPOS.Models.ViewModels
{
    public class MenuViewModel
    {
        public string CategoryName { get; set; }
        public List<MenuItem> Category { get; set; }

        // Safely handle null Category list
        public List<MenuItem> Starters =>
            Category?.Where(item => item.Course == MenuCourse.Starter).ToList() ?? new List<MenuItem>();

        public List<MenuItem> MainCourses =>
            Category?.Where(item => item.Course == MenuCourse.Main).ToList() ?? new List<MenuItem>();

        public List<MenuItem> Desserts
        {
            get
            {
                return Category != null
                    ? Category.Where(item => item.Course == MenuCourse.Dessert).ToList()
                    : new List<MenuItem>();
            }
        }

        public List<MenuItem> Drinks { get; set; }
        public List<MenuItem> SoftDrinks { get; set; }
        public List<MenuItem> Beers { get; set; }
        public List<MenuItem> Wines { get; set; }
        public List<MenuItem> Spirits { get; set; }
        public List<MenuItem> WarmDrinks { get; set; }

        public MenuViewModel(string categoryName, List<MenuItem> category, List<MenuItem> drinks)
        {
            CategoryName = categoryName;
            Category = category ?? new List<MenuItem>();
            Drinks = drinks ?? new List<MenuItem>();
        }
    }
}
