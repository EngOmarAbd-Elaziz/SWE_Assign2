using System.Windows.Controls;

namespace BudgetApp.Helpers
{
    public static class NavigationService
    {
        public static ContentControl MainContent;

        public static void Navigate(UserControl view)
        {
            MainContent.Content = view;
        }
    }
}