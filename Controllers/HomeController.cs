using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Syncfusion.EJ2.Charts;
using Newtonsoft.Json;
using Expense_Tracker_MVC.Models;
using System.Data.SqlClient;
using System.Data;
using Syncfusion.EJ2.Calendars;

namespace EJ2MVCSampleBrowser.Controllers
{
    public class HomeController : Controller
    {
        readonly String connectionString = @"Data Source=LAPTOP-8L25QF1A\SQLEXPRESS;Initial Catalog=OList_Data;Integrated Security=True;Connection Timeout=200000";

        public ActionResult Index()
        {
            return View();
        }

        
        [HttpPost]
        public ActionResult updateRange(string frmDate, string toDate)
        {
            // values for boxes
            DataForBoxes boxes = GetBoxesData(frmDate, toDate, connectionString);
            string revenue = String.Format("{0:0,0}", boxes.revenue); 
            string shipping = String.Format("{0:0,0}", boxes.shipping);
            string netRev = String.Format("{0:0,0}", boxes.net_rev);
            string avgPrice = String.Format("{0:0,0}", boxes.avg_price);

            
            
            return Json(new { rev = revenue
                            , ship = shipping
                            , net = netRev
                            , avg = avgPrice
                            ,pieDataSource = GetPieState(frmDate, toDate, connectionString)
                            ,lineDS = GetAreaChartData(frmDate, toDate, connectionString)
                            ,barDS = GetBarChartData(frmDate, toDate, connectionString)
                            ,dsTable = GetTableData(frmDate, toDate, connectionString)
            });
        }

        [HttpGet]
        public PartialViewResult Dashboard(string frmDate, string toDate)
        {
            string startDate = "2016-09-15";
            string endDate = "2018-09-04";
            if (frmDate != null)
            {
                startDate = frmDate;
                endDate = toDate;

            }

            //values for the date picker
            //ViewBag.startDate = new DateTime(2017, 05, 31);
            //ViewBag.endDate = new DateTime(2017, 11, 30);
            ViewBag.startDate = DateTime.Parse(startDate);
            ViewBag.endDate = DateTime.Parse(endDate);

            // values for boxes
            DataForBoxes boxes = GetBoxesData(startDate, endDate, connectionString);
            ViewBag.revenue = boxes.revenue;
            //TempData["revenue"] = boxes.revenue;
            ViewBag.shipping = boxes.shipping;
            ViewBag.netRev = boxes.net_rev;
            ViewBag.avgPrice = boxes.avg_price;

            //values for the pie chart 

            
            //List<DataForPie> piechart = GetPieState(startDate, endDate, connectionString);
            //ViewBag.pieDataSource = JsonConvert.SerializeObject(pieChart);
            ViewBag.pieDataSource = GetPieState(startDate, endDate, connectionString);
            ViewBag.lineDS = GetAreaChartData(startDate, endDate, connectionString);
            ViewBag.barDS = GetBarChartData(startDate, endDate, connectionString);
            ViewBag.dsTable = GetTableData(startDate, endDate, connectionString);


            ViewBag.animation = new { enable = false };
            ViewBag.palettes = new string[] { "#61EFCD", "#CDDE1F", "#FEC200", "#CA765A", "#2485FA", "#F57D7D", "#C152D2",
                    "#8854D9", "#3D4EB8", "#00BCD7" };
            ViewBag.legentSettings = new {visible = true};
            ViewBag.chartarea = new ChartBorder {Width = 0};
            ViewBag.content = "<p style='font-family:Roboto;font-size: 16px;font-weight: 400;font-weight: 400;letter-spacing: 0.02em;line-height: 16px;color: #797979 !important;'>Account - Balance</p>";
            ViewBag.border = new ChartBorder { Width = 0.5, Color = "#0470D8" };
            ViewBag.margin = new ChartMargin { Top = 90 };
            ViewBag.accBalancecontent = "<p style='font-family:Roboto;font-size: 16px;font-weight: 400;font-weight: 400;letter-spacing: 0.02em;line-height: 16px;color: #797979 !important;'>Income - Expense</p>";
            ViewBag.accBalanceborder = new ChartBorder { Width = 0.5, Color = "#A16EE5" };
            ViewBag.gridToolbar = new object[] { new { text = "Recent Transactions" } };
            return PartialView();
        }

        [HttpGet]
        public PartialViewResult Expense()
        {
            return PartialView();
        }

        [HttpGet]
        public PartialViewResult About()
        {
            return PartialView();
        }

        [HttpGet]
        public PartialViewResult Dialog()
        {
            return PartialView();
        }


        private DataForBoxes GetBoxesData(string startDate, string endDate, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                String queryString = @"select 
                        round(sum( ISNULL(payment_value,0) ),0) as revenue
	                ,round(sum(ISNULL(freight_value,0)),0) as shipping_expenses
	                , round(sum(ISNULL(payment_value,0)),0) - round(sum(freight_value),0) as net_revenue
	                ,round( avg(ISNULL(price,0)),2) as avg_price
	
                  FROM [OList_Data].[dbo].[order_payments] p 
	                JOIN [OList_Data].[dbo].[order_items] i ON p.order_id = i.order_id
	                JOIN orders o ON o.order_id = p.order_id
	                JOIN [OList_Data].[dbo].[products] r ON r.product_id = i.product_id
                WHERE order_purchase_timestamp between cast(@beg as date) and cast(@end as date)";


                SqlCommand command = new SqlCommand(queryString, connection);

                //add first param
                SqlParameter beg = new SqlParameter();
                beg.ParameterName = "@beg";
                beg.Value = startDate;
                command.Parameters.Add(beg);
                //add 2nd param
                SqlParameter end = new SqlParameter();
                end.ParameterName = "@end";
                end.Value = endDate;
                command.Parameters.Add(end);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                double revenue = 0;
                double shipping = 0;
                double net_rev = 0;
                double avg_price = 0;
                while (reader.Read())
                {
                    if (!(reader.GetValue(0) is DBNull)) {
                        revenue = Convert.ToDouble(reader.GetValue(0));
                        shipping = Convert.ToDouble(reader.GetValue(1));
                        net_rev = Convert.ToDouble(reader.GetValue(2));
                        avg_price = Convert.ToDouble(reader.GetValue(3));
                    }
                }
                //reader.Close();
                //connection.Close();
                return new DataForBoxes(revenue, shipping, net_rev, avg_price); 
            }
        }

        private List<DataForPie> GetPieState(string startDate, string endDate, string connectionString)
        {
            List<DataForPie> piechart = new List<DataForPie>();
            //List<DataForPie> PieChart = new List<Expense_Tracker_MVC.Models.DataForPie>

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                
                    String queryString = @"select ISNULL(customers.customer_state,0) as customer_state, 
                                            ISNULL(count(*),0) as number_state 
                                       FROM customers
                                       inner join orders ON orders.customer_id = customers.customer_id
                                       WHERE order_purchase_timestamp between cast(@beg as date) and cast(@end as date)
                                        GROUP BY ISNULL(customers.customer_state,0)"; 

                SqlCommand command = new SqlCommand(queryString, connection);

                //add first param
                SqlParameter beg = new SqlParameter();
                beg.ParameterName = "@beg";
                beg.Value = startDate;
                command.Parameters.Add(beg);
                //add 2nd param
                SqlParameter end = new SqlParameter();
                end.ParameterName = "@end";
                end.Value = endDate;
                command.Parameters.Add(end);

                DataTable dataTable = new DataTable();
                    SqlDataAdapter dataAdapter = new SqlDataAdapter();
                //dataAdapter.SelectCommand = new SqlCommand(queryString, connection);
                dataAdapter.SelectCommand = command;

                try
                {
                    dataAdapter.Fill(dataTable);
                    int total = 0;
                    foreach (DataRow row in dataTable.Rows)
                    {
                        total = total + (int)row["number_state"];
                    }

                    foreach (DataRow row in dataTable.Rows)
                    {
                        var pieVal = new DataForPie();
                        pieVal.x = row["customer_state"].ToString();
                        pieVal.y = (int)row["number_state"];
                        pieVal.text = row["number_state"].ToString();
                        piechart.Add(pieVal);
                    }
                  //  dataAdapter.Dispose();
                  //  dataTable.Dispose();
                }
                catch(TimeoutException e)
                {
                    Console.WriteLine(e);
                }
                //connection.Close();
                return piechart;
            }
        }

        private List<DataForLine> GetAreaChartData(string startDate, string endDate, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                String queryString = @"SELECT ISNULL(count(o.order_id),0) as n_orders
                                    ,ISNULL(sum(p.payment_value),0) as payments
                                    ,ISNULL(format(o.order_purchase_timestamp, 'yyyy-MM'),0) as months
                                    FROM[OList_Data].[dbo].[orders] o JOIN[OList_Data].[dbo].[order_payments] p
                                        ON o.order_id = p.order_id "
                                    //WHERE order_purchase_timestamp between cast(@beg as date) and cast(@end as date)
                                    + @" WHERE order_purchase_timestamp between cast('" + startDate +  @"' as date) and cast('" + endDate +  @"' as date) " +
                                    @" GROUP BY ISNULL(format(o.order_purchase_timestamp, 'yyyy-MM'),0)
                                    ORDER BY ISNULL(format(o.order_purchase_timestamp, 'yyyy-MM'),0)";

                SqlCommand command = new SqlCommand(queryString, connection);
                /*
                //add first param
                SqlParameter beg = new SqlParameter();
                beg.ParameterName = "@beg";
                beg.Value = startDate;
                command.Parameters.Add(beg);
                //add 2nd param
                SqlParameter end = new SqlParameter();
                end.ParameterName = "@end";
                end.Value = endDate;
                command.Parameters.Add(end);
                */
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                
                List<DataForLine> data = new List<DataForLine>();
                int cnt = 0;
                while (reader.Read())
                {
                    string x = Convert.ToString(reader.GetValue(reader.GetOrdinal("months")));
                    double y = Convert.ToDouble(reader.GetValue(reader.GetOrdinal("payments")));
                    data.Add(new DataForLine(x, y ));
                    cnt++;
                }
                //reader.Close();
                //connection.Close();
                return data;
            }
        }

        private List<DataForBar> GetBarChartData(string startDate, string endDate, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                String queryString = @"SELECT ISNULL(count (o.order_id),0) as n_orders
                                            ,ISNULL(count(distinct customer_id),0) as buyers
                                            , ISNULL(count(distinct seller_id),0) as sellers
                                             ,ISNULL(format(order_purchase_timestamp, 'yyyy-MM'),0) as months
                                      FROM [OList_Data].[dbo].[orders] o JOIN order_items i ON o.order_id = i.order_id
                                      WHERE order_purchase_timestamp between cast(@beg as date) and cast(@end as date)
                                      GROUP BY ISNULL(format(order_purchase_timestamp, 'yyyy-MM'),0)
                                      HAVING count(distinct seller_id) > 10
                                      Order by ISNULL(format(order_purchase_timestamp, 'yyyy-MM'),0)";

                SqlCommand command = new SqlCommand(queryString, connection);

                //add first param
                SqlParameter beg = new SqlParameter();
                beg.ParameterName = "@beg";
                beg.Value = startDate;
                command.Parameters.Add(beg);
                //add 2nd param
                SqlParameter end = new SqlParameter();
                end.ParameterName = "@end";
                end.Value = endDate;
                command.Parameters.Add(end);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                List<DataForBar> data = new List<DataForBar>();
                int cnt = 0;
                while (reader.Read())
                {
                    string months = Convert.ToString(reader.GetValue(reader.GetOrdinal("months")));
                    int  sellers = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("sellers")));
                    int buyers = Convert.ToInt32(reader.GetValue(reader.GetOrdinal("buyers")));
                    data.Add(new DataForBar(months, sellers, buyers));
                    cnt++;
                }
                //reader.Close();
                //connection.Close();
                return data;
            }
        }

        private List<DataForTable> GetTableData(string startDate, string endDate, string connectionString)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                String queryString = @"select distinct
                                        TOP 5 
                                        ISNULL(r.product_category_name,0) as category,
                                        ISNULL(order_purchase_timestamp,0) as order_date
                                        ,ISNULL(CASE 
	                                        when payment_type='credit_card' then 'Credit Card'
	                                        when payment_type='not_defined' then 'Gift Certificate'
	                                        when payment_type='debit_card' then 'Debit Card'
	                                        when payment_type='boleto' then 'PayPal'
	                                        when payment_type='voucher' then 'Voucher'
                                            end, 0) as payment
                                                ,ISNULL(payment_value,0) as payment_value
	                                        , ISNULL(price,0) as price
	                                        ,ISNULL(freight_value,0) as shipping
                                            FROM [OList_Data].[dbo].[order_payments] p 
	                                        JOIN [OList_Data].[dbo].[order_items] i ON p.order_id = i.order_id
	                                        JOIN orders o ON o.order_id = p.order_id
	                                        JOIN [OList_Data].[dbo].[products] r ON r.product_id = i.product_id 
                                            WHERE order_purchase_timestamp between cast('" + startDate + @"' as date) and cast('" + endDate + @"' as date) " 
                                        //" WHERE order_purchase_timestamp between cast(@beg as date) and cast(@end as date)                                            
                                        + @" order by ISNULL(order_purchase_timestamp,0) desc";

                SqlCommand command = new SqlCommand(queryString, connection);

                //add first param
                SqlParameter beg = new SqlParameter();
                beg.ParameterName = "@beg";
                beg.Value = startDate;
                command.Parameters.Add(beg);
                //add 2nd param
                SqlParameter end = new SqlParameter();
                end.ParameterName = "@end";
                end.Value = endDate;
                command.Parameters.Add(end);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                List<DataForTable> data = new List<DataForTable>();
                int cnt = 0;
                while (reader.Read())
                {

                    string category = Convert.ToString(reader.GetValue(reader.GetOrdinal("category")));
                    DateTime orderDate = Convert.ToDateTime(reader.GetValue(reader.GetOrdinal("order_date")));
                    string payment = Convert.ToString(reader.GetValue(reader.GetOrdinal("payment")));
                    string price = Convert.ToString(reader.GetValue(reader.GetOrdinal("price")));
                    string shipping = Convert.ToString(reader.GetValue(reader.GetOrdinal("shipping")));
                    data.Add(new DataForTable(category, orderDate, payment, price, shipping));
                    cnt++;
                }
                //reader.Close();
                //connection.Close();
                return data;
            }
        }

    }
}