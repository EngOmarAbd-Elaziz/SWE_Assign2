using Masroofy.App.Data.Repositories;
using Masroofy.App.Models;

namespace Masroofy.App.Services;

/// <summary>
/// Handles first-time application setup operations
/// such as creating the first user and
/// initializing the first budget cycle.
/// </summary>
public sealed class InitialSetupService
{
    /// <summary>
    /// Authentication service used for registration.
    /// </summary>
    private readonly IAuthService _auth;

    /// <summary>
    /// Repository responsible for user operations.
    /// </summary>
    private readonly IUserRepository _users;

    /// <summary>
    /// Repository responsible for infrastructure
    /// and database operations.
    /// </summary>
    private readonly IInfrastructureRepository _infra;

    /// <summary>
    /// Repository responsible for budget cycle operations.
    /// </summary>
    private readonly IBudgetCycleRepository _cycles;

    /// <summary>
    /// Repository responsible for audit logging.
    /// </summary>
    private readonly IAuditRepository _audits;

    /// <summary>
    /// Initializes the setup service dependencies.
    /// </summary>
    /// <param name="auth">
    /// Authentication service instance.
    /// </param>
    /// <param name="users">
    /// User repository instance.
    /// </param>
    /// <param name="infra">
    /// Infrastructure repository instance.
    /// </param>
    /// <param name="cycles">
    /// Budget cycle repository instance.
    /// </param>
    /// <param name="audits">
    /// Audit repository instance.
    /// </param>
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
    /// Checks whether the application
    /// is running for the first time.
    /// </summary>
    /// <returns>
    /// True if no users exist; otherwise false.
    /// </returns>
    public bool IsFirstRun() => _users.GetUserCount() == 0;

    /// <summary>
    /// Performs the initial application setup.
    /// Creates the first user and
    /// initializes the first budget cycle.
    /// </summary>
    /// <param name="userName">
    /// Name of the first user.
    /// </param>
    /// <param name="pin">
    /// User PIN code.
    /// </param>
    /// <param name="initialBalance">
    /// Initial budget balance.
    /// </param>
    /// <param name="cycleDays">
    /// Number of days in the budget cycle.
    /// </param>
    /// <param name="message">
    /// Output setup result message.
    /// </param>
    /// <returns>
    /// True if setup succeeds; otherwise false.
    /// </returns>
    public bool Setup(
        string userName,
        string pin,
        decimal initialBalance,
        int cycleDays,
        out string message)
    {
        // Validate balance and cycle duration
        if (initialBalance <= 0 || cycleDays <= 0)
        {
            message =
                "Initial balance and cycle duration must be positive.";

            return false;
        }

        // Register first user
        if (!_auth.Register(
            userName,
            pin,
            "User",
            null,
            out message))
        {
            return false;
        }

        // Load created user
        var user = _users.GetUserByName(userName);

        if (user == null)
        {
            message = "Could not load user after creation.";
            return false;
        }

        // Create initial budget cycle
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

        // Save cycle and audit log inside transaction
        using var connection = _infra.CreateConnection();

        connection.Open();

        using var tx = connection.BeginTransaction();

        _cycles.InsertCycle(cycle, tx);

        _audits.InsertAuditLog(
            "Initial setup completed",
            tx);

        tx.Commit();

        message = "Setup completed.";

        return true;
    }
}
