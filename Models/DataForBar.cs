using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Expense_Tracker_MVC.Models
{
    public class DataForBar
    {
        public string months;
        public int sellers;
        public int buyers;
        public DataForBar(string months, int sellers, int buyers)
        {
            this.months = months;
            this.sellers = sellers;
            this.buyers = buyers;
        }

    }
}