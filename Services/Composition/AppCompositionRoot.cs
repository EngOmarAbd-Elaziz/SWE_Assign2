using Masroofy.App.Data;
using Masroofy.App.Data.Repositories;

namespace Masroofy.App.Services.Composition;

/// <summary>
/// Acts as the application's composition root, constructing and wiring all core services,
/// repositories, and infrastructure dependencies from a single database path entry point.
/// Intended to be instantiated once at application startup and shared across the lifetime
/// of the app.
/// </summary>
public sealed class AppCompositionRoot
{
    /// <summary>
    /// Gets the low-level SQLite helper used to create connections and execute schema operations.
    /// </summary>
    public SQLiteHelper Database { get; }

    /// <summary>
    /// Gets the repository responsible for infrastructure-level operations such as database
    /// initialization and raw connection creation.
    /// </summary>
    public IInfrastructureRepository InfrastructureRepository { get; }

    /// <summary>
    /// Gets the repository responsible for user record queries and persistence.
    /// </summary>
    public IUserRepository UserRepository { get; }

    /// <summary>
    /// Gets the repository responsible for budget cycle queries and persistence.
    /// </summary>
    public IBudgetCycleRepository BudgetCycleRepository { get; }

    /// <summary>
    /// Gets the repository responsible for expense queries and persistence.
    /// </summary>
    public IExpenseRepository ExpenseRepository { get; }

    /// <summary>
    /// Gets the repository responsible for expense category queries and persistence.
    /// </summary>
    public ICategoryRepository CategoryRepository { get; }

    /// <summary>
    /// Gets the repository responsible for writing and reading audit log entries.
    /// </summary>
    public IAuditRepository AuditRepository { get; }

    /// <summary>
    /// Gets the repository responsible for debt record queries and persistence.
    /// </summary>
    public IDebtRepository DebtRepository { get; }

    /// <summary>
    /// Gets the service responsible for PIN hashing and verification.
    /// </summary>
    public SecurityService Security { get; }

    /// <summary>
    /// Gets the service responsible for user authentication and registration.
    /// </summary>
    public IAuthService Auth { get; }

    /// <summary>
    /// Gets the service responsible for managing the application's visual theme.
    /// </summary>
    public ThemeManager Theme { get; }

    /// <summary>
    /// Gets the service responsible for guiding the user through first-run initial setup,
    /// including admin account creation and default budget cycle configuration.
    /// </summary>
    public InitialSetupService Setup { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="AppCompositionRoot"/>, constructing all
    /// repositories and services in dependency order. The database schema is initialized
    /// immediately using a default hashed PIN of 1234.
    /// </summary>
    /// <param name="dbPath">
    /// The file path to the SQLite database. The file will be created if it does not exist.
    /// </param>
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
