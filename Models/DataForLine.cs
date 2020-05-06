using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Expense_Tracker_MVC.Models
{
    public class DataForLine
    {
        public string x;
        public double y;
        public DataForLine(string x, double y)
        {
            this.x = x;
            this.y = y;
        }

    }
}