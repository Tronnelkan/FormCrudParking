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
            // ������ Flyway ������� (�����������)
            // ����� ������ ��� ��� ������� Flyway ��� �������� ������� ������.

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm());
        }
    }
}
