using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ebus.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;

namespace Ebus.Controllers
{
    
    public class OwnerController : Controller
    {
        
        private readonly IConfiguration configuration;

        public OwnerController(IConfiguration config)
        {
            this.configuration = config;
        }
        public IActionResult OwnerView()
        {
            if (HttpContext.Session.GetString("owner") != null)
                return View();
            else
                return RedirectToAction("notAccess","Admin");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Owner");
            return RedirectToAction("firstview", "Home");
        }
        public IActionResult addRoute()
        {
            if (HttpContext.Session.GetString("owner") != null)
            {
                string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                string query = "select * from Bus_stop";
                SqlCommand com = new SqlCommand(query, connection);
                com.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = com.ExecuteReader();

                var model = new List<route>();
                while (reader.Read())
                {
                    var rou = new route();
                    rou.busStop = reader["stop"].ToString();
                    rou.location = reader["location"].ToString();

                    model.Add(rou);
                }
                connection.Close();
                return View(model);
            }
            else
            {
                return RedirectToAction("notAccess", "Admin");
            }

        }
       
        public void inputFromOption(string input,String textBox)
        {
            
            string a = input.ToString();
           
            if(String.IsNullOrEmpty(textBox))
            {
                TempData["inputoption"] = a;
            }
            else
            {   string q = textBox.ToString()+ ",";
                string p = String.Concat(q,a);
               
                TempData["inputoption"] = p;
            }               
            
            Response.Redirect("addRoute");
        }
        public IActionResult inputFromaddroute(string textBox)
        {
            ViewData["checkid"] = textBox.ToString();
            return View();
        }
        public IActionResult addBuses()
        {
            if (HttpContext.Session.GetString("owner") != null)
            {
                string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                string query = "select driver.dri_id , name , number_plate from driver left join bus on driver.dri_id = bus.dri_id";
                SqlCommand com = new SqlCommand(query, connection);
                com.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = com.ExecuteReader();
                var model = new List<showdriverIdForOwner>();
                while (reader.Read())
                {
                    showdriverIdForOwner sd = new showdriverIdForOwner();
                    sd.driverId = reader["dri_id"].ToString();
                    sd.driverName = reader["name"].ToString();
                    sd.busNumber = reader["number_plate"].ToString();
                    model.Add(sd);
                }
                connection.Close();
                return View(model);
            }
            else
            {
                return RedirectToAction("notAccess", "Admin");
            }
        }
        public IActionResult routeValue(string totalRoute)
        {
            return View();
        }
        public IActionResult addDriver()
        {
            if (HttpContext.Session.GetString("owner") != null)
              return View();
            else
                return RedirectToAction("notAccess", "Admin");
        }
       
        public void inputFromDriver(string name, string license)
        {


            string ownerEmail= HttpContext.Session.GetString("owner");
            string ID = getId(ownerEmail);
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);

            connection.Open();
            string zero = "0";
            string qry = "insert into driver values('"+ID.ToString()+"','"+name.ToString()+"','"+license.ToString()+"','"+zero+"')";
            SqlCommand command = new SqlCommand(qry, connection);
            command.CommandType = System.Data.CommandType.Text;
            command.CommandType = System.Data.CommandType.Text;
            command.ExecuteNonQuery();
            connection.Close();
            Response.Redirect("addDriver");


        }
        public void inputFromBus(string busNumber, string driverId, string service,string maleSeat, string femaleSeat)
        {
            string ownerEmail = HttpContext.Session.GetString("owner");
            string id = getId(ownerEmail);
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string qry = "insert into bus values('"+busNumber.ToString()+"','"+id+"','"+driverId.ToString()+"','"+service.ToString()+"','"+maleSeat.ToString()+"','"+femaleSeat.ToString()+"')";
            SqlCommand command = new SqlCommand(qry,connection);
            command.CommandType = System.Data.CommandType.Text;
            command.ExecuteNonQuery();
            connection.Close();
            Response.Redirect("addBuses");
        }
        public string getId(string email)
        {
            string id = "";
            
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select Own_id from owner where email = '" + email.ToString() + "'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            while(reader.Read())
            {
                id = reader["Own_id"].ToString();
            }
            return id;
        }

    }
}