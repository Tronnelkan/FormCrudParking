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
    public class Driver
    {
        public int IdDriver { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string CategoryDriverLicence { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    public class DriverForm : Form
    {
        private DataGridView driversGridView;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private string connectionString = "Host=localhost;Port=5433;Username=postgres;Password=tatarinn;Database=parking_db_2";

        public DriverForm()
        {
            this.Text = "Driver Management";
            this.Width = 800;
            this.Height = 600;
            this.StartPosition = FormStartPosition.CenterScreen;

            driversGridView = new DataGridView()
            {
                Dock = DockStyle.Top,
                Height = 450,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false
            };

            driversGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "ID",
                DataPropertyName = "IdDriver",
                Width = 50
            });
            driversGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "First Name",
                DataPropertyName = "FirstName",
                Width = 150
            });
            driversGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Last Name",
                DataPropertyName = "LastName",
                Width = 150
            });
            driversGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Age",
                DataPropertyName = "Age",
                Width = 50
            });
            driversGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Driver License Category",
                DataPropertyName = "CategoryDriverLicence",
                Width = 100
            });

            addButton = new Button() { Text = "Add", Left = 50, Width = 100, Top = 460 };
            editButton = new Button() { Text = "Edit", Left = 160, Width = 100, Top = 460 };
            deleteButton = new Button() { Text = "Delete", Left = 270, Width = 100, Top = 460 };

            addButton.Click += AddButton_Click;
            editButton.Click += EditButton_Click;
            deleteButton.Click += DeleteButton_Click;

            this.Controls.Add(driversGridView);
            this.Controls.Add(addButton);
            this.Controls.Add(editButton);
            this.Controls.Add(deleteButton);

            LoadDriversAsync();
        }

        private async void LoadDriversAsync()
        {
            try
            {
                using (IDbConnection db = new NpgsqlConnection(connectionString))
                {
                    var drivers = await db.QueryAsync<Driver>(
                        "SELECT id_driver AS IdDriver, firstname AS FirstName, lastname AS LastName, age AS Age, category_driver_licence AS CategoryDriverLicence FROM drivers ORDER BY id_driver"
                    );
                    driversGridView.DataSource = drivers.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading drivers: {ex.Message}");
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var driverDetailsForm = new DriverDetailsForm();
            if (driverDetailsForm.ShowDialog() == DialogResult.OK)
            {
                LoadDriversAsync();
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (driversGridView.CurrentRow != null)
            {
                var selectedDriver = (Driver)driversGridView.CurrentRow.DataBoundItem;
                var driverDetailsForm = new DriverDetailsForm(selectedDriver);
                if (driverDetailsForm.ShowDialog() == DialogResult.OK)
                {
                    LoadDriversAsync();
                }
            }
            else
            {
                MessageBox.Show("Please select a driver to edit.");
            }
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            if (driversGridView.CurrentRow != null)
            {
                var selectedDriver = (Driver)driversGridView.CurrentRow.DataBoundItem;
                var confirmResult = MessageBox.Show("Are you sure you want to delete this driver?", "Delete Confirmation", MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.Yes)
                {
                    try
                    {
                        using (IDbConnection db = new NpgsqlConnection(connectionString))
                        {
                            string query = "DELETE FROM drivers WHERE id_driver = @IdDriver";
                            await db.ExecuteAsync(query, new { IdDriver = selectedDriver.IdDriver });
                            LoadDriversAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting driver: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a driver to delete.");
            }
        }
    }

    public class DriverDetailsForm : Form
    {
        private TextBox firstNameTextBox;
        private TextBox lastNameTextBox;
        private TextBox ageTextBox;
        private TextBox categoryTextBox;
        private Button saveButton;
        private string connectionString = "Host=localhost;Port=5433;Username=postgres;Password=tatarinn;Database=parking_db_2";
        private bool isEditMode = false;
        private int driverId;

        public DriverDetailsForm(Driver driver = null)
        {
            this.Text = driver == null ? "Add Driver" : "Edit Driver";
            this.Width = 400;
            this.Height = 300;
            this.StartPosition = FormStartPosition.CenterParent;

            Label firstNameLabel = new Label() { Text = "First Name:", Left = 20, Top = 20 };
            firstNameTextBox = new TextBox() { Left = 150, Top = 20, Width = 200 };

            Label lastNameLabel = new Label() { Text = "Last Name:", Left = 20, Top = 60 };
            lastNameTextBox = new TextBox() { Left = 150, Top = 60, Width = 200 };

            Label ageLabel = new Label() { Text = "Age:", Left = 20, Top = 100 };
            ageTextBox = new TextBox() { Left = 150, Top = 100, Width = 200 };

            Label categoryLabel = new Label() { Text = "License Category:", Left = 20, Top = 140 };
            categoryTextBox = new TextBox() { Left = 150, Top = 140, Width = 200 };

            saveButton = new Button() { Text = "Save", Left = 150, Top = 180, Width = 100 };
            saveButton.Click += SaveButton_Click;

            this.Controls.Add(firstNameLabel);
            this.Controls.Add(firstNameTextBox);
            this.Controls.Add(lastNameLabel);
            this.Controls.Add(lastNameTextBox);
            this.Controls.Add(ageLabel);
            this.Controls.Add(ageTextBox);
            this.Controls.Add(categoryLabel);
            this.Controls.Add(categoryTextBox);
            this.Controls.Add(saveButton);

            if (driver != null)
            {
                isEditMode = true;
                driverId = driver.IdDriver;
                firstNameTextBox.Text = driver.FirstName;
                lastNameTextBox.Text = driver.LastName;
                ageTextBox.Text = driver.Age.ToString();
                categoryTextBox.Text = driver.CategoryDriverLicence;
            }
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(firstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(lastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(ageTextBox.Text) ||
                string.IsNullOrWhiteSpace(categoryTextBox.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            if (!int.TryParse(ageTextBox.Text, out int age))
            {
                MessageBox.Show("Age must be a number.");
                return;
            }

            var driver = new Driver
            {
                FirstName = firstNameTextBox.Text.Trim(),
                LastName = lastNameTextBox.Text.Trim(),
                Age = age,
                CategoryDriverLicence = categoryTextBox.Text.Trim()
            };

            try
            {
                using (IDbConnection db = new NpgsqlConnection(connectionString))
                {
                    if (isEditMode)
                    {
                        string query = "UPDATE drivers SET firstname = @FirstName, lastname = @LastName, age = @Age, category_driver_licence = @CategoryDriverLicence WHERE id_driver = @IdDriver";
                        await db.ExecuteAsync(query, new { driver.FirstName, driver.LastName, driver.Age, driver.CategoryDriverLicence, IdDriver = driverId });
                    }
                    else
                    {
                        string query = "INSERT INTO drivers (firstname, lastname, age, category_driver_licence) VALUES (@FirstName, @LastName, @Age, @CategoryDriverLicence)";
                        await db.ExecuteAsync(query, driver);
                    }
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving driver: {ex.Message}");
            }
        }
    }
}
