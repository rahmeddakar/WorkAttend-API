using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace WorkAttend.Model.Models
{
    public partial class Location
    {
             public int Id { get; set; }
             public string LocationName { get; set; }
             public double LatitudeP1 { get; set; }
             public double LongitudeP1 { get; set; }     
             public double LatitudeP2 { get; set; }
             public double LongitudeP2 { get; set; }
             public double LongitudeP3 { get; set; }
             public double LatitudeP3 { get; set; }
             public double LongitudeP4 { get; set; }
             public double LatitudeP4 { get; set; }
             public string LocationCode { get; set; }
    }
}