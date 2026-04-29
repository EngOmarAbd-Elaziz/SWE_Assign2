using BudgetApp.Services;

namespace BudgetApp.Controllers
{
    public class BudgetController
    {
        private readonly BudgetService _service;

        public BudgetController(BudgetService service)
        {
            _service = service;
        }

        public double GetDailyLimit()
        {
            return _service.GetDailyLimit();
        }
    }
}