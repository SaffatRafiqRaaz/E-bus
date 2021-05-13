using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ebus.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace Ebus.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration configuration;
        public HomeController(IConfiguration config)
        {
            this.configuration = config;
        }
        public IActionResult firstview()
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            SqlCommand com = new SqlCommand("select  count(*) from cusInfo",con);
            var count = (int)com.ExecuteScalar();
            ViewData["TotalData"] = count;
            con.Close();
            return View();
        }

        public IActionResult About()
        {
           

            return View();
        }

        public IActionResult Contact()
        {
            

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
