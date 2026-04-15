using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Linq;
using DigitalWallet.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalWallet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            if (string.IsNullOrEmpty(user.Name) ||
                string.IsNullOrEmpty(user.Email) ||
                string.IsNullOrEmpty(user.Password))
            {
                return BadRequest("All fields are required");
            }
            if (user.Password.Length < 8)
            {
                return BadRequest("Password must be at least 8 characters long");
            }

            var existingUser = _context.Users.FirstOrDefault(u => u.Email == user.Email);
            if (existingUser != null)
            {
                return BadRequest("Email already exists");
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(user);
        }
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginUser)
        {
            var user = _context.Users.FirstOrDefault(u =>
                u.Email == loginUser.Email &&
                u.Password == loginUser.Password
            );

            if (user == null)
                return Unauthorized("Invalid credentials");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("ThisIsMySuperSecretKeyForJwtAuthentication123456789"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
          new Claim("userId", user.Id.ToString())
          };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return Ok(new
            {
                token = jwt,
                userId = user.Id,
                name = user.Name
            });
        }

        [Authorize]

        [HttpPost("add-money")]
        public IActionResult AddMoney(int userId, decimal amount)
        {
            if (amount <= 0)
                return BadRequest("Amount must be greater than 0");

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);

            if (wallet == null)
            {
                wallet = new Wallet
                {
                    UserId = userId,
                    Balance = amount
                };

                _context.Wallets.Add(wallet);
            }
            else
            {
                wallet.Balance += amount;
            }

            _context.SaveChanges();

            return Ok(wallet);
        }
        [Authorize]

        [HttpPost("transfer")]
        public IActionResult Transfer(int senderId, int receiverId, decimal amount)
        {
            if (amount <= 0)
                return BadRequest("Invalid amount");

            var senderWallet = _context.Wallets.FirstOrDefault(w => w.UserId == senderId);

            if (senderWallet == null)
                return BadRequest("Sender wallet not found");

            var receiverWallet = _context.Wallets.FirstOrDefault(w => w.UserId == receiverId);


            if (receiverWallet == null)
            {
                receiverWallet = new Wallet
                {
                    UserId = receiverId,
                    Balance = 0
                };

                _context.Wallets.Add(receiverWallet);
            }

            if (senderWallet.Balance < amount)
                return BadRequest("Insufficient balance");

            senderWallet.Balance -= amount;
            receiverWallet.Balance += amount;

            var transaction = new Transaction
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Amount = amount,
                Date = DateTime.Now
            };

            int earnedPoints = (int)(amount / 100);

            _context.Transactions.Add(transaction);

            var userPoints = _context.Points.FirstOrDefault(p => p.UserId == senderId);

            if (userPoints == null)
            {
                userPoints = new Point
                {
                    UserId = senderId,
                    Points = earnedPoints
                };
                _context.Points.Add(userPoints);
            }
            else
            {
                userPoints.Points += earnedPoints;
            }
            _context.SaveChanges();

            return Ok(transaction);
        }
          [Authorize]

        [HttpPost("redeem")]
        public IActionResult Redeem(int userId)
        {
            var userPoints = _context.Points.FirstOrDefault(p => p.UserId == userId);

            if (userPoints == null || userPoints.Points < 10)
                return BadRequest("Not enough points");

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);

            if (wallet == null)
                return BadRequest("Wallet not found");

            int redeemPoints = 0;
            decimal reward = 0;

            if (userPoints.Points >= 50)
            {
                redeemPoints = 50;
                reward = 100;
            }
            else if (userPoints.Points >= 10)
            {
                redeemPoints = 10;
                reward = 50;
            }

            userPoints.Points -= redeemPoints;
            wallet.Balance += reward;

            _context.SaveChanges();

            return Ok(new
            {
                message = "Redeemed successfully",
                redeemedPoints = redeemPoints,
                cashback = reward,
                remainingPoints = userPoints.Points,
                newBalance = wallet.Balance
            });
        }
       [Authorize]
       [HttpGet("balance")]
       public IActionResult GetBalance(int userId)
       {
       var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);

        if (wallet == null)
        {
          return Ok(0); 
          }
         return Ok(wallet.Balance);
        }
        [Authorize]

        [HttpGet("points")]
        public IActionResult GetPoints(int userId)
        {
            var userPoints = _context.Points.FirstOrDefault(p => p.UserId == userId);

            if (userPoints == null)
                return Ok(new { points = 0 });

            return Ok(new { points = userPoints.Points });
        }
        [Authorize]

        [HttpGet("transactions")]
        public IActionResult GetTransactions(int userId)
        {
            var transactions = _context.Transactions
                .Where(t => t.SenderId == userId || t.ReceiverId == userId)
                .Select(t => new
                {
                    senderName = _context.Users
                        .Where(u => u.Id == t.SenderId)
                        .Select(u => u.Name)
                        .FirstOrDefault(),

                    receiverName = _context.Users
                        .Where(u => u.Id == t.ReceiverId)
                        .Select(u => u.Name)
                        .FirstOrDefault(),

                    amount = t.Amount,
                    date = t.Date
                })
                .ToList();

            return Ok(transactions);
        }
        [Authorize]

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userIdClaim = User.FindFirst("userId");

            if (userIdClaim == null)
                return Unauthorized();

            int userId = int.Parse(userIdClaim.Value);

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found");

            return Ok(new
            {
                userId = user.Id,
                name = user.Name,
                email = user.Email
            });
        }
    }
}
