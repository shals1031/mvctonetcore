using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using TeliconLatest.DataEntities;
using System.Linq;
using System.IO;

namespace TeliconLatest.Controllers
{
    public class CustomerSignController : Controller
    {
        private readonly TeliconDbContext Db;
        public CustomerSignController(TeliconDbContext db)
        {
            Db = db;
        }
        public IActionResult Index()
        {
            Customer customer = new Customer { JobDate = DateTime.Today, CustDate = DateTime.Today, TechDate = DateTime.Today };
            customer.CustomerEquipments = new();
            for (int ij = 0; ij < 5; ij++)
            {
                customer.CustomerEquipments.Add(new());
            }
            return View(customer);
        }
        [HttpPost]
        public IActionResult Index(Customer customer)
        {
            if (ModelState.IsValid)
            {
                List<CustomerEquipments> customerEquipments = customer.CustomerEquipments.Where(x => string.IsNullOrEmpty(x.MacDetail) && string.IsNullOrEmpty(x.SerialNo) && string.IsNullOrEmpty(x.Description)).ToList();
                foreach (var item in customerEquipments)
                {
                    if (string.IsNullOrEmpty(item.MacDetail) || string.IsNullOrEmpty(item.SerialNo) || string.IsNullOrEmpty(item.Description))
                        customer.CustomerEquipments.Remove(item);
                }
                if (!string.IsNullOrEmpty(customer.CustSignature))
                {
                    string fileName = Guid.NewGuid() + ".png";
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SignImages");
                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);

                    System.IO.File.WriteAllBytes(filePath + "/" + fileName, Convert.FromBase64String(customer.CustSignature.Replace("data:image/png;base64,", "")));
                    customer.CustSignature = fileName;
                }
                if (!string.IsNullOrEmpty(customer.TechSignature))
                {
                    string fileName = Guid.NewGuid() + ".png";
                    string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "SignImages");
                    if (!Directory.Exists(filePath))
                        Directory.CreateDirectory(filePath);

                    System.IO.File.WriteAllBytes(filePath + "/" + fileName, Convert.FromBase64String(customer.TechSignature.Replace("data:image/png;base64,", "")));
                    customer.TechSignature = fileName;
                }

                Db.Customers.Add(customer);
                Db.SaveChanges();

                if (customer.CustomerId > 0)
                {
                    TempData["SuccessMsg"] = "Your application saved successfully.";
                    return RedirectToAction("Index");
                }
                else
                    TempData["ErrorMsg"] = "Error occurred while saving application.";
            }
            return View(customer);
        }
    }
}
