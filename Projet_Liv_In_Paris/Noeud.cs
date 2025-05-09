using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

public class Noeud<T>
{
    public T Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
   
    public Noeud(T id, double latitude, double longitude)
    {
        Id = id;
        Latitude = latitude;
        Longitude = longitude;
      
    }

  
}