using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using KevinSoloProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace KevinSoloProject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment WebHostEnvironment;

    private MyContext _context;
    public HomeController(ILogger<HomeController> logger, MyContext context, IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _context = context;
        WebHostEnvironment = webHostEnvironment;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }
        int id = (int)HttpContext.Session.GetInt32("userId");

        ViewBag.iLoguari = _context.Users.FirstOrDefault(e => e.UserId == id);

        return View();
    }

    [HttpGet("Register")]
    public IActionResult Register()
    {
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return View();
        }
        return RedirectToAction("Index");
    }

    [HttpPost("Register")]
    public IActionResult Register(User user)
    {

        if (ModelState.IsValid)
        {
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already in use!");

                return View();
            }
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            user.Password = Hasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("userId", user.UserId);
            return RedirectToAction();
        }
        return View();
    }

    [HttpGet("Login")]
    public IActionResult Login()
    {
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return View();
        }
        return RedirectToAction("Index");
    }

    [HttpPost("Login")]
    public IActionResult LoginSubmit(LoginUser userSubmission)
    {
        if (ModelState.IsValid)
        {
            var userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
            if (userInDb == null)
            {
                ModelState.AddModelError("User", "Invalid UserName/Password");
                return View("Register");
            }

            var hasher = new PasswordHasher<LoginUser>();

            var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);

            if (result == 0)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                return View("Register");
            }
            HttpContext.Session.SetInt32("userId", userInDb.UserId);

            return RedirectToAction();
        }
        return View("Register");
    }

    [HttpGet("LogOut")]
    public IActionResult LogOut()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Register");
    }

    [HttpGet("Clothes")]
    public IActionResult Clothes()
    {
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }
        int id = (int)HttpContext.Session.GetInt32("userId");

        ViewBag.iLoguari = _context.Users.Include(e => e.PostedImage).FirstOrDefault(e => e.UserId == id);

        return View();
    }


    [HttpGet("Postclothes")]
    public IActionResult Postclothes()
    {
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }
        int id = (int)HttpContext.Session.GetInt32("userId");

        ViewBag.iLoguari = _context.Images.OrderByDescending(e => e.CreatedAt).FirstOrDefault();
        ViewBag.iLoguari = _context.Users.FirstOrDefault(e => e.UserId == id);
        return View();
    }

    [HttpGet("Favourite")]
    public IActionResult Favourite()
    {
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }
        int id = (int)HttpContext.Session.GetInt32("userId");

        ViewBag.iLoguari = _context.Users.Include(e => e.PostedImage).FirstOrDefault(e => e.UserId == id);
        ViewBag.myfavs = _context.Favourites.Include(c => c.FavouriteClothes).Include(c => c.User).Where(c => c.UserId == id);

        return View();
    }
    [HttpGet("Favourite/{id}")]
    public IActionResult AddFavorites(int id)
    {
        int SessionId = (int)HttpContext.Session.GetInt32("userId");
        Favourite NewFavorite = new Favourite
        {
            UserId = SessionId,
            ImageId = id

        };
        _context.Favourites.Add(NewFavorite);
        _context.SaveChanges();

        return RedirectToAction("Favourite");
    }

    [HttpGet("FavouriteRemove/{id}")]
    public IActionResult RemoveFavorites(int id)
    {
        int SessionId = (int)HttpContext.Session.GetInt32("userId");

        Favourite RemoveFavorites = _context.Favourites.First(e => (e.ImageId == id) && (e.UserId == SessionId));

        _context.Remove(RemoveFavorites);
        _context.SaveChanges();

        return RedirectToAction("Clothes");
    }

    [HttpGet("Profile")]
    public IActionResult Profile()
    {
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }
        int id = (int)HttpContext.Session.GetInt32("userId");

        return View();
    }

    [HttpPost("Postedimage")]
    public IActionResult Postedimage(Image marrNgaView, string Dimer)
    {
        string StringFileName = UploadFile(marrNgaView);
        var image = new Image()
        {
            Myimage = StringFileName,
            Stina = marrNgaView.Stina,
            Type = marrNgaView.Type,
            Category = marrNgaView.Category

        };

        int id = (int)HttpContext.Session.GetInt32("userId");

        image.UserId = id;
        _context.Images.Add(image);
        _context.SaveChanges();
        return RedirectToAction("Clothes");
    }

    private string UploadFile(Image marrNgaView)
    {
        string fileName = null;
        if (marrNgaView.Clothes != null)
        {
            string Uploaddir = Path.Combine(WebHostEnvironment.WebRootPath, "Images");
            fileName = marrNgaView.Clothes.FileName;
            string filePath = Path.Combine(Uploaddir, marrNgaView.Clothes.FileName);
            using (var filestream = new FileStream(filePath, FileMode.Create))
            {
                marrNgaView.Clothes.CopyTo(filestream);
            }
        }
        return fileName;
    }

    [HttpGet("Edit/{imgid}")]
    public IActionResult Edit(int imgid)
    {

        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }

        Image Editing = _context.Images.FirstOrDefault(p => p.ImageId == imgid);

        ViewBag.imazhiId = imgid;
        return View(Editing);
    }

    [HttpPost("Edit/{id}")]
    public IActionResult Edit(Image marrngadd, int id)

    {
        string StringFileName = UploadFile(marrngadd);
        Image editing = _context.Images.FirstOrDefault(p => p.ImageId == id);
        editing.Stina = marrngadd.Stina;
        editing.Type = marrngadd.Type;
        editing.Category = marrngadd.Category;

        editing.UpdatedAt = DateTime.Now;
        _context.SaveChanges();
        return RedirectToAction("Clothes");
    }


    [HttpPost("generate")]
    public IActionResult getrandomimage(string moti)
    {
        if (HttpContext.Session.GetInt32("userId") == null)
        {
            return RedirectToAction("Register");
        }

        ViewBag.kepucet = "";
        ViewBag.pantallonat ="";
        ViewBag.bluzat="";
        ViewBag.xhaketat="";

        string bluze = string.Empty;
        Image getRandom(List<Image> lista)
        {   Console.WriteLine("Lista nr "+lista.Count() );
            Random rnd = new Random();
            if (lista.Count() != 0)
            {
                ViewBag.Count = lista.Count();
                int indx = rnd.Next(0, lista.Count);
                Console.WriteLine("nuk eshteeee null");
                Image newimg = lista[indx];
                return newimg;
            }
            else
            {   
                Console.WriteLine("eshte null");
                return null;
            }

        }
        string stina = "";
        Double motiDouble = Convert.ToDouble(moti);
        if (motiDouble <= 10)
        {
            stina = "Dimer";
            List<Image>? bluza = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "bluze").ToList();
            List<Image>? kepuce = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "kepuce").ToList();
            List<Image> pantallona = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "Pantallona").ToList();
            List<Image> xhaketa = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "Xhakete").ToList();
            ViewBag.kepucet = getRandom(kepuce);
            ViewBag.pantallonat = getRandom(pantallona);
            ViewBag.xhaketat = getRandom(xhaketa);
            ViewBag.bluzat = getRandom(bluza);

        }
        if (motiDouble > 10 && motiDouble <= 15)
        {
            stina = "Vjeshte";
            List<Image>? bluza = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "bluze").ToList();
            List<Image>? kepuce = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "kepuce").ToList();
            List<Image> pantallona = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "Pantallona").ToList();
            List<Image> xhaketa = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "Xhakete").ToList();
            ViewBag.kepucet = getRandom(kepuce);
            ViewBag.pantallonat = getRandom(pantallona);
            ViewBag.xhaketat = getRandom(xhaketa);
            ViewBag.bluzat = getRandom(bluza);
        }
         if (motiDouble > 15 && motiDouble <= 22)
        {
            stina = "Pranvere";
            List<Image>? bluza = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "bluze").ToList();
            List<Image>? kepuce = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "kepuce").ToList();
            List<Image> pantallona = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "Pantallona").ToList();
            ViewBag.kepucet = getRandom(kepuce);
            ViewBag.pantallonat = getRandom(pantallona);
            ViewBag.bluzat = getRandom(bluza);
            
        }
        if (motiDouble > 22)
        {   
            stina = "Vere";
            List<Image>? bluza = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "bluze").ToList();
            List<Image>? kepuce = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "kepuce").ToList();
            List<Image> pantallona = _context.Images.Where(e => e.Stina == stina).Where(e => e.Type == "Pantallona").ToList();
            ViewBag.kepucet = getRandom(kepuce);
            ViewBag.pantallonat = getRandom(pantallona);
            ViewBag.bluzat = getRandom(bluza);
            
        }

        int id = (int)HttpContext.Session.GetInt32("userId");
        ViewBag.iLoguari = _context.Users.FirstOrDefault(e => e.UserId == id);

        return View("Index2");
    }


    [HttpGet("delete/{id}")]
    public IActionResult Delete(int id)
    {
        int SessionId = (int)HttpContext.Session.GetInt32("userId");
        Image thisimage = _context.Images.FirstOrDefault(p => p.ImageId == id);
        _context.Images.Remove(thisimage);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
