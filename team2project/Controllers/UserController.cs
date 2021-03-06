﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using System.Net;
using System.Net.Mime;
using System.Net.Mail;
using Events.Business;
using Events.Business.Classes;
using Events.Business.Models;
using Events.Business.Interfaces;
using Events.NHibernateDataProvider;
using team2project.Models;

using System.IO;

namespace team2project.Controllers
{
    public abstract class MyBaseController : Controller
    {
        protected string RenderPartialViewToString(string viewName, object model) {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.RouteData.GetRequiredString("action");

            ViewData.Model = model;

            using (StringWriter sw = new StringWriter()) {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                ViewContext viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);

                return sw.GetStringBuilder().ToString();
            }
        }
    }

    public class UserController : MyBaseController
    {
        //
        // GET: /User/

        IUserDataProvider data;
        EventManager eventManager;
        



        public UserController(IUserDataProvider data, EventManager eventManager)
        {
            this.data = data;
            this.eventManager = eventManager;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            string returnUrl = "";
            if (Request.UrlReferrer != null)
            {
                returnUrl = Server.UrlEncode(Request.UrlReferrer.PathAndQuery);
            }            
            if (string.IsNullOrEmpty(returnUrl) == false)
            {
                ViewBag.ReturnUrl = returnUrl;
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = data.GetByMail(email);
                if (user != null && isValid(email, password))
                {
                    if (user.IsActive == true)
                    {
                        string decodedUrl = "";
                        if (string.IsNullOrEmpty(returnUrl) == false)
                            decodedUrl = Server.UrlDecode(returnUrl);

                        FormsAuthentication.SetAuthCookie(email, false);

                        if (string.IsNullOrEmpty(decodedUrl) == false && decodedUrl.ToLower() != "/user/thankyoupage")
                        {
                            return Redirect(decodedUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Event");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Завершите регистрацию перейдя по ссылке на вашей почте");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Неправильный e-mail или пароль");
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Event");
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            if (ModelState.IsValid)
            {
                var user = data.GetByMail(email);
                if (user != null)
                {
                    var crypto = new SimpleCrypto.PBKDF2();

                    string newPassword = GeneratePassword();

                    var encrPass = crypto.Compute(newPassword);

                    user.Password = encrPass;
                    user.PasswordSalt = crypto.Salt;

                    data.UpdateUser(user);

                    MailMessage msg = new MailMessage();

                    string body = user.Name + ", ваш пароль: \n" + newPassword;
                    string subject = "Новый пароль";

                    EmailSender(user.Email, body, subject);
                }
            }
            return RedirectToAction("ThankYouPage", "User");
        }

        public ActionResult ThankYouPage()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration(UserViewModel user)
        {
            if (ModelState.IsValid)
            {
                var userExist = data.GetByMail(user.Email);
                if (userExist == null)
                {
                    var crypto = new SimpleCrypto.PBKDF2();

                    var encrPass = crypto.Compute(user.Password);

                    User newUser = AutoMapper.Mapper.Map<User>(user);
                    newUser.Password = encrPass;
                    newUser.PasswordSalt = crypto.Salt;
                    newUser.IsActive = false;

                    data.CreateUser(newUser);

                    string authority = Request.Url.Authority;
                    string scheme = Request.Url.Scheme;
                    string activationLink = scheme + "://" + authority + Url.Action("Activate", new { controller = "User", action = "Activate", id = user.Id });

                    string body = user.Name + ", спасибо за регистрацию\n";
                    body += "Для активации аккаунта перейдите по ссылке\n" + activationLink;
                    string subject = "Подтверждение регистрации";
                    string newBody = RenderPartialViewToString("email", activationLink);
                    EmailSender(user.Email, newBody, subject);
                    
                    ViewBag.RegistrationSuccess = "Пожалуйста, подтвердите регистрацию перейдя по ссылке на вашей почте";
                }
                else
                {
                    ModelState.AddModelError("", "Пользователь с такой почтой уже зарегистрирован");                   
                }
            }
            return View(new UserViewModel());

        }

        [HttpGet]
        public ActionResult Activate(string id)
        {
            var user = data.GetById(id);
            if (user != null && user.IsActive == false)
            {
                user.IsActive = true;
                data.UpdateUser(user);
                FormsAuthentication.SetAuthCookie(user.Email, false);
                UserViewModel model = AutoMapper.Mapper.Map<UserViewModel>(user);
                return RedirectToRoute("Welcome", "User");
            }
            return RedirectToAction("Index", "Event");
        }

        public ActionResult Welcome()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Update()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Update(string oldPassword, string password)
        {
            if (ModelState.IsValid)
            {
                var crypto = new SimpleCrypto.PBKDF2();

                var mail = User.Identity.Name;
                User user = data.GetByMail(mail);

                if (user.Password == crypto.Compute(oldPassword, user.PasswordSalt))
                {
                    var encrPass = crypto.Compute(password);

                    user.Password = encrPass;
                    user.PasswordSalt = crypto.Salt;

                    data.UpdateUser(user);
                    ViewBag.PasswordSuccess = "Пароль изменен";
                }
                else
                {
                    ViewBag.OldPasswordError = "Неправильный старый пароль";
                }
            }
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult MyEvents()
        {
            IList<Event> events = eventManager.GetByAuthorMail(User.Identity.Name);
            List<EventViewModel> eventsModels = AutoMapper.Mapper.Map<List<EventViewModel>>(events);

            return View(eventsModels);
        }

        [Authorize]
        [HttpGet]
        public ActionResult MyPastEvents()
        {
            IList<Event> events = eventManager.GetAuthorPastEvents(User.Identity.Name);
            List<EventViewModel> eventsModels = AutoMapper.Mapper.Map<List<EventViewModel>>(events);

            return View(eventsModels);
        }

        [Authorize]
        [HttpGet]
        public ActionResult MyFutureEvents()
        {
            IList<Event> events = eventManager.GetAuthorFutureEvents(User.Identity.Name);
            List<EventViewModel> eventsModels = AutoMapper.Mapper.Map<List<EventViewModel>>(events);

            return View(eventsModels);
        }

        private bool isValid(string email, string password)
        {
            var crypto = new SimpleCrypto.PBKDF2();
            bool isValid = false;

            User user = data.GetByMail(email);
            if (user != null)
            {
                if (user.Password == crypto.Compute(password, user.PasswordSalt))
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        private string GeneratePassword()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        private void EmailSender(string userEmail, string body, string subject)
        {
            MailMessage msg = new MailMessage();

            msg.From = new MailAddress("team2project222@gmail.com");
            msg.To.Add(userEmail);
            msg.Subject = subject;
            msg.IsBodyHtml = true;
            msg.Body = body;

            ContentType mimeType = new System.Net.Mime.ContentType("text/html");
            AlternateView alternate = AlternateView.CreateAlternateViewFromString(body, mimeType);
            msg.AlternateViews.Add(alternate);

            msg.Priority = MailPriority.High;

            SmtpClient client = new SmtpClient();

            client.Send(msg);
        }

    }
}
