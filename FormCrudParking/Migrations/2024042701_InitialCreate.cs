using FluentMigrator;

namespace YourNamespace.Migrations
{
    [Migration(2024042701)]
    public class InitialCreate : Migration
    {
        public override void Up()
        {
            // Створення послідовностей
            Create.Sequence("destination_id_seq").StartWith(100000).IncrementBy(1);
            Create.Sequence("days_id_seq").StartWith(100000).IncrementBy(1);
            Create.Sequence("drivers_id_seq").StartWith(100000).IncrementBy(1);
            Create.Sequence("buses_id_seq").StartWith(100000).IncrementBy(1);
            Create.Sequence("routes_id_seq").StartWith(100000).IncrementBy(1);
            Create.Sequence("busstop_id_seq").StartWith(100000).IncrementBy(1);
            Create.Sequence("flights_id_seq").StartWith(100000).IncrementBy(1);

            // Створення таблиці destination
            Create.Table("destination")
                .WithColumn("id_destination").AsInt64().PrimaryKey().WithDefault(SystemMethods.NextVal("destination_id_seq"))
                .WithColumn("name_destination").AsString(100).NotNullable().Unique();

            // Створення таблиці days
            Create.Table("days")
                .WithColumn("id_day").AsInt64().PrimaryKey().WithDefault(SystemMethods.NextVal("days_id_seq"))
                .WithColumn("name_days").AsString(50).NotNullable().Unique();

            // Створення таблиці drivers
            Create.Table("drivers")
                .WithColumn("id_driver").AsInt64().PrimaryKey().WithDefault(SystemMethods.NextVal("drivers_id_seq"))
                .WithColumn("lastname").AsString(100).NotNullable()
                .WithColumn("firstname").AsString(100).NotNullable()
                .WithColumn("age").AsInt32().NotNullable().WithDefaultValue(18).WithColumnDescription("Мінімальний вік 18").WithCheckConstraint("age >= 18")
                .WithColumn("category_driver_licence").AsString(10).NotNullable();

            // Створення таблиці buses
            Create.Table("buses")
                .WithColumn("id_bus").AsInt64().PrimaryKey().WithDefault(SystemMethods.NextVal("buses_id_seq"))
                .WithColumn("bus_mark").AsString(50).NotNullable()
                .WithColumn("number_sign").AsString(20).NotNullable().Unique()
                .WithColumn("the_driver").AsInt64().NotNullable().ForeignKey("FK_buses_drivers", "drivers", "id_driver").OnDelete(System.Data.Rule.Cascade)
                .WithColumn("number_of_seats").AsInt32().NotNullable().WithCheckConstraint("number_of_seats > 0");

            // Створення таблиці routes
            Create.Table("routes")
                .WithColumn("id_route").AsInt64().PrimaryKey().WithDefault(SystemMethods.NextVal("routes_id_seq"))
                .WithColumn("point_of_departure").AsString(100).NotNullable()
                .WithColumn("destination").AsInt64().NotNullable().ForeignKey("FK_routes_destination", "destination", "id_destination").OnDelete(System.Data.Rule.Cascade)
                .WithColumn("days").AsInt64().NotNullable().ForeignKey("FK_routes_days", "days", "id_day").OnDelete(System.Data.Rule.Cascade)
                .WithColumn("departure_time").AsTime().NotNullable();

            // Створення таблиці busstop
            Create.Table("busstop")
                .WithColumn("id_busstop").AsInt64().PrimaryKey().WithDefault(SystemMethods.NextVal("busstop_id_seq"))
                .WithColumn("name_busstop").AsString(100).NotNullable()
                .WithColumn("route").AsInt64().NotNullable().ForeignKey("FK_busstop_routes", "routes", "id_route").OnDelete(System.Data.Rule.Cascade);

            // Створення таблиці flights
            Create.Table("flights")
                .WithColumn("id_flight").AsInt64().PrimaryKey().WithDefault(SystemMethods.NextVal("flights_id_seq"))
                .WithColumn("departure_time").AsTime().NotNullable()
                .WithColumn("arrival_time").AsTime().NotNullable()
                .WithColumn("route").AsInt64().NotNullable().ForeignKey("FK_flights_routes", "routes", "id_route").OnDelete(System.Data.Rule.Cascade)
                .WithColumn("bus").AsInt64().NotNullable().ForeignKey("FK_flights_buses", "buses", "id_bus").OnDelete(System.Data.Rule.Cascade)
                .WithColumn("ticket_price").AsInt32().NotNullable().WithCheckConstraint("ticket_price > 0");
        }

        public override void Down()
        {
            // Видалення таблиць у зворотному порядку залежностей
            Delete.Table("flights");
            Delete.Table("busstop");
            Delete.Table("routes");
            Delete.Table("buses");
            Delete.Table("drivers");
            Delete.Table("days");
            Delete.Table("destination");

            // Видалення послідовностей
            Delete.Sequence("flights_id_seq");
            Delete.Sequence("busstop_id_seq");
            Delete.Sequence("routes_id_seq");
            Delete.Sequence("buses_id_seq");
            Delete.Sequence("drivers_id_seq");
            Delete.Sequence("days_id_seq");
            Delete.Sequence("destination_id_seq");
        }
    }
}
