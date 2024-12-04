using CliniqonProject.Data;
using CliniqonProject.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CliniqonProject.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IWebHostEnvironment Environment;
        private readonly ApplicationDbContext context;
        string apiBaseUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(ILogger<HomeController> logger, IWebHostEnvironment _environment, IConfiguration configuration, ApplicationDbContext _context, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            this.context = _context;
            Environment = _environment;
            _httpContextAccessor = httpContextAccessor;
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Login(LoginTbl login)
        {
            string email = login.Email;
            string password = login.Password;
            string encrptPass = Encrypt(password);

            var user = from username in context.LoginTbl
                       where username.Email.ToLower() == email.ToLower() && username.Password == encrptPass
                       select new
                       {
                           LoginID = username.LoginID,
                           UserId = username.UserId,
                       };

            var userList = user.FirstOrDefault();

            if (userList != null && userList.UserId > 0)
            {
                bool isActiveUser = context.UserTbl
                                     .Where(x => x.UserId == userList.UserId && x.Status == true)
                                     .Select(a => a.UserId)
                                     .FirstOrDefault() > 0;

                if (isActiveUser)
                {
                    HttpContext.Session.SetInt32("UserId", userList.UserId);
                    return RedirectToAction("UserList", "User");
                }
            }

            ViewBag.ErrorMessage = "Invalid email or password. Please try again.";
            return View(login); 
        }

        [HttpGet]
        [Route("User/RegisterUser")]
        public IActionResult RegisterUser()
        {


            return View();


        }
        public async Task<ActionResult> RegisterUser(userDetails user, List<IFormFile> ProfilePic1)
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    

                    if (ModelState.IsValid)
                    {
                        // Handling file uploads
                        if (ProfilePic1 != null && ProfilePic1.Count > 0)
                        {
                            string path = Path.Combine(this.Environment.WebRootPath, "PkgImage");
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            List<string> uploadedFiles = new List<string>();
                            Random random = new Random();
                            foreach (IFormFile postedFile in ProfilePic1)
                            {
                                int FileNameId = random.Next(10000, 999999);
                                string fileName = $"{FileNameId}{Path.GetExtension(postedFile.FileName)}";
                                using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                                {
                                    postedFile.CopyTo(stream);
                                    uploadedFiles.Add(fileName);
                                }
                                user.ProfilePic = fileName; // Store the last uploaded file name
                            }
                        }
                        // Insert into UserTbl
                        UserTbl newUser = new UserTbl
                        {
                            Email = user.Email,
                            ProfilePic = user.ProfilePic,
                            Name = user.Name,
                            DOB = user.DOB,
                            Designation = user.Designation,
                            Gender = user.Gender,
                            Country = user.Country,
                            FavoriteColor = user.FavoriteColor,
                            FavoriteActor = user.FavoriteActor,
                            Status = true,
                            AddedDate = DateTime.Now,
                        };

                        context.UserTbl.Add(newUser);
                        await context.SaveChangesAsync();

                        // Retrieve the userId of the newly added user
                        int userId = newUser.UserId;

                        string encryptedPassword = Encrypt(user.Password);

                        // Insert into LoginTbl
                        LoginTbl newLogin = new LoginTbl
                        {
                            UserId = userId,
                            Email = user.Email,
                            Password = encryptedPassword
                        };

                        context.LoginTbl.Add(newLogin);
                        await context.SaveChangesAsync();

                        await transaction.CommitAsync();
                        _logger.LogInformation("User and login details successfully created for user ID {UserId}", userId);
                        return RedirectToAction("Login");
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while registering the user with email {Email}", user.Email);

                }
            }

            return View(user);
        }
        public async Task<ActionResult> Home()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            UserTbl user = new UserTbl();

            try
            {
                var userList = await context.UserTbl.FindAsync(userId);

                if (userList != null)
                {
                    user.Email = userList.Email;
                    user.Name = userList.Name;
                    user.Gender = userList.Gender;
                    user.ProfilePic = userList.ProfilePic;
                    user.AddedDate = userList.AddedDate;
                    user.Country = userList.Country;
                    user.Designation = userList.Designation;
                    user.DOB = userList.DOB;
                    user.FavoriteActor = userList.FavoriteActor;
                    user.FavoriteColor = userList.FavoriteColor;
                    user.UserId = userId.Value;
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return RedirectToAction("Error", "Home"); 
            }
            ViewBag.ProfilePic = user.ProfilePic;
            return View(user);
        }
        public async Task<ActionResult> UserDetails(int id)
        {

            if (id == null)
            {
                return RedirectToAction("Login", "User");
            }

            UserTbl user = new UserTbl();

            try
            {
                var userList = await context.UserTbl.FindAsync(id);

                if (userList != null)
                {
                    user.Email = userList.Email;
                    user.Name = userList.Name;
                    user.Gender = userList.Gender;
                    user.ProfilePic = userList.ProfilePic;
                    user.AddedDate = userList.AddedDate;
                    user.Country = userList.Country;
                    user.Designation = userList.Designation;
                    user.DOB = userList.DOB;
                    user.FavoriteActor = userList.FavoriteActor;
                    user.FavoriteColor = userList.FavoriteColor;
                    user.UserId = userList.UserId;
                }
                else
                {
                    return RedirectToAction("Error", "Home");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }
            return View(user);
        }
        public IActionResult UserList()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var users = context.UserTbl.Where(u => u.Status == true && u.UserId != userId).OrderBy(u => u.AddedDate).ToList(); 

            return View(users); 
        }
        public IActionResult SearchFriends()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var users = context.UserTbl.Where(u => u.Status == true && u.UserId != userId).OrderBy(u => u.AddedDate).ToList();

            return View(users);
        }
        [HttpPost]
        public IActionResult MakeFriend([FromBody] FriendTable request)
        {
            if (request == null || request.UserId <= 0)
            {
                return Json(new { success = false, message = "Invalid User ID" });
            }
            request.FriendId = request.UserId;
            request.UserId = (int)HttpContext.Session.GetInt32("UserId");

            try
            {
                var existingFriendship = context.FriendTable.FirstOrDefault(f =>(f.UserId == request.UserId && f.FriendId == request.FriendId)); 

                if (existingFriendship != null)
                {
                    return Json(new { success = false, message = "You have already added this friend." });
                }

                var newFriendship = new FriendTable
                {
                    UserId = request.UserId,
                    FriendId = request.FriendId,
                    Status = "Requested",
                    RequestDate = DateTime.UtcNow
                };
                context.FriendTable.Add(newFriendship);
                context.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult ApplyFilter(string name, DateTime? dob, string gender, string color, string actor)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var query = context.UserTbl.Where(u => u.Status == true);

            // Apply filter conditions if the parameters are provided
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(u => u.Name.Contains(name));
            }

            if (dob.HasValue)
            {
                query = query.Where(u => u.DOB == dob.Value);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(u => u.Gender == gender);
            }

            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(u => u.FavoriteColor.Contains(color));
            }

            if (!string.IsNullOrEmpty(actor))
            {
                query = query.Where(u => u.FavoriteActor.Contains(actor));
            }

            var filteredData = query.ToList();

            var tableRows = string.Empty;
            foreach (var user in filteredData)
            {
                var profilePicUrl = Url.Content($"~/PkgImage/{user.ProfilePic}");
                tableRows += $@"
        <tr>
            <td><img class='profile-pic' src='{profilePicUrl}' alt='Profile Picture' /></td>
            <td>{user.Name}</td>
            <td>{user.Gender}</td>
            <td>{user.DOB:dd-MMM-yyyy}</td>
            <td>{user.FavoriteColor}</td>
            <td>{user.FavoriteActor}</td>
            <td>{user.AddedDate:dd-MMM-yyyy}</td>
        </tr>";
            }

            return Json(new { html = tableRows });

        }
        public IActionResult MatchFriends()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var loggedInUser = context.UserTbl.FirstOrDefault(u => u.UserId == userId);

            if (loggedInUser == null)
            {
                return NotFound("User not found.");
            }

            var matchingUsers = context.UserTbl
                .Where(u => u.UserId != userId && 
                            u.Status == true &&
                            (u.FavoriteColor == loggedInUser.FavoriteColor || 
                             u.FavoriteActor == loggedInUser.FavoriteActor || 
                             u.Country == loggedInUser.Country)) 
                .ToList();

            return View(matchingUsers);
        }

        public IActionResult MyFriends()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var users = (from friend in context.FriendTable
                         join user in context.UserTbl on friend.UserId equals user.UserId
                         where friend.UserId == userId
                         select new userDetails
                         {
                             FriendId = friend.FriendId,
                             Name = user.Name, 
                             Designation = user.Designation,
                             ProfilePic = user.ProfilePic,
                             Email = user.Email,
                             StatusFriend = friend.Status,
                             UserId = user.UserId
                         }).ToList();

            return View(users);
        }
        public async Task<IActionResult> Logout()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                HttpContext.Session.Remove("UserId");

            }
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "User");
        }
        public string Encrypt(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (System.Security.Cryptography.Aes encryptor = System.Security.Cryptography.Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return string.Empty;
            }

            string encryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }


    }
}
