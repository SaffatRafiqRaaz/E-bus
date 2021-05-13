using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ebus.Models;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace Ebus.Controllers
{
    public class PassengerController : Controller
    {
        private readonly IConfiguration configuration;
        private static Random random = new Random();
        public PassengerController(IConfiguration config)
        {
            this.configuration = config;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult PassengerView(string name) {
            if (HttpContext.Session.GetString("user") != null)
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
                return RedirectToAction("notAccess","Admin");
            }
        }
        
        public IActionResult showBus(string from, string to)
        {
            if (HttpContext.Session.GetString("user") != null)
            {
                HttpContext.Session.SetString("fromAndto",from.ToString()+","+to.ToString());
                string LocationOfFrom = findStartingLocationValue(from.ToString());
                string[] sep = LocationOfFrom.Split(",");
                double lat1 = Convert.ToDouble(sep[0]);
                double lan1 = Convert.ToDouble(sep[1]);
                double lat2 = 0;
                double lan2 = 0;
                string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                string query = "select bus.number_plate, bus.bus_service , bus.maleSeat, bus.femaleSeat, bus_state.maleSeat_booked, bus_state.femaleSeat_booked, bus_state.maleSeat_occupied,bus_state.femaleSeat_occupied,bus_state.busLocation from bus join bus_state on bus.number_plate = bus_state.number_plate where bus.number_plate in (select bus.number_plate from bus join services on bus.bus_service = services.bus_service where services.route like '%" + from.ToString() + "%' or services.route like '" + from.ToString() + "%' and services.route like '%" + to.ToString() + "%')";
                SqlCommand com = new SqlCommand(query, connection);
                com.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = com.ExecuteReader();
                var model = new List<showBus>();
                while (reader.Read())
                {
                    var rou = new showBus();
                    rou.busNumber = reader["number_plate"].ToString();
                    rou.busService = reader["bus_service"].ToString();
                    int male = Int32.Parse(reader["maleSeat"].ToString()) - Int32.Parse(reader["maleSeat_booked"].ToString()) - Int32.Parse(reader["maleSeat_occupied"].ToString());
                    int female = Int32.Parse(reader["femaleSeat"].ToString()) - Int32.Parse(reader["femaleSeat_booked"].ToString()) - Int32.Parse(reader["femaleSeat_occupied"].ToString());
                    string l = reader["busLocation"].ToString();
                    string[] lsp = l.Split(",");
                    lat2 = Convert.ToDouble(lsp[0]);
                    lan2 = Convert.ToDouble(lsp[1]);
                    NumberFormatInfo setPrecision = new NumberFormatInfo();
                    rou.fare = getFare(reader["bus_service"].ToString(), from.ToString(), to.ToString()).ToString();
                    setPrecision.NumberDecimalDigits = 2;
                    rou.distance = distance(lat1, lan1, lat2, lan2).ToString("N", setPrecision) + " KM";
                    rou.maleSeat = male.ToString();
                    rou.femaleSeat = female.ToString();

                    model.Add(rou);

                }


                return View(model);
            }
            else
            {
                return RedirectToAction("notAccess", "Admin");
            }
        }
        /*public string currentLatAndLogForStartingIndex(string location)
        {
            
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select route from services where bus_service = '" + busService.ToString() + "'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string route = "";
            while (reader.Read())
            {
                route = reader["route"].ToString();
            }
            connection.Close();
            string[] splitRoute = route.Split(",");
            int cnt = 0;
            foreach (var i in splitRoute)
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
            p = location.ToString() + "," + cnt.ToString();
            InsertLocationAndIndex(p);
            return "";
        }*/
        public string findStartingLocationValue(string loc)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select location from Bus_stop where stop = '" + loc.ToString() + "'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string passLocation = "";
            while(reader.Read())
            {
                passLocation = reader["location"].ToString();
            }
            connection.Close();
            return passLocation;
        }
        public string getGender(string email)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select gender from cusinfo where email = '"+email.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string gender = "";
            while(reader.Read())
            {
                gender = reader["gender"].ToString();
            }
            return gender;

        }
        public IActionResult request(string number, string male, string female, string fare)
        {

            if (HttpContext.Session.GetString("user") != null)
            {
                ViewData["av"] = "Available Male seat : " + male.ToString()+ " Female Seat : "+female.ToString();
                HttpContext.Session.SetString("requestInfo",number.ToString()+","+male.ToString() + "," +female.ToString() + "," +fare.ToString());
                string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                string query = "Select * from review where dri_id in(select dri_id from bus where number_plate = '"+number.ToString()+"')";
                SqlCommand com = new SqlCommand(query, connection);
                com.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = com.ExecuteReader();
                var model = new List<review>();
                while(reader.Read())
                {
                    review rv = new review();
                    rv.cusid = reader["cusId"].ToString();
                    rv.driId = reader["dri_id"].ToString();
                    rv.rev = reader["rev"].ToString();
                    model.Add(rv);
                }


                return View(model);
            }
            else
            {
                return RedirectToAction("notAccess", "Admin");
            }
        }
        
        public IActionResult review(string driverID, string customerId)
        {
            HttpContext.Session.SetString("review",driverID + "," + customerId);
            return View();
        }
        public void inputFromReview(string name)
        {
            string p = HttpContext.Session.GetString("review");
            string[] psplit = p.Split(",");
            string driver = psplit[0];
            string cusId = psplit[1];
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "insert into review values('"+driver.ToString()+"','"+cusId.ToString()+"','"+name.ToString()+"')";
            SqlCommand com = new SqlCommand(query, connection);
            com.ExecuteNonQuery();
            Response.Redirect("PassengerView");
        }
        public string findBookedMaleSeatAndFemaleSeat(string busNumber)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select maleSeat_booked,femaleSeat_booked from bus_state where number_plate = '"+busNumber.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string malebookSeat = "";
            string femalebookSeat = "";
            while (reader.Read())
            {
                malebookSeat = reader["maleSeat_booked"].ToString();
                femalebookSeat = reader["femaleSeat_booked"].ToString();

            }
            connection.Close();
            string totalBookSeat = malebookSeat + "," + femalebookSeat;
            return totalBookSeat;
        }
        public IActionResult InputFromRequest(string maleSeat, string femaleSeat)
        {
            string totalRequest = HttpContext.Session.GetString("requestInfo");
            string[] totalSplit = totalRequest.Split(",");
            string numberPlate = totalSplit[0];
            string male = totalSplit[1];
            string female = totalSplit[2];
            string fare = totalSplit[3];
            
            if(string.IsNullOrEmpty(maleSeat) && string.IsNullOrEmpty(femaleSeat))
            {
                ViewData["requestError"] = "You don't Book any seat";
            }
            else
            {
                string mail = HttpContext.Session.GetString("user");
                string gender = getGender(mail);
                if (gender.Equals("male"))
                {
                    if(string.IsNullOrEmpty(maleSeat))
                    {
                        ViewData["requestError"] = "You must  book a male Seat";
                    }
                    else
                    {
                        string fm = "";
                        if (string.IsNullOrEmpty(femaleSeat))
                        {
                            fm = "0";
                        }
                        else
                        {
                            fm = femaleSeat.ToString();
                        }
                        int m = Int32.Parse(maleSeat.ToString());
                        int f = Int32.Parse(fm);
                        HttpContext.Session.SetString("BookedSeat", m.ToString() + "," + f.ToString());
                        if(m<= Int32.Parse(male) && f <=Int32.Parse(female))
                        {
                            string bookSeat = findBookedMaleSeatAndFemaleSeat(numberPlate);
                            string[] seatSplit = bookSeat.Split(",");
                            insertIntoRecord(fare,numberPlate,m+f);
                            int maleBookSeat = Int32.Parse(seatSplit[0].ToString()) + m;
                            int femaleBookSeat = Int32.Parse(seatSplit[1].ToString()) + f;
                            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                            SqlConnection connection = new SqlConnection(connectionString);
                            connection.Open();
                            string query = "update bus_state set maleSeat_booked = '"+maleBookSeat.ToString()+"',femaleSeat_booked='"+femaleBookSeat.ToString()+"' where number_plate = '"+numberPlate.ToString()+"'";
                            SqlCommand com = new SqlCommand(query, connection);
                            com.ExecuteNonQuery();
                            connection.Close();
                            Response.Redirect("PassengerView");
                        }
                        else
                        {
                            ViewData["requestError"] = "Your limit , male Seat atmost : "+male+" and female seat atmost : " + female ;
                        }
                    }
                }
                else
                {
                    
                    if(String.IsNullOrEmpty(femaleSeat))
                    {
                        femaleSeat = "0";
                    }
                    else if(String.IsNullOrEmpty(maleSeat))
                    {
                        maleSeat = "0";
                    }
                    int m = Int32.Parse(maleSeat.ToString());
                    int f = Int32.Parse(female.ToString());
                    HttpContext.Session.SetString("BookedSeat", m.ToString() + "," + f.ToString());
                    if (m <= Int32.Parse(male) && f <= Int32.Parse(female))
                    {
                        string bookSeat = findBookedMaleSeatAndFemaleSeat(numberPlate);
                        string[] seatSplit = bookSeat.Split(",");
                        insertIntoRecord(fare, numberPlate, m + f);
                        int maleBookSeat = Int32.Parse(seatSplit[0].ToString()) + m;
                        int femaleBookSeat = Int32.Parse(seatSplit[1].ToString()) + f;
                        string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                        SqlConnection connection = new SqlConnection(connectionString);
                        connection.Open();
                        string query = "update bus_state set maleSeat_booked = '" + maleBookSeat.ToString() + "',femaleSeat_booked='" + femaleBookSeat.ToString() + "' where number_plate = '" + numberPlate.ToString() + "'";
                        SqlCommand com = new SqlCommand(query, connection);
                        com.ExecuteNonQuery();
                        connection.Close();
                        Response.Redirect("PassengerView");
                    }
                    else
                    {
                        ViewData["requestError"] = "Your limit , male Seat atmost : " + male + " and female seat atmost : " + female;
                    }

                }

            }
            return View();
        }
        public void insertIntoRecord(string fare,string numberPlate,int total)
        {
            string fromAndto = HttpContext.Session.GetString("fromAndto");
            string[] fSplit = fromAndto.Split(",");
            string from = fSplit[0];
            string to = fSplit[1];
            int cost = Int32.Parse(fare.ToString()) * total;
            string driId = driverId(numberPlate.ToString());
            string passId = getID(HttpContext.Session.GetString("user"));
            DateTime dt = DateTime.Now;
            string date = dt.ToShortDateString().ToString();
            string[] dateSplit = date.Split("/");
            string insertDate = dateSplit[2] + "-" + dateSplit[0] + "-" + dateSplit[1];
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "Insert into record values('"+driId.ToString()+"','"+passId.ToString()+"','"+numberPlate.ToString()+"','"+cost.ToString()+"','"+from+"','"+to+ "','" + insertDate.ToString() + "')";
            SqlCommand com = new SqlCommand(query, connection);
            com.ExecuteNonQuery();
            connection.Close();


        }
        public IActionResult record()
        {
            if (HttpContext.Session.GetString("user") != null)
            {
                string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                string query = "select * from record";
                SqlCommand com = new SqlCommand(query, connection);
                com.CommandType = System.Data.CommandType.Text;
                SqlDataReader reader = com.ExecuteReader();
                var model = new List<Record>();
                while(reader.Read())
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
            else
            {
                return RedirectToAction("notAccess", "Admin");
            }
        }
        public void ticket()
        {
            string s = "";
            while (true)
            {
                s = RandomNumber(3) + RandomString(7);
                string connectionString = configuration.GetConnectionString("DefaultConnectionString");
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();
                string query = "select count(*) from Ticket where code = '"+s.ToString()+"'";
                SqlCommand com = new SqlCommand(query, connection);
                var count = 0;
                count = (int)com.ExecuteScalar();
                connection.Close();
                if (count == 0)
                    break;
            }     
            insertTicketToDB(s.ToString());
            Response.Redirect("showTicket");
        }
        public void insertTicketToDB(string ticketCode)
        {
            string totalRequest = HttpContext.Session.GetString("requestInfo");
            string[] totalSplit = totalRequest.Split(",");
            string numberPlate = totalSplit[0];
            string eml = HttpContext.Session.GetString("user");
            string cusid = getID(eml);
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            
            string query = "insert into ticket values('"+cusid.ToString()+"','"+numberPlate.ToString()+"','"+ticketCode.ToString()+"')";
            SqlCommand com = new SqlCommand(query, connection);
            com.ExecuteNonQuery();
        }
        public IActionResult showTicket()
        {
            string email = HttpContext.Session.GetString("user");
            string id = getID(email);
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select code from ticket where cusid = '" + id.ToString() + "'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string code = "";
            while(reader.Read())
            {
                code = reader["code"].ToString();
            }
            ViewData["ticket"] = code.ToString();
            connection.Close();
            return View();
        }
        public string driverId(string numberPlate)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select dri_id from bus where number_plate='"+numberPlate.ToString()+"'";
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
        public string getID(string email)
        {
            
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select id from cusinfo where email = '"+email.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string id = "";
            while(reader.Read())
            {
                id = reader["id"].ToString();
            }
            return id;
        }
        public double distance(double lat1, double lon1, double lat2, double lon2)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
                dist = dist * 60 * 1.1515;
                dist = dist * 1.609344;
                return (dist);
            }
        }

        public double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        public double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }
        public double getFare(string busService, string locationStart, string LocationEnd)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnectionString");
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            string query = "select route from services where bus_service = '"+busService.ToString()+"'";
            SqlCommand com = new SqlCommand(query, connection);
            com.CommandType = System.Data.CommandType.Text;
            SqlDataReader reader = com.ExecuteReader();
            string rt = "";
            while(reader.Read())
            {
                rt = reader["route"].ToString();
            }
            connection.Close();
            string[] rtSplit = rt.Split(",");
            double fr = 0;
            bool paise = false;
            foreach(var i in rtSplit)
            {
                if(String.Equals( i, locationStart.ToString()))
                {

                    paise = true;
                }
                else if(String.Equals (i , LocationEnd.ToString()))
                {
                    fr++;
                    break;
                }
                else if(paise)
                {
                    fr++;
                }
            }
            return fr*5;
        }
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string RandomNumber(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}