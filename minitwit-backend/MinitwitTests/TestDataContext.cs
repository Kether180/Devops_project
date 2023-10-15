using Microsoft.EntityFrameworkCore;
using Minitwit.Models;
using Minitwit.data;



public class TestDataContext : DataContext {

    public TestDataContext(DbContextOptions<DataContext> options) : base(options)
    {

    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Email = "TestUser1@test.com",
                    PwHash = "k1Il+SFCneboG/h+NJG5hecINh/9Z/zOakUi3wVwxZi7kjz4", // user1
                    Username = "TestUser1"

                },

                new User
                {
                    UserId = 2,
                    Email = "TestUser2@test.com",
                    PwHash = "JzISAIxVvydWkTEbntPOPqs5qxlEiCUZAMT49qsPXS2seOFD", // user2
                    Username = "TestUser2"
                },

                new User{
                    UserId = 3,
                    Email = "TestUser3@test.com",
                    PwHash = "scXeX4z+qgDp6XAT+i+CQrT4zVK6Pu565DxyEaECyLgx0Lsa", // user3
                    Username = "TestUser3"
                }
                );
        }

}
