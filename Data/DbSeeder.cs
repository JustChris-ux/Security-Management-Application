using Microsoft.AspNetCore.Identity;
using SecurityFirm.Models;

namespace SecurityFirm.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed roles
            string[] roles = { "Admin", "Manager" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed admin user
            const string adminEmail = "admin@security.bg";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Системен администратор",
                    EmailConfirmed = true,
                    IsActive = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@12345");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Seed sample locations
            if (!context.Locations.Any())
            {
                var locations = new List<Location>
                {
                    new Location
                    {
                        Name = "Търговски Център Сердика",
                        Address = "бул. Сливница 2, 1202 София",
                        Info = "Голям търговски комплекс с над 200 магазина",
                        Notes = "Засилено охраняване в уикенди",
                        RiskLevel = RiskLevel.Medium,
                        IsActive = true
                    },
                    new Location
                    {
                        Name = "Бизнес Парк Изток",
                        Address = "ул. Цариградско шосе 115, 1784 София",
                        Info = "Офис комплекс с 12 сгради",
                        Notes = "24-часова охрана",
                        RiskLevel = RiskLevel.Low,
                        IsActive = true
                    },
                    new Location
                    {
                        Name = "Склад Логистик АД",
                        Address = "ул. Ботевградско шосе 247, 1517 София",
                        Info = "Логистичен склад, висока стойност на стоките",
                        Notes = "Строг контрол на достъпа",
                        RiskLevel = RiskLevel.High,
                        IsActive = true
                    }
                };
                context.Locations.AddRange(locations);
                await context.SaveChangesAsync();

                // Add cameras
                var cameras = new List<SecurityCamera>
                {
                    new SecurityCamera { Name = "Камера Вход Север", Position = "Северен вход", LocationId = 1 },
                    new SecurityCamera { Name = "Камера Паркинг А", Position = "Паркинг А", LocationId = 1 },
                    new SecurityCamera { Name = "Камера Вход Главен", Position = "Главен вход", LocationId = 2 },
                    new SecurityCamera { Name = "Камера Склад 1", Position = "Склад 1", LocationId = 3 },
                    new SecurityCamera { Name = "Камера Порта", Position = "Входна порта", LocationId = 3 },
                };
                context.SecurityCameras.AddRange(cameras);
                await context.SaveChangesAsync();
            }

            // Seed sample staff
            if (!context.StaffMembers.Any())
            {
                var staff = new List<StaffMember>
                {
                    new StaffMember
                    {
                        Name = "Иван Петров",
                        Phone = "0888 123 456",
                        Email = "i.petrov@security.bg",
                        HourlyRate = 7.50m,
                        PositivePoints = "Точен, надежден, добра комуникация",
                        NegativePoints = "",
                        Notes = "Предпочита нощни смени",
                        IsActive = true
                    },
                    new StaffMember
                    {
                        Name = "Мария Стоянова",
                        Phone = "0877 234 567",
                        Email = "m.stoyanova@security.bg",
                        HourlyRate = 8.00m,
                        PositivePoints = "Опитна, инициативна",
                        NegativePoints = "Закъснява понякога",
                        Notes = "Стаж 5 години",
                        IsActive = true
                    },
                    new StaffMember
                    {
                        Name = "Георги Димитров",
                        Phone = "0899 345 678",
                        Email = "g.dimitrov@security.bg",
                        HourlyRate = 7.00m,
                        PositivePoints = "Физически подготвен",
                        NegativePoints = "В процес на обучение",
                        Notes = "Нов служител - 3 месеца",
                        IsActive = true
                    },
                    new StaffMember
                    {
                        Name = "Петя Николова",
                        Phone = "0866 456 789",
                        Email = "p.nikolova@security.bg",
                        HourlyRate = 8.50m,
                        PositivePoints = "Отлична работа с клиенти, лидерски умения",
                        NegativePoints = "",
                        Notes = "Старши охранител",
                        IsActive = true
                    }
                };
                context.StaffMembers.AddRange(staff);
                await context.SaveChangesAsync();
            }
        }
    }
}
