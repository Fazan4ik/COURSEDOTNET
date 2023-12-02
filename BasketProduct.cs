using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COURSEDOTNET
{
        public class BasketProduct
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double PriceWithOne { get; set; }
            public int NumQuantity { get; set; }
        }

}
