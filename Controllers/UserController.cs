using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using The_Wall.Models;
using DbConnection;
using Login_Register.Models;
using Microsoft.AspNetCore.Http;

namespace The_Wall.Controllers
{
    public class UserController : Controller
    {
        private readonly DbConnector _dbConnector;

        public UserController(DbConnector connect)
        {
            _dbConnector = connect;
        }

        [HttpGet]
        [Route("Login")]
        public IActionResult Login()
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            string UserName = HttpContext.Session.GetString("UserName");

            if(UserId!=null){
                return RedirectToAction("Wall", "Wall");
            }else{
                return View("Login");
            }
        }

        [HttpGet]
        [Route("Logoff")]
        public IActionResult Logoff()
        {
            HttpContext.Session.Clear();
            return View("Login");
        }

        [HttpGet]
        [Route("Register")]
        public IActionResult Register()
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            string UserName = HttpContext.Session.GetString("UserName");

            if(UserId!=null){
                return RedirectToAction("Wall", "Wall");
            }else{
                return View("Register");
            }
        }

        [HttpPost]
        [Route("Register")]
        public IActionResult Register(RegisterViewModel model)
        {

            if(ModelState.IsValid)
            {
                User newUser = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Password = model.Password,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                List<Dictionary<string, object>> user = _dbConnector.Query($"SELECT email FROM users WHERE email='{newUser.Email}'");
                model.Unique = user.Count();
                TryValidateModel(model);

                if(!ModelState.IsValid){
                    return View("Register");
                }else{
                    _dbConnector.Execute($"INSERT INTO users (first_name, last_name, email, password, created_at, updated_at) VALUES ('{newUser.FirstName}', '{newUser.LastName}', '{newUser.Email}', '{newUser.Password}', now(), now())");

                    return RedirectToAction("Wall", "Wall");
                }
            }

            return View("Register");
        }

        [HttpPost]
        [Route("Login")]
        public IActionResult Login(LoginViewModel model)
        {

            List<Dictionary<string, object>> user = _dbConnector.Query($"SELECT * FROM users WHERE email='{model.Email}'");
            
            model.Found = user.Count() - 1;

            if(model.Found == 0){
                model.PasswordConfirmation = ((string)user[0].GetValueOrDefault("password") == model.Password)?0:1; 
            }

            TryValidateModel(model);

            if(ModelState.IsValid)
            {

                HttpContext.Session.SetInt32("UserId", (int)user[0]["id"]);
                HttpContext.Session.SetString("UserName", (string)user[0]["first_name"]);
                return RedirectToAction("Wall", "Wall");
                
            }

            return View("Login");
        }

    }
}
