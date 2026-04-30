using Masroofy.App.Controllers;
using Masroofy.App.Data;
using Masroofy.App.Services;
using Masroofy.App.Services.Composition;
using Masroofy.App.Strategies;
using Masroofy.App.Views.Forms;

namespace Masroofy.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "masroofy.db");
        var composition = new AppCompositionRoot(dbPath);
        if (composition.Setup.IsFirstRun())
        {
            using var setup = new SetupViewForm(composition.Setup);
            if (setup.ShowDialog() != DialogResult.OK)
            {
                return;
            }
        }

        var users = composition.UserRepository.GetUsers().Select(x => x.Name).ToList();
        using var login = new LoginViewForm(composition.Auth, users);
        if (login.ShowDialog() != DialogResult.OK || login.AuthenticatedUser == null)
        {
            return;
        }

        var controller = new AppController(
            composition.InfrastructureRepository,
            composition.UserRepository,
            composition.BudgetCycleRepository,
            composition.ExpenseRepository,
            composition.CategoryRepository,
            composition.AuditRepository,
            composition.DebtRepository,
            new NormalStrategy());
        controller.SetCurrentUser(login.AuthenticatedUser);
        Application.Run(new MainForm(controller, composition.Theme));
    }
}