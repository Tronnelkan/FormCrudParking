// Models.cs

using System.Collections.Generic;

namespace BusManagementApp
{
    public class Driver
    {
        public long IdDriver { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string CategoryDriverLicence { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        // Навігаційні властивості (якщо необхідно)
        public ICollection<Bus> Buses { get; set; }
    }

    public class Bus
    {
        public long IdBus { get; set; }
        public string BusMark { get; set; }
        public string NumberSign { get; set; }
        public long TheDriver { get; set; }
        public int NumberOfSeats { get; set; }

        // Навігаційні властивості
        public Driver Driver { get; set; }
    }
}
