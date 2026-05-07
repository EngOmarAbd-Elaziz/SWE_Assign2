using Masroofy.App.Data.Repositories;
using Masroofy.App.Models;

namespace Masroofy.App.Services;

public sealed class InitialSetupService
{
    private readonly IAuthService _auth;
    private readonly IUserRepository _users;
    private readonly IInfrastructureRepository _infra;
    private readonly IBudgetCycleRepository _cycles;
    private readonly IAuditRepository _audits;

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

    public bool IsFirstRun() => _users.GetUserCount() == 0;

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
