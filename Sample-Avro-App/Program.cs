using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Hadoop.Avro.Container;
using SampleLoggingCore;

namespace Sample_Avro_App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sample Avro-log to S3");

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = configBuilder.Build();

            SampleContext.Run("avro", (format, numOfFiles, numOfRecords) => 
            {
                using (var client = new SampleS3Client("avro", config["BuketName"], config["BuketPath"], Amazon.RegionEndpoint.USEast1))
                {
                    for (int i = 0; i < numOfFiles; i++)
                    {
                        var testData = new List<LogEntry>();

                        testData.AddRange(SampleData.GetBunchOfData(numOfRecords));

                        using (MemoryStream buffer = new MemoryStream())
                        {
                            using (var w = AvroContainer.CreateWriter<LogEntry>(buffer, Codec.Deflate))
                            {
                                using (var writer = new SequentialWriter<LogEntry>(w, 24))
                                {
                                    testData.ForEach(writer.Write);
                                }
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
