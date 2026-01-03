# Entity Framework Core & Clean Architecture Workflow

This guide documents the step-by-step process of building a robust data layer using .NET 8, Entity Framework Core, and PostgreSQL in a Clean Architecture solution.

## 1. Creating Domain Entities

In **Clean Architecture**, entities live in the **Domain Layer**. They are pure C# classes that represent business objects. They should have **no dependencies** on the database or external libraries.

### Key Principles

- **Encapsulation:** Use `private set` for properties to prevent invalid states.
- **Validation:** Use constructors to enforce rules (e.g., "Price cannot be negative").
- **Rich Models:** Include logic inside the entity (e.g., `CalculateTotal()`), not just data.

### Navigation Properties

Navigation properties are the "Magic Links" that allow you to travel between related objects in C# without writing SQL joins.

- **Reference Navigation (Parent):** Links to a single related object.
- **Collection Navigation (Children):** Links to a list of related objects.

#### Example Code: `Room.cs`

```csharp
public class Room
{
    // 1. The Primary Key
    public Guid Id { get; private set; }

    // 2. The Foreign Key (The raw data link)
    public Guid RoomTypeId { get; private set; }

    // 3. The Navigation Property (The object link)
    // Allows access like: myRoom.RoomType.Name
    public RoomType? RoomType { get; private set; }

    // 4. Encapsulation & Validation
    public Room(Guid roomTypeId, int roomNumber)
    {
        Id = Guid.NewGuid(); // Entity generates its own ID
        RoomTypeId = roomTypeId;
        RoomNumber = roomNumber;
    }
}
```

## 2\. Configuring DbContext (Persistence Layer)

Instead of cluttering the main `DbContext` file with thousands of lines of configuration, we use **`IEntityTypeConfiguration<T>`**. This allows us to create a separate configuration file for every single table.

### Why do this?

- **Organization:** Keeps the `DbContext` clean.
- **Scalability:** Easy to manage 50+ tables.
- **Separation of Concerns:** Each file focuses on one entity.

### The Fluent API

We use the Fluent API to translate C\# properties into SQL rules.

#### Example Code: `RoomConfiguration.cs`

```csharp
public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        // 1. Primary Key
        builder.HasKey(r => r.Id);

        // 2. Property Constraints
        builder.Property(r => r.RoomNumber)
            .IsRequired();

        // 3. Relationships
        // "A Room has ONE RoomType... which has MANY Rooms"
        builder.HasOne(r => r.RoomType)
            .WithMany(rt => rt.Rooms)
            .HasForeignKey(r => r.RoomTypeId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletions
    }
}
```

### Wiring it up

In your main `HotelDbContext.cs`, you tell EF Core to automatically find and apply all these configuration files:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Scans the assembly and applies all IEntityTypeConfiguration classes found
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
}
```

---

## 3\. Migrations & Database Updates

Migrations act as version control for your database schema. They calculate the difference between your **C\# Code** and the **Current Database State**.

### Prerequisites

1.  **Connection String:** Defined in `src/Api/appsettings.json`.
2.  **Dependency Injection:** `DbContext` registered in `src/Api/Program.cs`.

### The "Add Migration" Command

This generates the C\# files describing the changes. Run this from the **Solution Root**.

```bash
dotnet ef migrations add InitialCreate --project src/Infrastructure --startup-project src/Api --output-dir Persistence/Migrations
```

**Command Breakdown:**

- `migrations add InitialCreate`: Creates a new migration named "InitialCreate".
- `--project src/Infrastructure`: Tells EF Core that the **DbContext** (Logic) lives in the Infrastructure project.
- `--startup-project src/Api`: Tells EF Core to run the **API** project to find the Connection String and Configuration.
- `--output-dir ...`: Organizes generated files into a specific folder.

### The "Update Database" Command

This executes the SQL commands to create/update tables in the actual PostgreSQL database running in Docker.

```bash
dotnet ef database update --project src/Infrastructure --startup-project src/Api
```

**What happens:**

1.  Reads connection string from API.
2.  Connects to Postgres.
3.  Checks `__EFMigrationsHistory` table.
4.  Applies any pending migrations (SQL commands).

<!-- end list -->
