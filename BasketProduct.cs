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
       public string ProductId { get; set; }

        public string Name { get; set; }

      //  public string Category { get; set; }

        public float PriceWithOne { get; set; }

        public int NumQuantity { get; set; }

    //    public string Image { get; set; }
    }
}
