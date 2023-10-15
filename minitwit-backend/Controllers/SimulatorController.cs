using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Minitwit.data;
using Minitwit.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Prometheus;

namespace Minitwit.Controllers
{
    [Route("api/[controller]")]
    public class SimulatorController : ControllerBase
    {
        private readonly DataContext _context;
        private static readonly Gauge LatestGauge = Metrics.CreateGauge("minitwit_latest", "Latest value processed");
        private static readonly Counter RegistrationCounter = Metrics.CreateCounter("minitwit_registration_count", "Number of user registrations");
        private static readonly Histogram RegistrationLatencyHistogram = Metrics.CreateHistogram("minitwit_registration_latency", "Registration request latency");
        private readonly ILogger<SimulatorController> _logger;
        public SimulatorController(DataContext context, ILogger<SimulatorController> logger) // Connect directly to the database
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("/")]
        public ActionResult<Status> getStatus()
        {
            int latest = Helpers.GetLatest();
            int userCount = _context.Users.Count();
            _logger.LogInformation("Accessed localhost:5050", DateTime.UtcNow);
            
            return new Status {
                State = "Running",
                LatestRequest = latest,
                UserCount = userCount
            };
        }

        [HttpGet]
        [Route("/latest")]
        public ActionResult<LatestRes> Latest()
        {
            LatestRes res = new LatestRes();
            res.latest = Helpers.GetLatest();
            return Ok(res);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesErrorResponseType(typeof(Error))]
        [Route("/register")]
        public async Task<ActionResult<List<User>>> RegisterUser([FromBody] CreateUser user, int latest = -1)
        {
            Helpers.UpdateLatest(latest);

            _logger.LogInformation("Register User", DateTime.UtcNow);

            User newUser = new User();

            if (user.username == null || user.username == "")
                return BadRequest(new Error("You have to enter a username"));

            else if (user.email == null || !user.email.Contains("@"))
                return BadRequest(new Error("You have to enter a valid email address"));

            else if (user.pwd == null || user.pwd == "")
                return BadRequest(new Error("You have to enter a password"));

            else if (Helpers.GetUserIdByUsername(_context, user.username) != -1)
                return BadRequest(new Error("The username is already taken"));


            newUser.Username = user.username;
            newUser.Email = user.email;

            // Hashing the users password is done as stated in this post: https://stackoverflow.com/questions/4181198/how-to-hash-a-password

            // Creating the salt for the password hash
            byte[] salt = new byte[16];
            using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(salt);
            }

            // Hash the password with the salt
            var pbkdf2 = new Rfc2898DeriveBytes(user.pwd, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            // combine the salt and password into one variable, with the salt in the first 16 bytes,
            // and the password in the last 20.
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // store the salt + hashed password in a string
            string savedPasswordHash = Convert.ToBase64String(hashBytes);
            newUser.PwHash = savedPasswordHash;

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Registration successful", DateTime.UtcNow);
            return NoContent();
            
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("/getAllUsers")]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            List<User> users = _context.Users.ToList();

            await Task.CompletedTask;

            return Ok(users);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Route("/login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequest req)
        {
            if (req == null){
                _logger.LogError("Unable to process request", DateTime.UtcNow);
                return Unauthorized(new Error("Unable to process login request", 401));
            }

            User? user = _context.Users.Where(u => u.Username == req.username).FirstOrDefault();

            if (user == null){
                _logger.LogError("Username does not match a user", DateTime.UtcNow);
                return Unauthorized(new Error("Username does not match a user", 401));
            }

            string savedPasswordHash = user.PwHash;

            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, salt.Length);

            var pbkdf2 = new Rfc2898DeriveBytes(req.pwd, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                    return Unauthorized(new Error("Incorrect password or username", 401));
            }

            await Task.CompletedTask;

            return Ok(user.Username);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("/msgs")]
        public async Task<ActionResult<List<MessageRes>>> GetMsgs(int no = 100, int latest = -1)
        {
            Helpers.UpdateLatest(latest);

            List<Message> msgs = _context.Messages.OrderByDescending(m => m.PubDate).Take(no).ToList();

            List<MessageRes> res = new List<MessageRes>();
            foreach (Message msg in msgs)
            {
                string username = _context.Users.Where(u => u.UserId == msg.AuthorId).First().Username;
                res.Add(new MessageRes()
                {
                    content = msg.text,
                    pub_date = msg.PubDate,
                    user = username
                });
            }

            await Task.CompletedTask;

            return Ok(res);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesErrorResponseType(typeof(Error))]
        [Route("/msgs/{username}")]
        public async Task<ActionResult<List<Message>>> AddUMsg(string username, [FromBody] CreateMessage msg, int latest = -1)
        {
            Helpers.UpdateLatest(latest);

            int userId = Helpers.GetUserIdByUsername(_context, username);
            if (userId == -1)
                return BadRequest(new Error("Username does not match a user"));

            Message newMsg = new Message()
            {
                AuthorId = userId,
                text = msg.content,
                PubDate = DateTime.UtcNow,
                Flagged = 0
            };

            _context.Messages.Add(newMsg);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("/msgs/{username}")]
        public async Task<ActionResult<List<MessageRes>>> GetMsgsByUser(string username, int latest = -1)
        {
            Helpers.UpdateLatest(latest);

            User? user = _context.Users.Where(x => x.Username == username).FirstOrDefault();

            if (user == null)
                return BadRequest(new Error("Username does not match a user"));

            List<Message> msgs = _context.Messages.Where(x => x.AuthorId == user.UserId).ToList();

            List<MessageRes> res = new List<MessageRes>();
            foreach (Message msg in msgs)
            {
                res.Add(new MessageRes()
                {
                    content = msg.text,
                    user = username,
                    pub_date = msg.PubDate
                });
            }

            await Task.CompletedTask;

            return Ok(res);
        }

        [HttpPost]
        [ProducesErrorResponseType(typeof(Error))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Route("/fllws/{username}")]
        public async Task<ActionResult<List<Follower>>> AddFollower(string username, [FromBody] FollowRequest folReq, int latest = -1)
        {
            Helpers.UpdateLatest(latest);

            int userId = Helpers.GetUserIdByUsername(_context, username);
            if (userId == -1)
                return BadRequest(new Error("Username does not match a user"));


            if (folReq.follow != null && folReq.unfollow == null)
            {
                int req_userId = Helpers.GetUserIdByUsername(_context, folReq.follow);
                if (req_userId == -1)
                    return BadRequest(new Error("The user to follow was not found"));

                Follower? followerEntity = _context.Followers.Where(x => x.UserId == userId && x.FollowsId == req_userId).FirstOrDefault();

                if (followerEntity != null)
                    return BadRequest(new Error("The user is already following"));

                _context.Followers.Add(new Follower()
                {
                    UserId = userId,
                    FollowsId = req_userId
                });
            }
            else if (folReq.unfollow != null && folReq.follow == null)
            {
                int req_userId = Helpers.GetUserIdByUsername(_context, folReq.unfollow);
                if (req_userId == -1)
                    return BadRequest(new Error("The user to unfollow was not found"));

                Follower? followerEntity = _context.Followers.Where(x => x.UserId == userId && x.FollowsId == req_userId).FirstOrDefault();

                if (followerEntity == null)
                    return BadRequest(new Error("The user isn't following"));

                _context.Followers.Remove(_context.Followers.Where(x => x.UserId == userId && x.FollowsId == req_userId).First());
            }
            else
            {
                return BadRequest(new Error("You need to specify ONE of either follow or unfollow"));
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet]
        [ProducesErrorResponseType(typeof(Error))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("/fllws/{username}")]
        public async Task<ActionResult<List<User>>> GetUserFollowers(string username, int no = 100, int latest = -1)
        {
            Helpers.UpdateLatest(latest);

            int userId = Helpers.GetUserIdByUsername(_context, username);
            if (userId == -1)
                return BadRequest("Username does not match a user");

            List<string> followingRes = _context.Followers
                .Join(
                    _context.Users,
                    f => f.FollowsId,
                    u => u.UserId,
                    (f, u) => new
                    {
                        userId = f.UserId,
                        Follows = u.Username
                    }
                    )
                .Where(t => t.userId == userId).Select(v => v.Follows).ToList();

            await Task.CompletedTask;

            return Ok(new FollowsRes() { follows = followingRes });
        }
    }

    public static class Helpers
    {
        private static int LATEST = 0;

        public static int GetLatest()
        {
            return LATEST;
        }

        public static void UpdateLatest(int latest)
        {
            if (latest != -1)
            {
                LATEST = latest;
            }
            else
            {
                LATEST = 0;
            }
        }

        public static int GetUserIdByUsername(DataContext _context, string username)
        {
            User? u = _context.Users.Where(u => u.Username == username).FirstOrDefault();
            if (u != null)
                return u.UserId;
            return -1;

        }
    }

    public class Error
    {
        public int status { get; set; }
        public string error_msg { get; set; } = "";
        public Error(string _error_msg, int _status = 400)
        {
            error_msg = _error_msg;
            status = _status;
        }
    }

    public class Status
    {
        public string State { get; set; } = "";
        public int UserCount { get; set; }
        public int LatestRequest { get; set; }
    }

    public class LatestRes
    {
        public int latest { get; set; }
    }

    public class CreateUser
    {
        public string username { get; set; } = "";
        public string email { get; set; } = "";
        public string pwd { get; set; } = "";
    }

    public class MessageRes
    {
        public string content { get; set; } = "";
        public DateTime pub_date { get; set; }
        public string user { get; set; } = "";
    }

    public class CreateMessage
    {
        public string content { get; set; } = "";
    }

    public class FollowsRes
    {
        public List<string> follows { get; set; } = new List<string>();
    }

    public class FollowRequest
    {
        public string? follow { get; set; } = null;
        public string? unfollow { get; set; } = null;
    }

    public class LoginRequest
    {
        public string username { get; set; } = "";
        public string pwd { get; set; } = "";
    }
}
