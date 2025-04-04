using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

public class Noeud<T>
{
    public T Nom { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public Noeud(T nom, double latitude, double longitude)
    {
        Nom = nom;
        Latitude = latitude;
        Longitude = longitude;
    }

    public override string ToString()
    {
        return $"{Nom} ({Latitude}, {Longitude})";
    }
}
