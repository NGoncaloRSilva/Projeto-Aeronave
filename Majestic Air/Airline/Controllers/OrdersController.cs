﻿using Airline.Data.Entities;
using Airline.Data.Repositories;
using Airline.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Airline.Helpers;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MailKit.Search;
using System.Collections;

namespace Airline.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IMailHelper _mailHelper;

        public OrdersController(IOrderRepository orderRepository, ITicketRepository ticketRepository, IUserHelper userHelper,
            IConverterHelper converterHelper, IMailHelper mailHelper)
        {
            _orderRepository = orderRepository;
            _ticketRepository = ticketRepository;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
            _mailHelper = mailHelper;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var model = await _orderRepository.GetOrderAsync(this.User.Identity.Name);

            return View(model);
        }
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var model = await _orderRepository.GetDetailsTempsAsync(this.User.Identity.Name);

            return View(model);
        }

        public IActionResult AddTicket()
        {
            var model = new TicketViewModel
            {
                
                Flights = _ticketRepository.GetComboFlight(),
                Classes = _ticketRepository.GetComboClass(0),
                Seatss = _ticketRepository.GetComboSeats(0),
            };


            


            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> AddTicket(TicketViewModel ticket)
        {
            if (ModelState.IsValid)
            {
                


                Guid imageId = Guid.Empty;
                    ticket = await _ticketRepository.AddFlightAsync(ticket);

                    var product = _converterHelper.toTicket(ticket, imageId, true);

                    string inicial = product.Seat.Classe.Class.Substring(0, 1);
                
                    var lista = _ticketRepository.GetcomboTicket();
                    //Random _random = new Random();

                    string number1 = ticket.Seat.Name + (lista.Count() + 1).ToString();
                
                    product.Code = number1;

                product.User = await _userHelper.GetUserbyEmailAsync(this.User.Identity.Name);


                await _ticketRepository.CreateAsync(product);

                ticket.Id = lista.Count() + 1;



                await _orderRepository.AddItemToOrderAsync(ticket, this.User.Identity.Name);
                return RedirectToAction("Create");
            }

            return View(ticket);
        }

        public async Task<IActionResult> DeleteItem(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            await _orderRepository.DeleteDetailtempAsync(id.Value);
            return RedirectToAction("Create");
        }

        //public async Task<IActionResult> Increase(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    await _orderRepository.ModifyOrderDetailTempQuantity(id.Value, 1);
        //    return RedirectToAction("Create");
        //}

        //public async Task<IActionResult> Decrease(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    await _orderRepository.ModifyOrderDetailTempQuantity(id.Value, -1);
        //    return RedirectToAction("Create");
        //}

        [Authorize]
        public async Task<IActionResult> ConfirmOrder()
        {
            var response = await _orderRepository.ConfirmOrderAsync(this.User.Identity.Name);
            if (response)
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Create");

        }

        public async Task<IActionResult> Deliver(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderRepository.GetOrderAsync(id.Value);

            

            return View(order);

        }

        [HttpPost]
        public async Task<IActionResult> Deliver(Order model)
        {
            if (ModelState.IsValid)
            {
                if(model.Status == "Canceled")
                {
                    return NotFound();
                }

                model = await _orderRepository.DeliveryOrder(model);
                string str = "";

                foreach (var item in model.Items)
                {
                    str += item.Ticket.Seat.FlightId  + "-" + item.Ticket.Seat.Name + " ";
                }

                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(model.User);
                string tokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userid = model.User.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendEmail(model.User.UserName, "MajesticAir Order", $"<h1>MajesticAir Order</h1>" +
                    $"Ticket Code(s): {str}");


                if (response.IsSuccess)
                {
                    ViewBag.Message = "Your tickets have been sent to your email";
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError(string.Empty, "E-mail couldnt be Sent.");

                return RedirectToAction("Index");
            }



            return View(model);
        }

        public async Task<IActionResult> DeleteOrder(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _orderRepository.GetOrderAsync(id.Value);

            if (order.Status == "Canceled")
            {
                return NotFound();
            }



            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            
                
           var product = await _orderRepository.GetByIdAsync(id);
            
            await _orderRepository.DeleteOrder(product);

            return RedirectToAction("Index");


        }

        



        //[HttpPost]
        //[Route("Orders/GetClassesAsync")]
        //public async Task<JsonResult> GetClassesAsync(int flightId)
        //{
        //    var classes = await _ticketRepository.VerifyStock(flightId);


        //    return Json(classes.OrderBy(c => c.Value));
        //}

        [HttpPost]
        [Route("Orders/GetSeatsAsync")]
        public async Task<JsonResult> GetSeatsAsync(int flightId)
        {
            var seats = await _ticketRepository.VerifySeats(flightId);


            return Json(seats.OrderBy(c => c.Text));
        }
    }
}
