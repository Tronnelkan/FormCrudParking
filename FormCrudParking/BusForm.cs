using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
using Npgsql;

namespace ParkingApp
{

    public class Bus
    {
        public int IdBus { get; set; }
        public string BusMark { get; set; }
        public string NumberSign { get; set; }
        public int TheDriver { get; set; }
        public int NumberOfSeats { get; set; }

        public string DriverName { get; set; }
    }

    public class BusForm : Form
    {
        private DataGridView busesGridView;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private string connectionString = "Host=localhost;Port=5433;Username=postgres;Password=tatarinn;Database=parking_db_2";

        public BusForm()
        {
            InitializeComponents();
            LoadBusesAsync();
        }

        private void InitializeComponents()
        {
            this.Text = "Bus Management";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            busesGridView = new DataGridView()
            {
                Dock = DockStyle.Top,
                Height = 450,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false
            };

            busesGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "ID",
                DataPropertyName = "IdBus",
                Width = 50
            });
            busesGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Bus Brand",
                DataPropertyName = "BusMark",
                Width = 150
            });
            busesGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "License Plate",
                DataPropertyName = "NumberSign",
                Width = 100
            });
            busesGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Driver",
                DataPropertyName = "DriverName",
                Width = 150
            });
            busesGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Number of Seats",
                DataPropertyName = "NumberOfSeats",
                Width = 100
            });

            addButton = new Button() { Text = "Add", Left = 50, Width = 100, Top = 460 };
            editButton = new Button() { Text = "Edit", Left = 160, Width = 100, Top = 460 };
            deleteButton = new Button() { Text = "Delete", Left = 270, Width = 100, Top = 460 };

            addButton.Click += AddButton_Click;
            editButton.Click += EditButton_Click;
            deleteButton.Click += DeleteButton_Click;

            this.Controls.Add(busesGridView);
            this.Controls.Add(addButton);
            this.Controls.Add(editButton);
            this.Controls.Add(deleteButton);
        }

        private async void LoadBusesAsync()
        {
            try
            {
                using (IDbConnection db = new NpgsqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            buses.id_bus AS IdBus,
                            buses.bus_mark AS BusMark,
                            buses.number_sign AS NumberSign,
                            drivers.firstname || ' ' || drivers.lastname AS DriverName,
                            buses.number_of_seats AS NumberOfSeats
                        FROM buses
                        INNER JOIN drivers ON buses.the_driver = drivers.id_driver
                        ORDER BY buses.id_bus";

                    var buses = await db.QueryAsync<Bus>(query);
                    busesGridView.DataSource = buses.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading buses: {ex.Message}");
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var busDetailsForm = new BusDetailsForm();
            if (busDetailsForm.ShowDialog() == DialogResult.OK)
            {
                LoadBusesAsync();
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (busesGridView.CurrentRow != null)
            {
                var selectedBus = (Bus)busesGridView.CurrentRow.DataBoundItem;
                var busDetailsForm = new BusDetailsForm(selectedBus.IdBus);
                if (busDetailsForm.ShowDialog() == DialogResult.OK)
                {
                    LoadBusesAsync();
                }
            }
            else
            {
                MessageBox.Show("Please select a bus to edit.");
            }
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            if (busesGridView.CurrentRow != null)
            {
                var selectedBus = (Bus)busesGridView.CurrentRow.DataBoundItem;
                var confirmResult = MessageBox.Show("Are you sure you want to delete this bus?", "Delete Confirmation", MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.Yes)
                {
                    try
                    {
                        using (IDbConnection db = new NpgsqlConnection(connectionString))
                        {
                            string query = "DELETE FROM buses WHERE id_bus = @IdBus";
                            await db.ExecuteAsync(query, new { IdBus = selectedBus.IdBus });
                            LoadBusesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting bus: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a bus to delete.");
            }
        }
    }

    public class BusDetailsForm : Form
    {
        private TextBox busMarkTextBox;
        private TextBox numberSignTextBox;
        private ComboBox driverComboBox;
        private TextBox seatsTextBox;
        private Button saveButton;
        private string connectionString = "Host=localhost;Port=5433;Username=postgres;Password=tatarinn;Database=parking_db_2";
        private bool isEditMode = false;
        private int busId;

        public BusDetailsForm(int? existingBusId = null)
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

        private async void InitializeDataAsync(int? existingBusId)
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

        private async Task LoadBusDetailsAsync(int busId)
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
                TheDriver = (int)driverComboBox.SelectedValue,
                NumberOfSeats = seats
            };

            try
            {
                using (IDbConnection db = new NpgsqlConnection(connectionString))
                {
                    if (isEditMode)
                    {
                        string query = "UPDATE buses SET bus_mark = @BusMark, number_sign = @NumberSign, the_driver = @TheDriver, number_of_seats = @NumberOfSeats WHERE id_bus = @IdBus";
                        await db.ExecuteAsync(query, new { bus.BusMark, bus.NumberSign, bus.TheDriver, bus.NumberOfSeats, IdBus = busId });
                    }
                    else
                    {
                        string query = "INSERT INTO buses (bus_mark, number_sign, the_driver, number_of_seats) VALUES (@BusMark, @NumberSign, @TheDriver, @NumberOfSeats)";
                        await db.ExecuteAsync(query, bus);
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
