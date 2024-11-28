// BusDetailsForm.cs

using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
using Npgsql;
using System.Configuration;

namespace BusManagementApp
{
    public class BusDetailsForm : Form
    {
        private TextBox busMarkTextBox;
        private TextBox numberSignTextBox;
        private ComboBox driverComboBox;
        private TextBox seatsTextBox;
        private Button saveButton;
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        private bool isEditMode = false;
        private long busId;

        public BusDetailsForm(long? existingBusId = null)
        {
            InitializeComponents();
            InitializeDataAsync(existingBusId);
        }

        private void InitializeComponents()
        {
            this.Text = "Add Bus";
            this.Width = 400;
            this.Height = 350;
            this.StartPosition = FormStartPosition.CenterParent;

            Label busMarkLabel = new Label() { Text = "Bus Brand:", Left = 20, Top = 20 };
            busMarkTextBox = new TextBox() { Left = 150, Top = 20, Width = 200 };

            Label numberSignLabel = new Label() { Text = "License Plate:", Left = 20, Top = 60 };
            numberSignTextBox = new TextBox() { Left = 150, Top = 60, Width = 200 };

            Label driverLabel = new Label() { Text = "Driver:", Left = 20, Top = 100 };
            driverComboBox = new ComboBox() { Left = 150, Top = 100, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

            Label seatsLabel = new Label() { Text = "Number of Seats:", Left = 20, Top = 140 };
            seatsTextBox = new TextBox() { Left = 150, Top = 140, Width = 200 };

            saveButton = new Button() { Text = "Save", Left = 150, Top = 180, Width = 100 };
            saveButton.Click += SaveButton_Click;

            this.Controls.Add(busMarkLabel);
            this.Controls.Add(busMarkTextBox);
            this.Controls.Add(numberSignLabel);
            this.Controls.Add(numberSignTextBox);
            this.Controls.Add(driverLabel);
            this.Controls.Add(driverComboBox);
            this.Controls.Add(seatsLabel);
            this.Controls.Add(seatsTextBox);
            this.Controls.Add(saveButton);
        }

        private async void InitializeDataAsync(long? existingBusId)
        {
            await LoadDriversAsync();

            if (existingBusId != null)
            {
                isEditMode = true;
                busId = existingBusId.Value;
                await LoadBusDetailsAsync(busId);
                this.Text = "Edit Bus";
            }
        }

        private async Task LoadDriversAsync()
        {
            try
            {
                using (IDbConnection db = new NpgsqlConnection(connectionString))
                {
                    var drivers = await db.QueryAsync<Driver>(
                        "SELECT id_driver AS IdDriver, firstname AS FirstName, lastname AS LastName FROM drivers ORDER BY id_driver"
                    );
                    var driverList = drivers.Select(d => new
                    {
                        d.IdDriver,
                        FullName = $"{d.FirstName} {d.LastName}"
                    }).ToList();

                    driverComboBox.DataSource = driverList;
                    driverComboBox.DisplayMember = "FullName";
                    driverComboBox.ValueMember = "IdDriver";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading drivers: {ex.Message}");
            }
        }

        private async Task LoadBusDetailsAsync(long busId)
        {
            try
            {
                using (IDbConnection db = new NpgsqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            id_bus AS IdBus,
                            bus_mark AS BusMark,
                            number_sign AS NumberSign,
                            the_driver AS TheDriver,
                            number_of_seats AS NumberOfSeats
                        FROM buses
                        WHERE id_bus = @IdBus";

                    var bus = await db.QueryFirstOrDefaultAsync<Bus>(query, new { IdBus = busId });

                    if (bus != null)
                    {
                        busMarkTextBox.Text = bus.BusMark;
                        numberSignTextBox.Text = bus.NumberSign;
                        driverComboBox.SelectedValue = bus.TheDriver;
                        seatsTextBox.Text = bus.NumberOfSeats.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Bus not found.");
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading bus details: {ex.Message}");
            }
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(busMarkTextBox.Text) ||
                string.IsNullOrWhiteSpace(numberSignTextBox.Text) ||
                driverComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(seatsTextBox.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (!int.TryParse(seatsTextBox.Text, out int seats))
            {
                MessageBox.Show("Number of seats must be a number.");
                return;
            }

            var bus = new Bus
            {
                BusMark = busMarkTextBox.Text.Trim(),
                NumberSign = numberSignTextBox.Text.Trim(),
                TheDriver = (long)driverComboBox.SelectedValue,
                NumberOfSeats = seats
            };

            try
            {
                using (IDbConnection db = new NpgsqlConnection(connectionString))
                {
                    if (isEditMode)
                    {
                        // Використання збереженої процедури для оновлення автобуса
                        string proc = "update_bus";
                        var parameters = new DynamicParameters();
                        parameters.Add("p_id_bus", busId);
                        parameters.Add("p_bus_mark", bus.BusMark);
                        parameters.Add("p_number_sign", bus.NumberSign);
                        parameters.Add("p_the_driver", bus.TheDriver);
                        parameters.Add("p_number_of_seats", bus.NumberOfSeats);

                        await db.ExecuteAsync(proc, parameters, commandType: CommandType.StoredProcedure);
                    }
                    else
                    {
                        // Використання збереженої процедури для додавання автобуса
                        string proc = "add_bus";
                        var parameters = new DynamicParameters();
                        parameters.Add("p_bus_mark", bus.BusMark);
                        parameters.Add("p_number_sign", bus.NumberSign);
                        parameters.Add("p_the_driver", bus.TheDriver);
                        parameters.Add("p_number_of_seats", bus.NumberOfSeats);

                        await db.ExecuteAsync(proc, parameters, commandType: CommandType.StoredProcedure);
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving bus: {ex.Message}");
            }
        }
    }
}
