/***********************************************************************************
* File:         Program.cs                                                         *
* Contents:     Class Program                                                      *
* Author:       Stanislav "Bav" Koncebovski (stanislav@pikkatech.eu)               *
* Date:         2025-02-16 05:56                                                   *
* Version:      1.0                                                                *
* Copyright:    pikkatech.eu (www.pikkatech.eu)                                    *
***********************************************************************************/

using System.Text.Json;
using System.Text.Json.Nodes;
using Blaeus.Domain.Geospatial;
using Blaeus.Library.Domain;
using Blaeus.Library.Domain.Enumerations;
using Blaeus.Library.Management;
using Factotum.Logging;

namespace Blaues2.Tests
{
	public static class Program
	{
		public static void Main()
		{
			Console.WriteLine("Hello tests");

			var fileName = "Data/lviv.json";

			string jsonString = File.ReadAllText(fileName);

			JsonNode root = JsonNode.Parse(jsonString);

			GeoLocality locality = GeoLocalityFromGeonamesJson(root);
		}

		static GeoLocality GeoLocalityFromGeonamesJson(JsonNode root)
		{
			GeoLocality locality = new GeoLocality();

			locality.Name			= (string)root["asciiName"]!;
			locality.CountryCode	= (string)root["countryCode"]!;

			double latitude			= Double.Parse((string)root["lat"]!);
			double longitude		= Double.Parse((string)root["lng"]!);

			locality.Point			= new GeoPoint(latitude, longitude);

			locality.GeonamesId		= (int)root["geonameId"]!;
			locality.Population		= (int)root["population"]!;

			JsonNode jBox			= root["bbox"]!;
			double east				= (double)jBox["east"]!;
			double south			= (double)jBox["south"]!;
			double north			= (double)jBox["north"]!;
			double west				= (double)jBox["west"]!;

			locality.BoundingBox	= new GeoRectangle(west, north, east, south);

			locality.GeoNamesFeatureClass	= (GeoNamesFeatureClass)Enum.Parse(typeof(GeoNamesFeatureClass), (string)root["fcl"]!);
			locality.GeoNamesFeatureCode	= (GeoNamesFeatureCode)Enum.Parse(typeof(GeoNamesFeatureCode), (string)root["fcode"]!);


			JsonArray alternateNames	= (JsonArray)root["alternateNames"]!;

			foreach (JsonNode item in alternateNames)
			{
				string name = (string)item!["name"]!;
				string language = (string)item!["lang"]!;

				if (language == "link")
				{
					continue;
				}

				JsonNode isHistoric = item["isHistoric"]!;

				if (isHistoric == null)
				{
					if (!String.IsNullOrEmpty(language))
					{
						locality.AlternativeNames[language] = name;
					}
				}
				else
				{
					HistoricName historic = new HistoricName{Name=name, Key=language, Source="www.geonames.org"};
					locality.HistoricNames.Add(historic);
				}
			}

			return locality;
		}
	}
}
