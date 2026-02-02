using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MedML.Models;

namespace MedML.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.Migrate();

            if (context.HeartDiseaseRecords.Any())
            {
                return;   // DB has been seeded
            }

            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "heart.csv");
            if (!File.Exists(csvPath))
            {
                // Fallback to project directory if running from bin
                // This is a hack for development environment
                csvPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Data\heart.csv"));
            }

            if (!File.Exists(csvPath))
            {
                return;
            }

            var lines = File.ReadAllLines(csvPath);
            // Skip header
            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(',');
                if (values.Length != 12) continue;

                var record = new HeartDiseaseRecord
                {
                    Age = int.Parse(values[0]),
                    Sex = values[1],
                    ChestPainType = values[2],
                    RestingBP = int.Parse(values[3]),
                    Cholesterol = int.Parse(values[4]),
                    FastingBS = int.Parse(values[5]),
                    RestingECG = values[6],
                    MaxHR = int.Parse(values[7]),
                    ExerciseAngina = values[8],
                    Oldpeak = float.Parse(values[9], System.Globalization.CultureInfo.InvariantCulture),
                    ST_Slope = values[10],
                    HeartDisease = int.Parse(values[11])
                };

                context.HeartDiseaseRecords.Add(record);
            }

            context.SaveChanges();
        }
    }
}
