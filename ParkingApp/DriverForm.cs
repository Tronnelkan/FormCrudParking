// DriverForm.cs

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
using Npgsql;

namespace BusManagementApp
{
    public class DriverForm : Form
    {
        private DataGridView driversGridView;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public DriverForm()
        {
            InitializeComponents();
            LoadDriversAsync();
        }

        private void InitializeComponents()
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
                HeaderText = "License Category",
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
                var driverDetailsForm = new DriverDetailsForm(selectedDriver.IdDriver);
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
}
