# Ba.Core.Persistence.Mongo

A modern and extensible Entity Framework Core–based Persistence Library designed for scalable, clean, and maintainable .NET applications.

## Features

- Generic MongoDB Repository Pattern
- Soft Delete support (DeletedDate)
- Date-based auditing (CreatedDate, UpdatedDate)
- Pagination utilities (`Paginate`, `ToPaginateAsync`)
- Dynamic Querying (Filter, Sort, Nested Filters)
- Async CRUD operations
- Clean Architecture & DDD compatible
- Minimal configuration, plug-and-play usage

---

## Installation

Install via .NET CLI: 
```bash```
dotnet add package Ba.Core.Persistence.Mongo

Or via NuGet Package Manager:
```powershell```
Install-Package Ba.Core.Persistence.Mongo

## Basic Usage

- Program.cs

builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddScoped(typeof(IMongoRepositoryAsync<>), typeof(MongoRepositoryBase<>));

- Appsettings.json

"MongoSettings": {
  "ConnectionString": "mongodb://localhost:27017",
  "Database": "MyAppDb"
}

## Example Document
public class User : BaseDocument
{
    public string Name { get; set; }
    public string Email { get; set; }
}

## Example Usage
var user = await _userRepository.GetAsync(x => x.Email == "demo@example.com");

## License
This project is licensed under the MIT License.

## Contributing
Contributions, issues, and feature requests are welcome!
Feel free to open a Pull Request or create an Issue on GitHub.