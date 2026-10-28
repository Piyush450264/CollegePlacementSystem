# Local setup — CollegePlacementSystem

## 1. Configure local secrets

Open a terminal in the folder containing `CollegePlacementSystem.csproj` and run:

```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=college_placement_db;User=YOUR_MYSQL_USER;Password=YOUR_MYSQL_PASSWORD;"
dotnet user-secrets set "AdminAccount:Email" "admin@college.com"
dotnet user-secrets set "AdminAccount:Password" "REPLACE_WITH_A_STRONG_PASSWORD"
```

Replace `YOUR_MYSQL_USER`, `YOUR_MYSQL_PASSWORD`, and `REPLACE_WITH_A_STRONG_PASSWORD` with your own local values. The administrator password must satisfy the password rules configured in `Program.cs`: at least 8 characters, uppercase, lowercase, digit, and non-alphanumeric character.

Do not share your secrets or put them in `appsettings.json`. If the password previously included in your project was a real password, change it in MySQL as well.

## 2. Confirm MySQL is ready

Ensure MySQL is running and the database `college_placement_db` exists. Apply migrations if needed:

```powershell
dotnet ef database update
```

If `dotnet ef` is unavailable, install the tool with `dotnet tool install --global dotnet-ef` (or update it if already installed).

## 3. Build and run

```powershell
dotnet restore
dotnet build
dotnet run
```

The application seeds the `Admin` and `Student` roles and creates the administrator specified in User Secrets. The administrator email defaults to `admin@college.com` only if you configure that value as shown above; its password is the value you set locally.
