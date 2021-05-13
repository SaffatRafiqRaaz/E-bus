using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Ebus.Models;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Http;

namespace Ebus.Controllers
{
    
    public class AdminController : Controller
    {
        private readonly IConfiguration configuration;

        public AdminController(IConfiguration config)
        {
            this.configuration = config;
        }
        public IActionResult notAccess()
        {
            return View();
        }
        public IActionResult adminView()
        {
            if (HttpContext.Session.GetString("admin") != null)
                return View();
            else
                return RedirectToAction("notAccess", "Admin");
        }
        public IActionResult showPassenger()
        {
            if (HttpContext.Session.GetString("admin") != null)
            {
                string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                string query = "select * from cusinfo";
                SqlCommand com = new SqlCommand(query, connection);
                com.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = com.ExecuteReader();
                var model = new List<showcustomer>();
                while (reader.Read())
                {
                    showcustomer sc = new showcustomer();
                    sc.name = reader["name"].ToString();
                    sc.email = reader["email"].ToString();
                    sc.phone = reader["phone"].ToString();
                    sc.gender = reader["gender"].ToString();
                    sc.physical = reader["physically_Disable"].ToString();
                    model.Add(sc);
                }
                connection.Close();
                return View(model);
            }
            else
            {
                return RedirectToAction("notAccess", "Admin");
            }
        }
        public void removePassenger(string eml)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "Delete from cusinfo where email='"+eml.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.ExecuteNonQuery();
            Response.Redirect("showPassenger");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("admin");
            return RedirectToAction("firstview","Home");

        }
        public IActionResult daily()
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select * from record";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            var model = new List<Record>();
            while (reader.Read())
            {
                Record r = new Record();
                r.driverId = reader["dri_id"].ToString();
                r.customerId = reader["cusId"].ToString();
                r.busNumber = reader["number_plate"].ToString();
                r.fare = reader["fare"].ToString();
                r.to = reader["locationStart"].ToString();
                r.from = reader["locationEnd"].ToString();
                r.date = reader["date"].ToString();
                model.Add(r);
            }
            return View(model);

        }
        public IActionResult addBusStop()
        {
            return View();
        }
        public void inputFromBusStop(string stop, string latitude, string longitude)
        {
            if(String.IsNullOrEmpty(stop.ToString()) || String.IsNullOrEmpty(latitude.ToString()) || String.IsNullOrEmpty(longitude.ToString()))
            {
                Response.Redirect("empty");
            }
            else
            {
                string p = latitude.ToString() + "," + longitude.ToString();
                string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                string query = "Insert into Bus_stop values('"+stop.ToString()+"','"+p.ToString()+"')";
                SqlCommand com = new SqlCommand(query, connection);
                com.ExecuteNonQuery();
                Response.Redirect("adminView");
            }
        }
        public IActionResult empty()
        {
            return View();
        }
    }
}