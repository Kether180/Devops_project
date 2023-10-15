
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Minitwit.Controllers;
using Minitwit.data;
using Minitwit.Models;
using Moq;

public class MinitwitTests : IDisposable
{
    private readonly DataContext context;
    private readonly SimulatorController simCon;

    private readonly ILogger<SimulatorController> logger;

    public MinitwitTests()
    {
        var builder = new DbContextOptionsBuilder<DataContext>();
            builder.UseInMemoryDatabase(databaseName: "MiniTwitDatabase");

            var dbContextOptions = builder.Options;
            context = new TestDataContext(dbContextOptions);
            // Delete existing db before creating a new one
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            logger = Mock.Of<ILogger<SimulatorController>>();
            simCon = new SimulatorController(context, logger);
    }

    public void Dispose()
    {
        // Teardown
        context.Dispose();
    }

    [Fact]
    public async Task test_get_all_users() {
        // Arrange

        // Act
        var response = await simCon.GetUsers();
        var result = (OkObjectResult)response.Result!;
        var userList = (List<User>) result.Value!;

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
        Assert.IsType<List<User>>(result.Value);
        Assert.Equal(3, userList.Count);
    }

    [Fact]
    public async Task test_create_user_successful(){
        // Arrange
        var user = new CreateUser {
            username = "testUser",
            email = "testuser@email.com",
            pwd = "testpass"
        };

        // Act
        var result = await simCon.RegisterUser(user, 1);
        var id = Helpers.GetUserIdByUsername(context, "testUser");

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
        Assert.Equal(4,id);
    }

    [Theory]
    [InlineData("","mail@mail.com","12345", "You have to enter a username")]
    [InlineData("username","mailmail.com","12345", "You have to enter a valid email address")]
    [InlineData("username","mail@mail.com","", "You have to enter a password")]
    [InlineData("TestUser1","TestUser1@test.com","12345", "The username is already taken")]
    public async Task test_create_user_unsuccessful(String username, string email, String pass, String errMsg){
        // Arrange
        var user = new CreateUser {
            username = username,
            email = email,
            pwd = pass
        };
        var error = new Error (errMsg);

        // Act
        var response = await simCon.RegisterUser(user, 1);
        var result = (BadRequestObjectResult)response.Result!;
        var resultError = (Error) result.Value!;

        // Assert
        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.IsType<Error>(result.Value);
        Assert.Equal(error.error_msg, resultError.error_msg);
    }

    [Theory]
    [InlineData("TestUser1", "user1")]
    [InlineData("TestUser2","user2")]
    [InlineData("TestUser3","user3")]
    public async Task test_login_successful(String username, String password){
        // Arrange
        var loginReq = new LoginRequest {
            username = username,
            pwd = password
        };

        // Act
        var response = await simCon.Login(loginReq);
        var result = (OkObjectResult) response.Result!;
        var resultUser = result.Value;

        // Assert
        Assert.IsType<OkObjectResult>(response.Result);
        Assert.Equal(username ,resultUser);
    }

    [Theory]
    [InlineData("TestUser1337","user1337", "Username does not match a user")]
    [InlineData("TestUser3","user1", "Incorrect password or username")]
    public async Task test_login_unsuccessful(String username, String password, String errMsg){
        // Arrange
        var loginReq = new LoginRequest {
            username = username,
            pwd = password
        };
        var error = new Error ( errMsg , 401);

        // Act
        var response = await simCon.Login(loginReq);
        var result = (UnauthorizedObjectResult) response.Result!;
        var resultError = (Error) result.Value!;

        // Assert
        Assert.Equal(error.error_msg, resultError.error_msg);
        Assert.IsType<Error>(result.Value);
        Assert.IsType<UnauthorizedObjectResult>(response.Result);
    }

    [Fact]
    public async Task test_follow_successful(){
        // Arrange
        var followReq = new FollowRequest {
            follow = "TestUser2"
        };


        // Act
        var result = await simCon.AddFollower("TestUser1", followReq);
        var followerCount = context.Followers.Where(u => u.UserId == 1 && u.FollowsId == 2).Count();

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
        Assert.Equal(1, followerCount);
    }

    [Theory]
    [InlineData("TestUser1337", "", "", "Username does not match a user")]
    [InlineData("TestUser1", "TestUser1337", "", "The user to follow was not found")]
    [InlineData("TestUser1", "TestUser2", "TestUser2", "The user is already following")]
    public async Task test_follow_unsuccessful(String user, String userToFollow, String user2ToFollow,String errMsg){
        // Arrange
        var followReq = new FollowRequest {
            follow = userToFollow
        };
        var followReq2 = new FollowRequest {
            follow = user2ToFollow
        };
        var error = new Error (errMsg);

        // Act
        var response = await simCon.AddFollower(user, followReq);
        if (user2ToFollow != "") {
            response = await simCon.AddFollower(user, followReq2);
        }
        var result = (BadRequestObjectResult)response.Result!;
        var resultError = (Error) result.Value!;

        // Assert
        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.IsType<Error>(result.Value);
        Assert.Equal(error.error_msg, resultError.error_msg);
    }

    [Fact]
    public async Task test_follow_unfollow_successful(){
        // Arrange
        var followReq = new FollowRequest {
            follow = "TestUser2"
        };

        var unfollowReq = new FollowRequest {
            unfollow = "TestUser2"
        };

        // Act
        await simCon.AddFollower("TestUser1", followReq);
        var result = await simCon.AddFollower("TestUser1", unfollowReq);
        var followerCount = context.Followers.Where(u => u.UserId == 1 && u.FollowsId == 2).Count();

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
        Assert.Equal(0, followerCount);
    }

    [Theory]
    [InlineData("TestUser1", "TestUser1337", "The user to unfollow was not found")]
    [InlineData("TestUser1", "TestUser2", "The user isn't following")]
    public async Task test_unfollow_unsuccessful(String user, String userToUnfollow, String errMsg){
        // Arrange
        var unfollowReq = new FollowRequest {
            unfollow = userToUnfollow
        };
        var error = new Error (errMsg);

        // Act
        var response = await simCon.AddFollower(user, unfollowReq);
        var result = (BadRequestObjectResult)response.Result!;
        var resultError = (Error) result.Value!;

        // Assert
        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.IsType<Error>(result.Value);
        Assert.Equal(error.error_msg, resultError.error_msg);
    }

    [Fact]
    public async Task test_try_to_follow_and_unfollow_fails(){
        // Arrange
        var followUnfollowReq = new FollowRequest {
            follow = "TestUser1",
            unfollow = "TestUser2"
        };
        var error = new Error ("You need to specify ONE of either follow or unfollow");

        // Act
        var response = await simCon.AddFollower("TestUser1", followUnfollowReq);
        var result = (BadRequestObjectResult)response.Result!;
        var resultError = (Error) result.Value!;

        // Assert
        Assert.IsType<BadRequestObjectResult>(response.Result);
        Assert.IsType<Error>(result.Value);
        Assert.Equal(error.error_msg, resultError.error_msg);
    }

    [Fact]
    public async Task test_post_message_successful(){
        // Arrange
        var msg1 = new CreateMessage{
            content = "Test message"
        };

        // Act
        var result = await simCon.AddUMsg("TestUser1", msg1);
        var msgCount = context.Messages.Where(a => a.AuthorId == 1).Count();

        // Assert
        Assert.IsType<NoContentResult>(result.Result);
        Assert.Equal(1, msgCount);
    }

    [Fact]
    public async Task test_post_message_unsuccessful(){
        // Arrange
        var msg1 = new CreateMessage{
            content = "Test message"
        };

        // Act
        var result = await simCon.AddUMsg("TestUser1337", msg1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
