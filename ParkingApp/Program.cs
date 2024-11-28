// Program.cs

using System;
using System.Windows.Forms;

namespace BusManagementApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Запуск Flyway міграцій (опціонально)
            // Можна додати код для запуску Flyway або виконати міграції окремо.

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm());
        }
    }
}
