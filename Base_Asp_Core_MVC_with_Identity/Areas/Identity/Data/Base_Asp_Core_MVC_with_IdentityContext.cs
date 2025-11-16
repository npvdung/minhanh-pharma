using Base_Asp_Core_MVC_with_Identity.Areas.Identity.Data;
using Base_Asp_Core_MVC_with_Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;


namespace Base_Asp_Core_MVC_with_Identity.Data;

public class Base_Asp_Core_MVC_with_IdentityContext : IdentityDbContext<UserSystemIdentity>
{
    public Base_Asp_Core_MVC_with_IdentityContext(DbContextOptions<Base_Asp_Core_MVC_with_IdentityContext> options)
        : base(options)
    {
    }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Import> ImportsProduct { get; set; }
    public DbSet<ImportProducts> ImportProductDetails { get; set; }
    public DbSet<Sales> Invoices { get; set; }
    public DbSet<SalesProducts> Invoice_Details { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<DisposalRecords> ReturnProducts { get; set; }
    public DbSet<DisposalProducts> Return_Product_Details { get; set; }
    public DbSet<Warehouse> stocks { get; set; }
    public DbSet<Supplier> suppliers { get; set; }
    public DbSet<ReSales> reSales { get; set; }
    public DbSet<ProductUnit> productUnits { get; set; }
    public DbSet<ReSalesDetail> reSalesDetail { get; set; }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
        builder.ApplyConfiguration(new ApplicationUserEntityConfiguration());
    }

    private class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<UserSystemIdentity>
    {
        public void Configure(EntityTypeBuilder<UserSystemIdentity> builder)
        {
            builder.Property(u => u.FirstName).HasMaxLength(150);
            builder.Property(u => u.LastName).HasMaxLength(150);
            builder.Property(u => u.CodeUser).HasMaxLength(150);
            builder.Property(u => u.Status).HasDefaultValue(0);
            builder.Property(u => u.Gender).HasMaxLength(100);
            builder.Property(u => u.DateOfBirth).HasDefaultValue(DateTime.Now);
            builder.Property(u => u.Position).HasMaxLength(150);
            builder.Property(u => u.Address).HasMaxLength(150);
            builder.Property(u => u.Note).HasMaxLength(150);

        }
    }

}
