using Masroofy.App.Data;
using Masroofy.App.Data.Repositories;

namespace Masroofy.App.Services.Composition;

/// <summary>
/// Central composition root responsible for
/// creating and connecting all application services,
/// repositories, and infrastructure dependencies.
/// </summary>
public sealed class AppCompositionRoot
{
    /// <summary>
    /// Main SQLite database helper instance.
    /// </summary>
    public SQLiteHelper Database { get; }

    /// <summary>
    /// Repository responsible for infrastructure operations
    /// such as database initialization and backup.
    /// </summary>
    public IInfrastructureRepository InfrastructureRepository { get; }

    /// <summary>
    /// Repository responsible for user operations.
    /// </summary>
    public IUserRepository UserRepository { get; }

    /// <summary>
    /// Repository responsible for budget cycle operations.
    /// </summary>
    public IBudgetCycleRepository BudgetCycleRepository { get; }

    /// <summary>
    /// Repository responsible for expense operations.
    /// </summary>
    public IExpenseRepository ExpenseRepository { get; }

    /// <summary>
    /// Repository responsible for category operations.
    /// </summary>
    public ICategoryRepository CategoryRepository { get; }

    /// <summary>
    /// Repository responsible for audit log operations.
    /// </summary>
    public IAuditRepository AuditRepository { get; }

    /// <summary>
    /// Repository responsible for debt management operations.
    /// </summary>
    public IDebtRepository DebtRepository { get; }

    /// <summary>
    /// Service responsible for security features
    /// such as hashing and PIN verification.
    /// </summary>
    public SecurityService Security { get; }

    /// <summary>
    /// Authentication service used for login and registration.
    /// </summary>
    public IAuthService Auth { get; }

    /// <summary>
    /// Theme manager responsible for application appearance.
    /// </summary>
    public ThemeManager Theme { get; }

    /// <summary>
    /// Service responsible for first-time setup operations.
    /// </summary>
    public InitialSetupService Setup { get; }

    /// <summary>
    /// Initializes all repositories, services,
    /// and application dependencies.
    /// </summary>
    /// <param name="dbPath">
    /// Path to the SQLite database file.
    /// </param>
    public AppCompositionRoot(string dbPath)
    {
        // Create database helper
        Database = new SQLiteHelper(dbPath);

        // Initialize repositories
        InfrastructureRepository =
            new SqliteInfrastructureRepository(Database);

        UserRepository =
            new SqliteUserRepository(Database);

        BudgetCycleRepository =
            new SqliteBudgetCycleRepository(Database);

        ExpenseRepository =
            new SqliteExpenseRepository(Database);

        CategoryRepository =
            new SqliteCategoryRepository(Database);

        AuditRepository =
            new SqliteAuditRepository(Database);

        DebtRepository =
            new SqliteDebtRepository(Database);

        // Initialize security service
        Security = new SecurityService();

        // Create database tables and seed data
        InfrastructureRepository.InitializeDatabase(
            Security.HashPinSha256("1234"));

        // Initialize authentication service
        Auth = new AuthService(UserRepository, Security);

        // Initialize theme manager
        Theme = new ThemeManager();

        // Initialize first-time setup service
        Setup = new InitialSetupService(
            Auth,
            UserRepository,
            InfrastructureRepository,
            BudgetCycleRepository,
            AuditRepository);
    }
}
