using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace FirstWorldProblems
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryText { get; set; }
        public DateTime DateAdded { get; set; }
        public bool ViewCategoryFilter { get; set; }
    }
}
