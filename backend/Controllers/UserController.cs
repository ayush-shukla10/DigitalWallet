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
    }
}
