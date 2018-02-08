using System;
using System.IO;
using Parquet.Data;
using SampleLoggingCore;
using Microsoft.Extensions.Configuration;

namespace Sample_Parquet_App
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sample Parquet-log to S3");

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = configBuilder.Build();

            Schema schema = new Schema(
                    new DataField<DateTime>("Timestamp"),
                    new DataField<int>("Priority"),
                    new DataField<string>("Source"),
                    new DataField<string>("Message"),
                    new DataField<string>("Tags")
                );

            SampleContext.Run("parquet", (format, numOfFiles, numOfRecords) =>
            {
                using (var client = new SampleS3Client("parquet", config["BuketName"], config["BuketPath"], Amazon.RegionEndpoint.USEast1))
                {
                    for (int i = 0; i < numOfFiles; i++)
                    {
                        DataSet ds = new DataSet(schema);

                        foreach (LogEntry entry in SampleData.GetBunchOfData(numOfRecords))
                        {
                            ds.Add(new Row(entry.Timestamp, entry.Priority, entry.Source, entry.Message, entry.Tag));
                        }

                        using (MemoryStream buffer = new MemoryStream())
                        {
                            using (var writer = new Parquet.ParquetWriter(buffer))
                            {
                                writer.Write(ds, Parquet.CompressionMethod.None);
                            }

                            client.PutObject(buffer);
                        }
                    }
                }
            });

            Console.ReadKey();
        }
    }
}
