﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airline.Data;
using Airline.Data.Entities;
using Airline.Data.Repositories;
using Airline.Helpers;
using Airline.Models;

namespace Airline.Controllers
{
    public class AirshipsController : Controller
    {
        private readonly IAirshipRepository _airshipRepository;
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;

        public AirshipsController(IAirshipRepository airshipRepository, IUserHelper userHelper, IBlobHelper blobHelper, IConverterHelper converterHelper)
        {
            _airshipRepository = airshipRepository;
            _userHelper = userHelper;
            _converterHelper = converterHelper;
        }

        // GET: Airships
        public IActionResult Index()
        {
            return View(_airshipRepository.GetAll().OrderBy(p => p.AirshipName));
        }

        // GET: Airships/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            var model = await _airshipRepository.GetByIdAsync(id.Value);
            if (model == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            return View(model);
        }

        // GET: Airships/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Airships/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AirshipViewModel airship)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;
                
                var product = _converterHelper.toAirship(airship, imageId, true);

                //TODO: Modificar para o que tiver logado
                product.User = await _userHelper.GetUserbyEmailAsync(this.User.Identity.Name);
                await _airshipRepository.CreateAsync(product);
                //No generic repository grava automaticamente
                return RedirectToAction(nameof(Index));
            }
            return View(airship);
        }

        // GET: Airships/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            var model = await _airshipRepository.GetByIdAsync(id.Value);
            if (model == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }
            var viewmodel = _converterHelper.toAirshipViewModel(model);
            return View(viewmodel);
        }

        // POST: Airships/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AirshipViewModel airship)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Guid imageId = Guid.Empty;

                    var product = _converterHelper.toAirship(airship, imageId, false);


                    product.User = await _userHelper.GetUserbyEmailAsync(this.User.Identity.Name);
                    await _airshipRepository.UpdateAsync(product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _airshipRepository.ExistAsync(airship.Id))
                    {
                        return new NotFoundViewResult("ProductNotFound");
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(airship);
        }

        // GET: Airships/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            var model = await _airshipRepository.GetByIdAsync(id.Value);
            if (model == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            return View(model);
        }

        // POST: Airships/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _airshipRepository.GetByIdAsync(id);
            await _airshipRepository.DeleteAsync(product);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult ProductNotFound()
        {
            return View();
        }
    }
}