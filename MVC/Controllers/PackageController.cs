using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using mvc.Migrations;
using mvc.Models;
using mvc.RepoInterfaces;
using System.Threading.Tasks;
using mvc.ViewModels.PackageVM;

namespace mvc.Controllers
{
    public class PackageController : Controller
    {
        IPackageRepository _packageRepository;
        public PackageController(IPackageRepository packageRepository)
        {
            _packageRepository = packageRepository;
        }
        public async Task<IActionResult> GetAll()
        {
            List<Package> packages = await _packageRepository.GetAll().ToListAsync();
            return View(packages);

        }
        public async Task<IActionResult> GetById(int id)
        {
            Package package =await _packageRepository.GetByIdAsync(id);
            return View(package);
        }
        public IActionResult Add()
        {
            return View();
        }
        public async Task<IActionResult> SaveAdd(AddPackageVM newPackage)
        {
            if (_packageRepository.IsExist(newPackage.Name))
            {
                ModelState.AddModelError("Name", "This name already exists");
            }
            if (!ModelState.IsValid)
            {
                return View("Add", newPackage);
            }

            Package package = new Package
            {
                Name = newPackage.Name,
                MonthlyPrice = newPackage.MonthlyPrice,
                YearlyPrice = newPackage.YearlyPrice,
            };
            await _packageRepository.AddAsync(package);
            await _packageRepository.SaveAsync();
            return RedirectToAction("GetAll");
        }
        public async Task<IActionResult> Delete(int id)
        {
            await _packageRepository.DeleteAsync(id);
            await _packageRepository.SaveAsync();
            return RedirectToAction("GetAll");
        }
        public async Task<IActionResult> Edit(int id)
        {
            Package package = await _packageRepository.GetByIdAsync(id);
            UpdatePackageVM updatePackageVM = new UpdatePackageVM
            {
                Id = package.Id,
                Name = package.Name,
                MonthlyPrice = package.MonthlyPrice,
                YearlyPrice = package.YearlyPrice,
            };

            return View(updatePackageVM);
        }
        public async Task<IActionResult> SaveUpdate(int id,UpdatePackageVM updatedPackage)
        {
            if (_packageRepository.IsExist(updatedPackage.Name))
            {
                ModelState.AddModelError("Name", "This name already exists");
            }
            if (!ModelState.IsValid)
            {
                return View("Edit", updatedPackage);
            }
            Package package = new Package
            {
                Id = id,
                Name = updatedPackage.Name,
                MonthlyPrice = updatedPackage.MonthlyPrice,
                YearlyPrice = updatedPackage.YearlyPrice,
            };
            _packageRepository.Update(package);
            await _packageRepository.SaveAsync();
            return RedirectToAction("GetAll");
        }

    }
}
