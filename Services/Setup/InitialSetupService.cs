using Masroofy.App.Data.Repositories;
using Masroofy.App.Models;

namespace Masroofy.App.Services;

/// <summary>
/// Handles the application's first-run setup flow, including registering the initial user,
/// validating setup inputs, and creating the first budget cycle, all within a single
/// atomic transaction.
/// </summary>
public sealed class InitialSetupService
{
    private readonly IAuthService _auth;
    private readonly IUserRepository _users;
    private readonly IInfrastructureRepository _infra;
    private readonly IBudgetCycleRepository _cycles;
    private readonly IAuditRepository _audits;

    /// <summary>
    /// Initializes a new instance of <see cref="InitialSetupService"/> with all required
    /// dependencies for user registration and budget cycle creation.
    /// </summary>
    /// <param name="auth">The authentication service used to register the initial user.</param>
    /// <param name="users">The repository used to retrieve the newly created user record.</param>
    /// <param name="infra">Provides raw database connection creation for transactional operations.</param>
    /// <param name="cycles">The repository used to persist the initial budget cycle.</param>
    /// <param name="audits">The repository used to write the setup completion audit entry.</param>
    public InitialSetupService(
        IAuthService auth,
        IUserRepository users,
        IInfrastructureRepository infra,
        IBudgetCycleRepository cycles,
        IAuditRepository audits)
    {
        _auth = auth;
        _users = users;
        _infra = infra;
        _cycles = cycles;
        _audits = audits;
    }

    /// <summary>
    /// Determines whether the application has no registered users yet,
    /// indicating that the initial setup has not been completed.
    /// </summary>
    /// <returns>True if no users exist; otherwise false.</returns>
    public bool IsFirstRun() => _users.GetUserCount() == 0;

    /// <summary>
    /// Executes the full first-run setup by validating inputs, registering the initial user,
    /// and creating their first budget cycle. The cycle insert and audit entry are committed
    /// in a single atomic transaction. Returns false with a descriptive message if any step fails.
    /// </summary>
    /// <param name="userName">The username for the initial user account.</param>
    /// <param name="pin">The plain-text PIN for the initial user account.</param>
    /// <param name="initialBalance">The starting allowance for the first budget cycle; must be greater than zero.</param>
    /// <param name="cycleDays">The duration of the first budget cycle in days; must be greater than zero.</param>
    /// <param name="message">
    /// When the method returns, contains a success confirmation or a description of why setup failed.
    /// </param>
    /// <returns>True if setup completed successfully; otherwise false.</returns>
    public bool Setup(string userName, string pin, decimal initialBalance, int cycleDays, out string message)
    {
        if (initialBalance <= 0 || cycleDays <= 0)
        {
            message = "Initial balance and cycle duration must be positive.";
            return false;
        }
        if (!_auth.Register(userName, pin, "User", null, out message))
        {
            return false;
        }
        var user = _users.GetUserByName(userName);
        if (user == null)
        {
            message = "Could not load user after creation.";
            return false;
        }
        var start = DateTime.Today;
        var cycle = new BudgetCycle
        {
            UserId = user.Id,
            TotalAllowance = initialBalance,
            RemainingBalance = initialBalance,
            StartDate = start,
            EndDate = start.AddDays(cycleDays - 1),
            LastRolloverDate = start
        };
        using var connection = _infra.CreateConnection();
        connection.Open();
        using var tx = connection.BeginTransaction();
        _cycles.InsertCycle(cycle, tx);
        _audits.InsertAuditLog("Initial setup completed", tx);
        tx.Commit();
        message = "Setup completed.";
        return true;
    }
}
