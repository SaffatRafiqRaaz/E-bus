using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Session;


namespace Ebus.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration configuration;
        
        public LoginController(IConfiguration config)
        {
            this.configuration = config;
        }
        public IActionResult CustomerLogin()
        {
            

            return View();
        }
        public IActionResult CustomerRegistration()
        {
            return View();
        }
        public IActionResult adminLogin()
        {
            return View();
        }
        public IActionResult OwnerLogin()
        {
            return View();
        }
        public IActionResult OwnerRegistration()
        {
            return View();
        }
        public IActionResult DriverRegistration()
        {
            return View();
        }
        public IActionResult driverLogin()
        {
            return View();
        }
        public IActionResult inputFromAdminLogin(string name , string pwd)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select count(*) from admin where email = '"+name.ToString()+"' and password = '"+pwd.ToString()+"'";
            SqlCommand com = new SqlCommand(query,connection);
            var count = (int)com.ExecuteScalar();
            connection.Close();
            if (count == 1)
            {
                HttpContext.Session.SetString("admin", name.ToString());
                return RedirectToAction("adminView","Admin");
            }
            else
                return View();

        }
       
        public void InputFromOwnerRegistration(string name, string email, string fpwd, string company, string trade_license)
        {
            string z = "0";
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "Insert into owner values('" + name.ToString() + "','" + email.ToString() + "','" + fpwd.ToString()+ "','" + company.ToString() + "','"+trade_license.ToString()+"','"+z+"')";
            SqlCommand com = new SqlCommand(query, connection);
            com.ExecuteNonQuery();
            connection.Close();
            Response.Redirect("OwnerRegistration");
        }
        public void InputFromCustomerRegistration(string name, string email, string fpwd, string spwd, string phone, string radio, string physically_Disable)
        {
            string password = "";
            string zero = "0";
            if(fpwd.Equals(spwd))
            {
                password = encryption(spwd);
            }
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "Insert into cusInfo values('"+name.ToString()+"','"+email.ToString() + "','"+phone.ToString() + "','"+radio.ToString() + "','"+password+"','"+ zero + "','"+physically_Disable.ToString() + "')";
            SqlCommand com = new SqlCommand(query,connection);
            com.ExecuteNonQuery();
            connection.Close();
            Response.Redirect("CustomerLogin");
            
        }

        public IActionResult InputFromOwnerSignin(string email, string password)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select count(*) from owner where email = '" + email.ToString() + "' and pass = '" + password.ToString() + "'";
            SqlCommand com = new SqlCommand(query, connection);
            var count = (int)com.ExecuteScalar();
            connection.Close();
            if (count == 1)
            {
                HttpContext.Session.SetString("owner", email.ToString()); ;
                TempData["owner"] = email.ToString();
                return RedirectToAction("OwnerView", "Owner");
            }
            else
                return View();

        }
        public IActionResult inputFormCustomerLogin(string email, string password)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string a = encryption(password.ToString());
            string query = "select count(*) from cusinfo where email = '" + email.ToString() + "' and password = '" + a + "'";
            SqlCommand com = new SqlCommand(query, connection);
            var count = (int)com.ExecuteScalar();
            if(count == 1)
            {
                HttpContext.Session.SetString("user", email.ToString());
                return RedirectToAction("PassengerView", "Passenger",new { name = email.ToString() });
            }
            connection.Close();

            return View();
        }
        public IActionResult inputFormDriverLogin(string email, string password)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            
            string query = "select count(*) from driver where name = '" + email.ToString() + "' and dri_id = '" + password.ToString() + "'";
            SqlCommand com = new SqlCommand(query, connection);
            var count = (int)com.ExecuteScalar();
            connection.Close();
            if (count == 1)
            {
                HttpContext.Session.SetString("driver", email.ToString());
                return RedirectToAction("driverPage", "Driver");
            }
            else
                return View();

            
        }
        public string encryption(string password)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            UTF8Encoding encoder = new UTF8Encoding();
            Byte[] originalBytes = encoder.GetBytes(password);
            Byte[] encodedBytes = md5.ComputeHash(originalBytes);
            password = BitConverter.ToString(encodedBytes).Replace("-", "");
            var result = password.ToLower();
            return result;
        }
    }
}