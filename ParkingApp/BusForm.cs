// BusForm.cs

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
    public class BusForm : Form
    {
        private DataGridView busesGridView;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

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
                    b.id_bus AS IdBus,
                    b.bus_mark AS BusMark,
                    b.number_sign AS NumberSign,
                    d.firstname || ' ' || d.lastname AS DriverName,
                    b.number_of_seats AS NumberOfSeats
                FROM buses b
                INNER JOIN drivers d ON b.the_driver = d.id_driver
                ORDER BY b.id_bus";

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
}
