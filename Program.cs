using Masroofy.App.Controllers;
using Masroofy.App.Data;
using Masroofy.App.Services;
using Masroofy.App.Services.Composition;
using Masroofy.App.Strategies;
using Masroofy.App.Views.Forms;

namespace Masroofy.App;

/// <summary>
/// Application entry point for Masroofy system.
/// Responsible for initializing dependencies, handling first run setup,
/// authentication flow, and launching the main application window.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Main entry method of the application.
    /// Initializes database, composition root, setup flow, login flow,
    /// and finally starts the main UI with an authenticated controller.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "masroofy.db");
        var composition = new AppCompositionRoot(dbPath);

        // First run setup (database initialization / user setup)
        if (composition.Setup.IsFirstRun())
        {
            using var setup = new SetupViewForm(composition.Setup);
            if (setup.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        }

        // Load existing users for authentication
        var users = composition.UserRepository.GetUsers().Select(x => x.Name).ToList();

        // Login screen
        using var login = new LoginViewForm(composition.Auth, users);
        if (login.ShowDialog() != DialogResult.OK || login.AuthenticatedUser == null)
        {
            return;
        }

        // Build application controller with all dependencies (DI composition root)
        var controller = new AppController(
            composition.InfrastructureRepository,
            composition.UserRepository,
            composition.BudgetCycleRepository,
            composition.ExpenseRepository,
            composition.CategoryRepository,
            composition.AuditRepository,
            composition.DebtRepository,
            new NormalStrategy());

        // Attach authenticated user session
        controller.SetCurrentUser(login.AuthenticatedUser);

        // Start main application window
        Application.Run(new MainForm(controller, composition.Theme));
    }
}
