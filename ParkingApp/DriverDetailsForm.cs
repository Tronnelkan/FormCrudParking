// DriverDetailsForm.cs

using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
using Npgsql;

namespace BusManagementApp
{
    public class DriverDetailsForm : Form
    {
        private TextBox firstNameTextBox;
        private TextBox lastNameTextBox;
        private TextBox ageTextBox;
        private TextBox categoryTextBox;
        private Button saveButton;
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        private bool isEditMode = false;
        private long driverId;

        public DriverDetailsForm(long? existingDriverId = null)
        {
            InitializeComponents();
            InitializeDataAsync(existingDriverId);
        }

        private void InitializeComponents()
        {
            this.Text = "Add Driver";
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
        }

        private async void InitializeDataAsync(long? existingDriverId)
        {
            if (existingDriverId != null)
            {
                isEditMode = true;
                driverId = existingDriverId.Value;
                await LoadDriverDetailsAsync(driverId);
                this.Text = "Edit Driver";
            }
        }

        private async Task LoadDriverDetailsAsync(long driverId)
        {
            try
            {
                using (IDbConnection db = new NpgsqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            id_driver AS IdDriver,
                            firstname AS FirstName,
                            lastname AS LastName,
                            age AS Age,
                            category_driver_licence AS CategoryDriverLicence
                        FROM drivers
                        WHERE id_driver = @IdDriver";

                    var driver = await db.QueryFirstOrDefaultAsync<Driver>(query, new { IdDriver = driverId });

                    if (driver != null)
                    {
                        firstNameTextBox.Text = driver.FirstName;
                        lastNameTextBox.Text = driver.LastName;
                        ageTextBox.Text = driver.Age.ToString();
                        categoryTextBox.Text = driver.CategoryDriverLicence;
                    }
                    else
                    {
                        MessageBox.Show("Driver not found.");
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading driver details: {ex.Message}");
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
                        // Використання збереженої процедури для оновлення водія
                        string proc = "update_driver";
                        var parameters = new DynamicParameters();
                        parameters.Add("p_id_driver", driverId);
                        parameters.Add("p_firstname", driver.FirstName);
                        parameters.Add("p_lastname", driver.LastName);
                        parameters.Add("p_age", driver.Age);
                        parameters.Add("p_category_driver_licence", driver.CategoryDriverLicence);

                        await db.ExecuteAsync(proc, parameters, commandType: CommandType.StoredProcedure);
                    }
                    else
                    {
                        // Використання збереженої процедури для додавання водія
                        string proc = "add_driver";
                        var parameters = new DynamicParameters();
                        parameters.Add("p_firstname", driver.FirstName);
                        parameters.Add("p_lastname", driver.LastName);
                        parameters.Add("p_age", driver.Age);
                        parameters.Add("p_category_driver_licence", driver.CategoryDriverLicence);

                        await db.ExecuteAsync(proc, parameters, commandType: CommandType.StoredProcedure);
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
