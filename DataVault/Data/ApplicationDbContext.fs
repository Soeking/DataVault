namespace DataVault.Data

open Microsoft.AspNetCore.Identity.EntityFrameworkCore
open Microsoft.EntityFrameworkCore

type ApplicationDbContext(options: DbContextOptions<ApplicationDbContext>) =
    inherit IdentityDbContext(options)
    