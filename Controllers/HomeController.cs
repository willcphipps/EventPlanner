using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using BeltExam.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeltExam.Controllers {
    public class HomeController : Controller {
        private MyContext _context { get; set; }

        public HomeController (MyContext context) {
            _context = context;
        }

        [HttpGet ("")]
        public IActionResult Index () {
            return View ();
        }

        [HttpPost ("login")]
        public IActionResult Login (LoginUser userLogin) {
            if (ModelState.IsValid) {
                var userInDb = _context.Users.FirstOrDefault (u => u.Email == userLogin.LoginEmail);
                if (userInDb == null) {
                    ModelState.AddModelError ("LoginEmail", "Invalid Email/Password");
                    return View ("Index");
                } else {
                    var hasher = new PasswordHasher<LoginUser> ();
                    var result = hasher.VerifyHashedPassword (userLogin, userInDb.Password, userLogin.LoginPassword);
                    if (result == 0) {
                        ModelState.AddModelError ("LoginEmail", "Invalid Email/Password");
                        return View ("Index");
                    } else {
                        HttpContext.Session.SetInt32 ("userid", userInDb.UserId);
                        return Redirect ("/Dashboard");
                    }
                }
            } else {
                return View ("Index");
            }
        }

        [HttpPost ("Register")]
        public IActionResult Register (User user) {
            if (ModelState.IsValid) {
                if (_context.Users.Any (u => u.Email == user.Email)) {
                    ModelState.AddModelError ("Email", "Email already in use!");
                    return View ("Index");
                } else {
                    PasswordHasher<User> Hasher = new PasswordHasher<User> ();
                    user.Password = Hasher.HashPassword (user, user.Password);
                    _context.Users.Add (user);
                    _context.SaveChanges ();
                    HttpContext.Session.SetInt32 ("userid", user.UserId);
                    return Redirect ("/Dashboard");
                }
            }
            return View ("Index");
        }

        [HttpGet ("Dashboard")]
        public IActionResult Dashboard () {
            int? userid = HttpContext.Session.GetInt32 ("userid");
            if (userid == null) {
                return RedirectToAction ("Logout");
            } else {
                ViewBag.User = _context.Users.FirstOrDefault (u => u.UserId == userid);
                List<DojoActivity> dojoactivities = _context.DojoActivities.Include (a => a.Coordinator).Include (a => a.CalandarActivities).ThenInclude (c => c.UserJoin).Where(d => d.StartDate > DateTime.Now).OrderBy(z => z.StartDate).ToList ();
                return View (dojoactivities);
            }
        }

        [HttpGet ("Logout")]
        public IActionResult Logout () {
            HttpContext.Session.Clear ();
            return RedirectToAction ("Index");
        }

        [HttpGet ("Add")]
        public IActionResult Add () {
            return View ();
        }

        [HttpPost ("Create/Activity")]
        public IActionResult Create (DojoActivity newactivity) {
            if (ModelState.IsValid) {
                int? userid = HttpContext.Session.GetInt32 ("userid");
                if (userid == null) {
                    return RedirectToAction ("Logout");
                }
                newactivity.UserId = (int) userid;
                _context.DojoActivities.Add (newactivity);
                _context.SaveChanges ();
                return Redirect ($"/Display/{newactivity.ActivityId}"); { }
            }
            return View ("Add");
        }

        [HttpGet ("Join/{userid}/{actid}")]
        public IActionResult Join (int userid, int actid) {
            CalandarEvent tojoin = new CalandarEvent ();
            tojoin.UserId = userid;
            tojoin.ActivityId = actid;
            _context.CalandarEvents.Add (tojoin);
            _context.SaveChanges ();
            return RedirectToAction ("Dashboard");
        }

        [HttpGet ("Leave/{userid}/{actid}")]
        public IActionResult Leave (int userid, int actid) {
            CalandarEvent toleave = _context.CalandarEvents.FirstOrDefault (x => x.ActivityId == actid && x.UserId == userid);
            _context.CalandarEvents.Remove (toleave);
            _context.SaveChanges ();
            return RedirectToAction ("Dashboard");
        }

        [HttpGet ("cancel/{actid}")]
        public IActionResult Cancel (int actid) {
            DojoActivity tocancel = _context.DojoActivities.FirstOrDefault (act => act.ActivityId == actid);
            _context.DojoActivities.Remove (tocancel);
            _context.SaveChanges ();
            return RedirectToAction ("Dashboard");
        }

        [HttpGet ("Display/{actId}")]
        public IActionResult Display (int actId) {
            int? userid = HttpContext.Session.GetInt32 ("userid");
            if (userid == null) {
                return RedirectToAction ("Logout");
            } else {
                ViewBag.User = _context.Users.FirstOrDefault (u => u.UserId == userid);
                DojoActivity activity = _context.DojoActivities.Include (act => act.Coordinator).Include(a => a.CalandarActivities).ThenInclude (c => c.UserJoin).FirstOrDefault(act => act.ActivityId == actId);
                // _context.DojoActivities.Include (a => a.Coordinator).Include (a => a.CalandarActivities).ThenInclude (c => c.UserJoin)
                return View ("Display", activity);
            }
        }
    }
}