using BudgetApp.Interfaces;
namespace BudgetApp.Interfaces
{
    public interface IDatabase
    {
        void Insert(object entity);
        void Update(object entity);
        void Delete(object entity);
        List<T> FetchAll<T>() where T : class;
    }
}
