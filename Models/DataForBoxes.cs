using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Expense_Tracker_MVC.Models
{
    public class DataForBoxes
    {
        public double revenue;
        public double shipping;
        public double net_rev;
        public double avg_price;
        

        public DataForBoxes(double r, double s, double n, double p)
        {
            this.revenue = r;
            this.shipping = s;
            this.net_rev = n;
            this.avg_price = p;
        }

    }
}