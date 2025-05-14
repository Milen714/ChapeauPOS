using System.Linq;

namespace ChapeauPOS.Models.ViewModels
{
    public class MenuViewModel
    {
        public string CategoryName { get; set; }
        public List<MenuItem> Category { get; set; }
        public List<MenuItem> Starters
        {
            get
            {
                return Category.Where(item => item.Course == MenuCourse.Starter).ToList();
            }
        }
        public List<MenuItem> MainCourses
        {
            get
            {
                return Category.Where(item => item.Course == MenuCourse.Main).ToList();
            }
        }
        public List<MenuItem> Desserts
        {
            get
            {
                return Category.Where(item => item.Course == MenuCourse.Dessert).ToList();
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
            Category = category;
            Drinks = drinks;
        }
    }
}
