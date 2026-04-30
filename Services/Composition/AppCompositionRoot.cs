using Masroofy.App.Data;
using Masroofy.App.Data.Repositories;

namespace Masroofy.App.Services.Composition;

public sealed class AppCompositionRoot
{
    public SQLiteHelper Database { get; }
    public IInfrastructureRepository InfrastructureRepository { get; }
    public IUserRepository UserRepository { get; }
    public IBudgetCycleRepository BudgetCycleRepository { get; }
    public IExpenseRepository ExpenseRepository { get; }
    public ICategoryRepository CategoryRepository { get; }
    public IAuditRepository AuditRepository { get; }
    public IDebtRepository DebtRepository { get; }
    public SecurityService Security { get; }
    public IAuthService Auth { get; }
    public ThemeManager Theme { get; }
    public InitialSetupService Setup { get; }

    public AppCompositionRoot(string dbPath)
    {
        Database = new SQLiteHelper(dbPath);
        InfrastructureRepository = new SqliteInfrastructureRepository(Database);
        UserRepository = new SqliteUserRepository(Database);
        BudgetCycleRepository = new SqliteBudgetCycleRepository(Database);
        ExpenseRepository = new SqliteExpenseRepository(Database);
        CategoryRepository = new SqliteCategoryRepository(Database);
        AuditRepository = new SqliteAuditRepository(Database);
        DebtRepository = new SqliteDebtRepository(Database);
        Security = new SecurityService();
        InfrastructureRepository.InitializeDatabase(Security.HashPinSha256("1234"));
        Auth = new AuthService(UserRepository, Security);
        Theme = new ThemeManager();
        Setup = new InitialSetupService(Auth, UserRepository, InfrastructureRepository, BudgetCycleRepository, AuditRepository);
    }
}
