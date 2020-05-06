using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Expense_Tracker_MVC.Models
{
    public class DataForTable
    {
        public string category;
        public DateTime orderDate;
        public string payment;
        public string price;
        public string shipping;

        public DataForTable(string c, DateTime o, string p,string r, string s)
        {
            this.category = c;
            this.orderDate = o;
            this.payment = p;
            this.price = r;
            this.shipping = s;
        }

    }
}