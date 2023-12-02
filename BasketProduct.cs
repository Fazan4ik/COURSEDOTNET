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

            public static BasketProduct FromDataReader(SqlDataReader reader)
            {
                return new BasketProduct
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    PriceWithOne = reader.GetDouble(reader.GetOrdinal("PriceWithOne")),
                    NumQuantity = reader.GetInt32(reader.GetOrdinal("NumQuantity"))
                };
            }
        }

}
