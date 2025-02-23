﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Airline.Data;
using Airline.Data.Entities;
using Airline.Helpers;
using Airline.Data.Repositories;
using Airline.Models;
using Microsoft.AspNetCore.Authorization;
using Vereyon.Web;

namespace Airline.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ModelsController : Controller
    {
        private readonly IModelRepository _modelRepository;
        private readonly IUserHelper _userHelper;
        private readonly IBlobHelper _blobHelper;
        //private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly IFlashMessage _flashMessage;

        public ModelsController(IModelRepository modelRepository, IUserHelper userHelper, IBlobHelper blobHelper, IConverterHelper converterHelper
            , IFlashMessage flashMessage)
        {
            _modelRepository = modelRepository;
            _userHelper = userHelper;
            _blobHelper = blobHelper;
            //_imageHelper = imageHelper;
            _converterHelper = converterHelper;
            _flashMessage = flashMessage;
        }

        // GET: Models
        
        public IActionResult Index()
        {
            return View(_modelRepository.GetAll().OrderBy(p => p.Name));
        }

        // GET: Models/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            var model = await _modelRepository.GetByIdAsync(id.Value);
            if (model == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            return View(model);
        }

        // GET: Models/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Models/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ModelViewModel model)
        {
            if (ModelState.IsValid)
            {
                Guid imageId = Guid.Empty;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {


                    imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "models");

                }


                var product = _converterHelper.toModel(model, imageId, true);




                product.User = await _userHelper.GetUserbyEmailAsync(this.User.Identity.Name);

                try
                {
                    await _modelRepository.CreateAsync(product);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    _flashMessage.Danger("This country already exist!");
                }
            }
            return View(model);
        }

        // GET: Models/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            var model = await _modelRepository.GetByIdAsync(id.Value);
            if (model == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }
            var viewmodel = _converterHelper.toModelViewModel(model);
            return View(viewmodel);
        }

        // POST: Models/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,ModelViewModel model)
        {
            

            if (ModelState.IsValid)
            {
                try
                {
                    Guid imageId = Guid.Empty;

                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {


                        imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "models");

                    }


                    var product = _converterHelper.toModel(model, imageId, false);


                    product.User = await _userHelper.GetUserbyEmailAsync(this.User.Identity.Name);
                    await _modelRepository.UpdateAsync(product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _modelRepository.ExistAsync(model.Id))
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
            return View(model);
        }

        // GET: Models/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            var model = await _modelRepository.GetByIdAsync(id.Value);
            if (model == null)
            {
                return new NotFoundViewResult("ProductNotFound");
            }

            return View(model);
        }

        // POST: Models/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _modelRepository.GetByIdAsync(id);
            try
            {
                await _modelRepository.DeleteAsync(product);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"{product.Name} provavelmente está a ser usado!!";
                    ViewBag.ErrorMessage = $"{product.Name} não pode ser apagado visto haverem encomendas que o usam.</br></br>" +
                       $"Exprimente primeiro apagar todas as encomendas que o estão a usar," +
                       $"e torne novamente a apagá-lo";
                }




                return View("Error");

            }
            
        }

        public IActionResult ProductNotFound()
        {
            return View();
        }
    }
}
