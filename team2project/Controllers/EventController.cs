﻿using System;
using System.Web.Mvc;
using team2project.Models;
using DAL.Models;
using BLL.Classes;

namespace team2project.Controllers
{
    public class EventController : Controller
    {
        BusinessLogicLayer <EventViewModel, EventModel> Bll;


        [HttpGet]
        public ActionResult Index()
        {
            Bll = new BusinessLogicLayer<Models.EventViewModel, EventModel>();
            return View("List", Bll.GetList());
        }

        
        [HttpGet]
        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /EmployeeInfo/Create

        [HttpGet]
        public ActionResult Create()
        {
            var Event = new EventViewModel();
            return View(Event);
        }

        //
        // POST: /EmployeeInfo/Create

        [HttpPost]
        public ActionResult Create(EventViewModel evnt)
        {
            try
            {
                evnt.Id = Guid.NewGuid().ToString();
                Bll.Create(evnt);
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //[HttpGet]
        //public ActionResult Details(uint id)
        //{
        //    var model = new DataProvider().GetById(id);
        //    if (model == null)
        //    {
        //        return RedirectToAction("Index", new { controller = "Error", action = "Index" });
        //    }
        //    return View(model);
        //}

    }
}