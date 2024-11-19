using System;
using System.Windows.Forms;

namespace ParkingApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm());
        }
    }

    public class MainForm : Form
    {
        private Button driversButton;
        private Button busesButton;

        public MainForm()
        {
            this.Text = "Parking - Main";
            this.Width = 300;
            this.Height = 200;

            driversButton = new Button() { Text = "Driver management", Width = 200, Top = 30, Left = 40 };
            busesButton = new Button() { Text = "Bus management", Width = 200, Top = 80, Left = 40 };

            driversButton.Click += DriversButton_Click;
            busesButton.Click += BusesButton_Click;

            this.Controls.Add(driversButton);
            this.Controls.Add(busesButton);
        }

        private void DriversButton_Click(object sender, EventArgs e)
        {
            var driverForm = new DriverForm();
            driverForm.ShowDialog();
        }

        private void BusesButton_Click(object sender, EventArgs e)
        {
            var busForm = new BusForm();
            busForm.ShowDialog();
        }
    }
}
