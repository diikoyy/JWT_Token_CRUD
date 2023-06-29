using FormulaOneApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FormulaOneApp.Data
{
    //AppDbContext inherit from class DbContext
    public class AppDbContext : IdentityDbContext
    {
        //Dbset: build the sql script for the class teams and create a table
        public DbSet<Team> Teams { get; set; }

        //Constructor to connect with Db by appsettings.json
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {

        }
        //Create migration by terminal: dotnet ef migrations add "initialize migration"
        //Run SQL Script in migration: dotnet ef database update
    }
}
