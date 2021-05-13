using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Ebus.Models;
using System.Device.Location;
using Microsoft.AspNetCore.Http;

namespace Ebus.Controllers
{
    public class DriverController : Controller
    {
        
        private readonly IConfiguration configuration;
        
        public DriverController(IConfiguration config)
        {
            this.configuration = config;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult driverPage()
        {
            if (HttpContext.Session.GetString("driver") != null)
            {
                
                return View();
            }
            else
            {
                return RedirectToAction("firstview","Home");
            }
        }
        public IActionResult requestList()
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string nm = HttpContext.Session.GetString("driver");
            string id = getID();
            string query = "select * from ticket where number_plate in (select number_plate from bus where dri_id ='"+id.ToString()+"')";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            var model = new List<RequestList>();
            while(reader.Read())
            {
                RequestList rq = new RequestList();
                rq.id = reader["cusid"].ToString();
                rq.code = reader["code"].ToString();
                model.Add(rq);
            }
            connection.Close();
            return View(model);
        }
        public IActionResult checkEntry()
        {
            if (HttpContext.Session.GetString("driver") != null)
            {

                return View();
            }
            else
            {
                return RedirectToAction("firstview", "Home");
            }
        }
        public void inputFromCheck(string code)
        {
            if(String.IsNullOrEmpty(code))
            {
                Response.Redirect("checkEntry");
            }
            else
            {
                string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                string nm = HttpContext.Session.GetString("driver");
                string numberPlate = getNumberPlate(nm);
                string query = "select count(*) from ticket where number_plate = '" + numberPlate.ToString() + "' and code = '"+code.ToString()+"'";
                SqlCommand com = new SqlCommand(query, connection);
                var count = (int)com.ExecuteScalar();
                connection.Close();
                if (count == 1)
                {
                    deleteTicketCode(code.ToString());
                    upDateBookSeat();
                    Response.Redirect("Varify");

                }
                
            }
        }
        public void upDateBookSeat()
        {
            string bookedSeat = HttpContext.Session.GetString("BookedSeat");
            string[] bSplit = bookedSeat.Split(",");
            string m = bSplit[0];
            string f = bSplit[1];
            string total = getTotalSeat();
            string[] totalSplit = total.Split(",");
            int maleBook = Int32.Parse(totalSplit[0].ToString()) - Int32.Parse(m.ToString());
            int femaleBook = Int32.Parse(totalSplit[1].ToString()) - Int32.Parse(f.ToString());
            int maleOcc = Int32.Parse(totalSplit[2].ToString()) + Int32.Parse(m.ToString());
            int femaleOcc = Int32.Parse(totalSplit[3].ToString()) + Int32.Parse(f.ToString());
            string nm = HttpContext.Session.GetString("driver");
            string numberPlate = getNumberPlate(nm);
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            
            string query = "update bus_state set maleSeat_booked = '"+maleBook.ToString()+"', femaleSeat_booked= '"+femaleBook.ToString()+"', maleSeat_occupied = '"+maleOcc.ToString()+"',femaleSeat_occupied='"+femaleOcc.ToString()+"' where number_plate = '"+numberPlate.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.ExecuteNonQuery();
            connection.Close();
        }
        public string getTotalSeat()
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string nm = HttpContext.Session.GetString("driver");
            string numberPlate = getNumberPlate(nm);
            string query = "select maleSeat_booked, femaleSeat_booked, maleSeat_occupied,femaleSeat_occupied from bus_state where number_plate = '"+numberPlate.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string malebook = "";
            string femalebook = "";
            string maleOcc = "";
            string femaleOcc = "";
            while (reader.Read())
            {
                malebook = reader["maleSeat_booked"].ToString();
                femalebook = reader["femaleSeat_booked"].ToString();
                maleOcc = reader["maleSeat_occupied"].ToString();
                femaleOcc = reader["femaleSeat_occupied"].ToString();
            }
            connection.Close();
            string p = malebook + "," + femalebook + "," + maleOcc + "," + femaleOcc;
            return p;

        }
        public IActionResult Varify()
        {
            if (HttpContext.Session.GetString("driver") != null)
            {
                ViewData["p"] = "Passenger is varified";
                return View();
            }
            else
            {
                return RedirectToAction("firstview", "Home");
            }
        }
        public void deleteTicketCode(string c)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "delete from ticket where code = '"+c.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.ExecuteNonQuery();
            connection.Close();
        }
        public void Trip_start()
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string nm = HttpContext.Session.GetString("driver");
            string numberPlate = getNumberPlate(nm);
            string z = "0";
            String query = "Insert into bus_state values('" + numberPlate.ToString() + "','" + z.ToString() + "','" + z.ToString() + "','" + z.ToString() + "','" + z.ToString() + "','" + z.ToString() + "','" + z.ToString() + "','" + z.ToString() + "','" + z.ToString() + "')";
            SqlCommand com = new SqlCommand(query, connection);
            com.ExecuteNonQuery();
            connection.Close();
            Response.Redirect("driverPage");
        }
        public string busServiceForCurrentMethod()
        {
            string nm = HttpContext.Session.GetString("driver");
            string id = getID();
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select bus_service from bus where dri_id = '" + id.ToString() + "'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string busService = "";
            while(reader.Read())
            {
                busService = reader["bus_service"].ToString();

            }
            connection.Close();
            return busService;
        }
        public void current(string location)
        {
            string busService = busServiceForCurrentMethod();
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select route from services where bus_service = '"+busService.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string route = "";
            while(reader.Read())
            {
                route = reader["route"].ToString();
            }
            connection.Close();
            string[] splitRoute = route.Split(",");
            int cnt = 0;
            foreach(var i in splitRoute)
            {
                if (i == location.ToString())
                {
                    cnt++;
                    break;
                }
                else
                    cnt++;
            }
            string p = "";
            p = location.ToString() +","+ cnt.ToString();
            InsertLocationAndIndex(p);
            Response.Redirect("driverRoute");

        }
        public void InsertLocationAndIndex(string p)
        {
            string nm = HttpContext.Session.GetString("driver");
            string numberPlate = getNumberPlate(nm);
            string loc = p.ToString();
            string[] locSplit = loc.Split(",");
            string location = locSplit[0];
            string count = locSplit[1];
            string busService = busServiceForCurrentMethod();
            string lonAndLati = GetLongitudeAndLatitude(location);
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "update bus_state set currentLocation='" + location.ToString() + "',currentIndex='" + count.ToString() + "', busLocation = '"+lonAndLati.ToString()+"' where number_plate ='" + numberPlate.ToString() + "' ";
            SqlCommand com = new SqlCommand(query, connection);
            com.ExecuteNonQuery();
            connection.Close();

        }
        public string GetLongitudeAndLatitude(string location)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select location from Bus_stop where stop = '"+location.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string p = "";
            while(reader.Read())
            {
                p = reader["location"].ToString();
            }
            return p;
        }
        public IActionResult driverRoute()
        {
            //location code start
            //location code end

            passLocation();
            
            //ViewData["loca"] = location.ToString();
            // ViewData["loca"] = HttpContext.Session.GetString("loc").ToString();
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select route from services where bus_service in  (select bus_service from bus where dri_id=2002)";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            var model = new List<busStopFromDriver>();
            string s = "";
            while(reader.Read())
            {
                
                s = reader["route"].ToString();
                
            }
            connection.Close();
            string[] stopList = s.Split(",");
            foreach(var i in stopList)
            {
                busStopFromDriver bs = new busStopFromDriver();
                bs.stop = i;
                model.Add(bs);
            }
            return View(model);
        }
        
       public void passLocation()
        {
           
            GeoCoordinateWatcher watcher;
            watcher = new GeoCoordinateWatcher();
            watcher.Start();
            string location = "";
            
            watcher.PositionChanged += (sender, e) =>
            {
                var coordinate = e.Position.Location;
                Console.WriteLine("Lat: {0}, Long: {1}", coordinate.Latitude,
                    coordinate.Longitude);
                double lon = Double.Parse(coordinate.Longitude.ToString());
                double lat = Double.Parse(coordinate.Latitude.ToString());

              
                
                
                location = coordinate.Latitude.ToString()+"," + coordinate.Longitude.ToString();
                ViewData["loca"] = location.ToString();
                // Uncomment to get only one event.
                // HttpContext.Session.SetString("locc",location);
               /* string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                //HttpContext.Session.SetString("loc", location.ToString());

                //string name = HttpContext.Session.GetString("driver");
                //string numberPlate = getNumberPlate(name);

                connection.Open();
                string query = "update bus_state set busLocation = '" + location + "' where number_plate = '4'";
                SqlCommand com = new SqlCommand(query, connection);
                com.ExecuteNonQuery();
                connection.Close();*/
                watcher.Stop();
                





            };


            // Begin listening for location updates


            
        }
        /*public void loc(Double[] list)
        {
            double[] a= {0,0};
            int i = 0;
            foreach (double temp in list) {

                a[i] = temp;
                i++;
            }
            string location = a[0].ToString() + "," + a[1].ToString();
             string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                //HttpContext.Session.SetString("loc", location.ToString());

                string name = HttpContext.Session.GetString("driver");
                string numberPlate = getNumberPlate(name);

                connection.Open();
                string query = "update bus_state set busLocation = '" + location + "' where number_plate = '4'";
                SqlCommand com = new SqlCommand(query, connection);
                com.ExecuteNonQuery();
                connection.Close();
        }*/
        public string getID()
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string name = HttpContext.Session.GetString("driver");
            string query = "select dri_id from driver where name = '"+name.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string id = "";
            while(reader.Read())
            {
                id = reader["dri_id"].ToString();
            }
            connection.Close();
            return id;
        }
        public string getNumberPlate(string name)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string id = getID();
            string query = "Select number_plate from bus where dri_id = '"+id.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string numberPlate = "";
            while(reader.Read())
            {
                numberPlate = reader["number_plate"].ToString();
            }
            connection.Close();
            return numberPlate;
        }
    }
}