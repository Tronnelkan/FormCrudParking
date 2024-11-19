using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
using Npgsql;

namespace ParkingApp
{
    // Класс сущности Driver
    public class Driver
    {
        public string IdDriver { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string CategoryDriverLicence { get; set; }
    }

    // Форма для управления водителями
    public class DriverForm : Form
    {
        private DataGridView driversGridView;
        private Button addButton;
        private Button editButton;
        private Button deleteButton;
        private string connectionString = "Host=localhost;Port=5433;Username=postgres;Password=tatarinn;Database=parking_db";

        public DriverForm()
        {
            this.Text = "Управление водителями";
            this.Width = 800;
            this.Height = 600;

            driversGridView = new DataGridView() { Dock = DockStyle.Top, Height = 450 };
            addButton = new Button() { Text = "Добавить", Left = 50, Width = 100, Top = 460 };
            editButton = new Button() { Text = "Редактировать", Left = 160, Width = 100, Top = 460 };
            deleteButton = new Button() { Text = "Удалить", Left = 270, Width = 100, Top = 460 };

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
            using (IDbConnection db = new NpgsqlConnection(connectionString))
            {
                var drivers = await db.QueryAsync<Driver>("SELECT * FROM drivers");
                driversGridView.DataSource = new List<Driver>(drivers);
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
                MessageBox.Show("Пожалуйста, выберите водителя для редактирования.");
            }
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            if (driversGridView.CurrentRow != null)
            {
                var selectedDriver = (Driver)driversGridView.CurrentRow.DataBoundItem;
                var confirmResult = MessageBox.Show("Вы уверены, что хотите удалить этого водителя?", "Подтверждение удаления", MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.Yes)
                {
                    using (IDbConnection db = new NpgsqlConnection(connectionString))
                    {
                        string query = "DELETE FROM drivers WHERE id_driver = @IdDriver";
                        await db.ExecuteAsync(query, new { IdDriver = selectedDriver.IdDriver });
                        LoadDriversAsync();
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите водителя для удаления.");
            }
        }
    }

    // Форма для добавления/редактирования водителя
    public class DriverDetailsForm : Form
    {
        private TextBox idTextBox;
        private TextBox firstNameTextBox;
        private TextBox lastNameTextBox;
        private TextBox ageTextBox;
        private TextBox categoryTextBox;
        private Button saveButton;
        private string connectionString = "Host=localhost;Port=5433;Username=postgres;Password=tatarinn;Database=parking_db";
        private bool isEditMode = false;

        public DriverDetailsForm(Driver driver = null)
        {
            this.Text = driver == null ? "Добавить водителя" : "Редактировать водителя";
            this.Width = 400;
            this.Height = 300;

            Label idLabel = new Label() { Text = "ID водителя:", Left = 20, Top = 20 };
            idTextBox = new TextBox() { Left = 150, Top = 20, Width = 200 };

            Label firstNameLabel = new Label() { Text = "Имя:", Left = 20, Top = 60 };
            firstNameTextBox = new TextBox() { Left = 150, Top = 60, Width = 200 };

            Label lastNameLabel = new Label() { Text = "Фамилия:", Left = 20, Top = 100 };
            lastNameTextBox = new TextBox() { Left = 150, Top = 100, Width = 200 };

            Label ageLabel = new Label() { Text = "Возраст:", Left = 20, Top = 140 };
            ageTextBox = new TextBox() { Left = 150, Top = 140, Width = 200 };

            Label categoryLabel = new Label() { Text = "Категория прав:", Left = 20, Top = 180 };
            categoryTextBox = new TextBox() { Left = 150, Top = 180, Width = 200 };

            saveButton = new Button() { Text = "Сохранить", Left = 150, Top = 220, Width = 100 };
            saveButton.Click += SaveButton_Click;

            this.Controls.Add(idLabel);
            this.Controls.Add(idTextBox);
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
                idTextBox.Text = driver.IdDriver;
                idTextBox.Enabled = false; // ID нельзя изменять
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
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            if (!int.TryParse(ageTextBox.Text, out int age))
            {
                MessageBox.Show("Возраст должен быть числом.");
                return;
            }

            var driver = new Driver
            {
                IdDriver = idTextBox.Text.Trim(),
                FirstName = firstNameTextBox.Text.Trim(),
                LastName = lastNameTextBox.Text.Trim(),
                Age = age,
                CategoryDriverLicence = categoryTextBox.Text.Trim()
            };

            using (IDbConnection db = new NpgsqlConnection(connectionString))
            {
                if (isEditMode)
                {
                    string query = "UPDATE drivers SET firstname = @FirstName, lastname = @LastName, age = @Age, category_driver_licence = @CategoryDriverLicence WHERE id_driver = @IdDriver";
                    await db.ExecuteAsync(query, driver);
                }
                else
                {
                    string query = "INSERT INTO drivers (id_driver, firstname, lastname, age, category_driver_licence) VALUES (@IdDriver, @FirstName, @LastName, @Age, @CategoryDriverLicence)";
                    await db.ExecuteAsync(query, driver);
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
