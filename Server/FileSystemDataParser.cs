using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

using Catan.Entities;

namespace Server
{
    public class FileSystemDataParser
    {

        public static List<int> ParseRoadImageIndexes(string filepath)
        {
            List<int> indexes = new List<int>();

            try
            {
                Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(filepath);
                StreamReader file = new System.IO.StreamReader(stream);
                string line = file.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    indexes.Add(Int32.Parse(line));                                        
                    line = file.ReadLine();
                }

                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - " + e.Message);
            }
            return indexes;
        }

        public static bool[,] ParseAdjacentData(string filePath)
        {
            bool[,] adjacentMatrix = new bool[100, 100];

            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    adjacentMatrix[i, j] = false;
                }
            }

            try
            {
                Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(filePath);
                StreamReader file = new System.IO.StreamReader(stream);
                string line = file.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    line = line.Replace(" ", string.Empty);
                    line = line.Trim(':');
                    string[] coordinateSet = line.Split(':');
                    string baseCoordinate = string.Empty;
                    for (int i = 0; i < coordinateSet.Length; i++)
                    {                        
                        if (i == 0)
                        {
                            baseCoordinate = coordinateSet[i];
                            continue;
                        }
                        adjacentMatrix[Int32.Parse(baseCoordinate), Int32.Parse(coordinateSet[i])] = true;
                    }

                    line = file.ReadLine();
                }

                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - " + e.Message);
            }

            return adjacentMatrix;
        }

        public static List<DrawCoordinate> ParseCoordinateData(string filePath)
        {
            List<DrawCoordinate> coordinates = new List<DrawCoordinate>();
                        
            try
            {
                Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(filePath);
                StreamReader file = new System.IO.StreamReader(stream);
                string line = file.ReadLine();
                line = line.Replace(" ", string.Empty);
                string[] coordinateSet = line.Split(':');

                foreach (string coordinate in coordinateSet)
                {
                    string[] xy = coordinate.Split(',');

                    DrawCoordinate c = new DrawCoordinate(Int32.Parse(xy[0]), Int32.Parse(xy[1]), 0);
                    coordinates.Add(c);
                }

                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR - " + e.Message);
            }

            return coordinates;             
        }

        public static BoundingCoordinates CreateBoundingCoordinates(string line)
        {
            string pattern = @"\d+";

            MatchCollection matches = Regex.Matches(line, pattern, RegexOptions.IgnoreCase);
            
            int ID = Int32.Parse(matches[0].Value);
            DrawCoordinate dc1 = new DrawCoordinate(Int32.Parse(matches[1].Value), Int32.Parse(matches[2].Value), 0);
            DrawCoordinate dc2 = new DrawCoordinate(Int32.Parse(matches[3].Value), Int32.Parse(matches[4].Value), 0);
            DrawCoordinate dc3 = new DrawCoordinate(Int32.Parse(matches[5].Value), Int32.Parse(matches[6].Value), 0);
            DrawCoordinate dc4 = new DrawCoordinate(Int32.Parse(matches[7].Value), Int32.Parse(matches[8].Value), 0);

            BoundingCoordinates boundingCoordinates = new BoundingCoordinates(ID, dc1, dc2, dc3, dc4);

            return boundingCoordinates;
        }

        public static List<BoundingCoordinates> ParseBoundryData(string filePath)
        {
            string line;
            List<BoundingCoordinates> boundingCoordinates = new List<BoundingCoordinates>();

            try
            {
                System.IO.StreamReader file =
                    new System.IO.StreamReader(filePath);
                while ((line = file.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line.Trim()))
                        break;
                    boundingCoordinates.Add(CreateBoundingCoordinates(line));
                }

                file.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            return boundingCoordinates;
        }
    }
}
