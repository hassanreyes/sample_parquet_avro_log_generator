using System;
using System.Diagnostics;
using Amazon.S3;

namespace SampleLoggingCore
{
    public static class SampleContext
    {
        public static void Run(string format, Action<string, int, int> formatHandler)
        {
            if (CaptureParams(out int numOfFiles, out int numOfRecords))
            {
                try
                {
                    Stopwatch watch = new Stopwatch();

                    watch.Start();

                    formatHandler(format, numOfFiles, numOfRecords);

                    var timespan = watch.Elapsed;

                    Console.WriteLine($"{numOfFiles} files with {numOfRecords * 10} records each took > {timespan}");
                }
                catch (AmazonS3Exception amazonS3Exception)
                {
                    if (amazonS3Exception.ErrorCode != null &&
                        (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                        ||
                        amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                    {
                        Console.WriteLine("Check the provided AWS Credentials.");
                        Console.WriteLine(
                            "For service sign up go to http://aws.amazon.com/s3");
                    }
                    else
                    {
                        Console.WriteLine(
                            "Error occurred. Message:'{0}' when writing an object"
                            , amazonS3Exception.Message);
                    }
                }
            }

            Console.ReadKey();
        }

        public static bool CaptureParams(out int numOfFiles, out int numOfRecords)
        {
            numOfRecords = numOfFiles = -1;

            Console.WriteLine("Enter the number of files to be created:");
            var numOfFilesInput = Console.ReadLine();
            Console.WriteLine("Enter the number of records per file (x10):");
            var numOfRecInput = Console.ReadLine();

            if (!int.TryParse(numOfFilesInput, out numOfFiles))
            {
                Console.WriteLine("Not valid number");
                return false;
            }

            if (!int.TryParse(numOfRecInput, out numOfRecords))
            {
                Console.WriteLine("Not valid number");
                return false;
            }

            return true;
        }
    }
}
