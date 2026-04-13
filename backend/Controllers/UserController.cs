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
        public IActionResult Login(User login)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Email == login.Email && u.Password == login.Password);

            if (user == null)
                return Unauthorized("Invalid email or password");

            return Ok(user);
        }
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

            var senderPoints = _context.Points.FirstOrDefault(p => p.UserId == senderId);

            if (senderPoints == null)
            {
                senderPoints = new Point
                {
                    UserId = senderId,
                    Points = earnedPoints
                };

                _context.Points.Add(senderPoints);
            }
            else
            {
                senderPoints.Points += earnedPoints;
            }

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            return Ok(transaction);
        }
        [HttpPost("redeem")]
        public IActionResult Redeem(int userId)
        {
            // Get user points
            var userPoints = _context.Points.FirstOrDefault(p => p.UserId == userId);

            //If no points or less than 10
            if (userPoints == null || userPoints.Points < 10)
                return BadRequest("Not enough points");

            // Get wallet
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);

            if (wallet == null)
                return NotFound("Wallet not found");

            // Redeem logic
            int redeemPoints = 10;
            decimal reward = redeemPoints * 5;      // add money
            userPoints.Points -= 10;   // deduct points

            _context.SaveChanges();

            return Ok(new
            {
                message = "Redeemed successfully",
                newBalance = wallet.Balance,
                remainingPoints = userPoints.Points
            });
        }
        [HttpGet("balance")]
        public IActionResult GetBalance(int userId)
        {
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserId == userId);

            if (wallet == null)
                return NotFound("Wallet not found");

            return Ok(wallet.Balance);
        }
        [HttpGet("transactions")]
        public IActionResult GetTransactions(int userId)
        {
            var transactions = _context.Transactions
                .Where(t => t.SenderId == userId || t.ReceiverId == userId)
                .ToList();

            return Ok(transactions);
        }
    }
}
