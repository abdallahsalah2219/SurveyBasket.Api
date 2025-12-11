namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder
            .OwnsMany(x => x.RefreshTokens)
            .ToTable("RefreshTokens")
            .WithOwner()
            .HasForeignKey("UserId");

        builder.Property(x => x.FirstName)
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .HasMaxLength(100);

        builder.HasData(new ApplicationUser
        {
            Id = DefaultUsers.AdminId,
            FirstName = "Survey Basket",
            LastName = "Admin",
            UserName = DefaultUsers.AdminEmail,
            NormalizedUserName = DefaultUsers.AdminEmail.ToUpper(),
            Email = DefaultUsers.AdminEmail,
            NormalizedEmail = DefaultUsers.AdminEmail.ToUpper(),
            ConcurrencyStamp = DefaultUsers.AdminConcurrencyStamp,
            SecurityStamp = DefaultUsers.AdminSecurityStamp,
            EmailConfirmed = true,
            PasswordHash = DefaultUsers.AdminPasswordHash
        });
    }
}

