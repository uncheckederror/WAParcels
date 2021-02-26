using FluentFTP;
using Microsoft.Extensions.Configuration;

using Flurl.Http;

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using Serilog;

namespace AllParcels
{
    class Program
    {
        // GOAL:
        // Ingest Parcel data from primary sources on a daily basis.
        // Download the data
        // Unzip the files
        // *** Everything above this line is reality ***
        // Load the data into Postgres using PostGIS
        // Use Postgres + PostGIS to create vector tiles from the data
        // Display the vector tiles of parcels in an interactive web map.
        // Maintain this service like OpenStreetMaps.
        static async Task Main()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            var config = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json")
                            .Build();

            Log.Information(AppContext.BaseDirectory);

            // This is the output folder.
            var targetFolderPath = Path.Combine(AppContext.BaseDirectory, "WA");

            Log.Information($"Ingested artifacts will be saved to {targetFolderPath}");

            // Verify that the output directory exists, and create it otherwise.
            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath);
            }

            // Load county specific information from the JSON file.
            var counties = new List<County>();
            var x = config.GetSection("Counties").GetChildren();

            foreach (var item in x)
            {
                var y = item.GetChildren().ToList();
                var name = y.Where(x => x.Key == "Name").Select(x => x.Value).FirstOrDefault();
                var dataSource = y.Where(x => x.Key == "DataSource").Select(x => x.Value).FirstOrDefault();
                var parcelDetails = y.Where(x => x.Key == "ParcelDetails").Select(x => x.Value).FirstOrDefault();
                var parcelViewer = y.Where(x => x.Key == "ParcelViewer").Select(x => x.Value).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(dataSource))
                {
                    // Create an object to represent each county.
                    counties.Add(new County
                    {
                        Name = name,
                        DataSource = dataSource,
                        ParcelDetails = parcelDetails,
                        ParcelViewer = parcelViewer,
                        ResultFilePath = targetFolderPath,
                        RawFilePath = string.Empty,
                        Downloaded = false,
                        Succeeded = false,
                        Zipped = false
                    });
                }
            }

            Log.Information($"Found {counties.Count} counties to retrive parcels from.");

            foreach (var county in counties)
            {
                if (!string.IsNullOrWhiteSpace(county.DataSource))
                {
                    // Download the data.
                    county.Downloaded = await county.TryDownloadFile().ConfigureAwait(false);

                    if (!county.Downloaded)
                    {
                        county.Downloaded = await county.TryDownloadFTPFile().ConfigureAwait(false);
                    }

                    // Wait and then retry failed requests.
                    if (!county.Downloaded)
                    {
                        await Task.Delay(1000);
                        county.Downloaded = await county.TryDownloadFile().ConfigureAwait(false);
                    }

                    if (!county.Downloaded)
                    {
                        await Task.Delay(1000);
                        county.Downloaded = await county.TryDownloadFTPFile().ConfigureAwait(false);
                    }

                    // Unzip the data.
                    if (county.Zipped && county.Downloaded)
                    {
                        var checkUnzip = await county.TryUnzipFile().ConfigureAwait(false);
                        Log.Information($"Unzipped {county.Name}");
                    }
                    else if (county.Downloaded)
                    {
                        if (Directory.Exists(county.RawFilePath))
                        {
                            string[] files = Directory.GetFiles(county.RawFilePath);

                            // Copy the files and overwrite destination files if they already exist.
                            foreach (string s in files)
                            {
                                // Use static Path methods to extract only the file name from the path.
                                var fileName = Path.GetFileName(s);
                                var destFile = Path.Combine(county.ResultFilePath, fileName);
                                File.Copy(s, destFile, true);
                            }
                        }
                        else
                        {
                            county.Succeeded = false;
                            Log.Error("Source path does not exist!");
                        }
                    }

                    // Grab only the files that we need.
                    if (county.Downloaded)
                    {
                        // ESRI shapefiles have mandatory and optional files. 
                        // The mandatory file extensions needed for a shapefile are .shp, .shx and .dbf. 
                        // The optional files are: .prj, .xml, .sbn and .sbx.
                        // .prj files are very helpful as they discribe the projection of the data.
                        // https://desktop.arcgis.com/en/arcmap/10.3/manage-data/shapefiles/shapefile-file-extensions.htm
                        var fileNames = new List<string>();
                        fileNames.AddRange(Directory.GetFiles(Path.GetDirectoryName(county.RawFilePath), "*.shp", SearchOption.AllDirectories));
                        fileNames.AddRange(Directory.GetFiles(Path.GetDirectoryName(county.RawFilePath), "*.shx", SearchOption.AllDirectories));
                        fileNames.AddRange(Directory.GetFiles(Path.GetDirectoryName(county.RawFilePath), "*.dbf", SearchOption.AllDirectories));
                        fileNames.AddRange(Directory.GetFiles(Path.GetDirectoryName(county.RawFilePath), "*.prj", SearchOption.AllDirectories));

                        // Copy the data to the output folder.
                        foreach (var file in fileNames)
                        {
                            FileInfo currentFile = new FileInfo(Path.Combine(county.RawFilePath, file));
                            if (currentFile.Exists)
                            {
                                File.Copy(currentFile.FullName, Path.Combine(county.ResultFilePath, county.Name + currentFile.Extension), true);
                            }
                        }

                        Log.Information($"Copied {fileNames.Count} {county.Name} County files to the Parcels folder.");
                    }
                }
            }

            if (File.Exists(Path.Combine(AppContext.BaseDirectory, "WA.zip")))
            {
                File.Delete(Path.Combine(AppContext.BaseDirectory, "WA.zip"));
            }

            // This step isnow redundant because github actions automatically compress artifacts.
            // Zip up the output files.
            //ZipFile.CreateFromDirectory(targetFolderPath, Path.Combine(AppContext.BaseDirectory, "WA.zip"));

            Log.Information($"Successfully download parcels from {counties.Where(x => x.Succeeded == true).Count()} of {counties.Count} counties attempted.");
            var failed = counties.Where(x => x.Succeeded == false).ToArray();
            if (failed is not null && failed.Any())
            {
                Log.Error($"Failed to download parcels from these counties:");
                var output = string.Empty;
                foreach (var fail in failed)
                {
                    Log.Error($"{fail?.Name} - {fail?.DataSource}");
                }
            }
            Log.Information($"Ingested data can be found at:");
            Log.Information(targetFolderPath);
        }

        public class County
        {
            public string Name { get; set; }
            public string State { get; set; }
            public string DataSource { get; set; }
            public string ParcelViewer { get; set; }
            public string ParcelDetails { get; set; }
            public string RawFilePath { get; set; }
            public string ResultFilePath { get; set; }
            public bool Zipped { get; set; }
            public bool Downloaded { get; set; }
            public bool Succeeded { get; set; }
            public NetworkCredential Credential { get; set; }

            public async Task<bool> TryDownloadFTPDirectory()
            {
                var address = new Uri(DataSource);
                var host = address.Host;
                var scheme = address.Scheme;
                var sourceFolder = address.AbsolutePath;

                // Bail if its not an FTP address.
                if (scheme != Uri.UriSchemeFtp)
                {
                    return false;
                }

                // Create an FTP client
                var client = new FtpClient(host);

                // Handle optional ftp credentials.
                if (!(Credential is null))
                {
                    client = new FtpClient()
                    {
                        Host = host,
                        Credentials = Credential
                    };
                }

                var root = Path.GetDirectoryName(AppContext.BaseDirectory);
                var sink = Path.Combine(root, host, DateTime.Now.ToString("yyyy-MM-dd"));

                // Verify that the sink directory exists, and create it otherwise.
                if (!Directory.Exists(sink))
                {
                    Directory.CreateDirectory(sink);
                }

                await client.ConnectAsync();

                // Verify that the source directory exists, bail if it doesn't.
                var checkExists = await client.DirectoryExistsAsync(sourceFolder);

                if (!checkExists)
                {
                    return false;
                }

                var results = await client.DownloadDirectoryAsync(sink, sourceFolder, FtpFolderSyncMode.Update);

                // Verify that we we successfully captured all of the files, and retry the download if we didn't.
                foreach (var result in results)
                {
                    var check = await client.CompareFileAsync(result.LocalPath, result.RemotePath);

                    if (!(check == FtpCompareResult.Equal || check == FtpCompareResult.ChecksumNotSupported))
                    {
                        await client.DownloadFileAsync(result.LocalPath, result.RemotePath);
                        var recheck = await client.CompareFileAsync(result.LocalPath, result.RemotePath);

                        if (!(recheck == FtpCompareResult.Equal || recheck == FtpCompareResult.ChecksumNotSupported))
                        {
                            Log.Error($"Failed to retrive {result.RemotePath}- {recheck}");
                        }
                        else
                        {
                            Log.Information($"Retrived {result.RemotePath} - {recheck}");
                        }
                    }
                    else
                    {
                        Log.Information($"Retrived {result.RemotePath} - {check}");
                    }
                }

                await client.DisconnectAsync();

                return true;
            }

            public async Task<bool> TryDownloadFTPFile()
            {
                var address = new Uri(DataSource);
                var host = address.Host;
                var scheme = address.Scheme;
                var sourceFile = address.AbsolutePath;
                var fileName = address.Segments.LastOrDefault();

                // Bail if its not an FTP address.
                if (scheme != Uri.UriSchemeFtp)
                {
                    return false;
                }

                // Create an FTP client
                var client = new FtpClient(host);

                // Handle optional ftp credentials.
                if (!(Credential is null))
                {
                    client = new FtpClient()
                    {
                        Host = host,
                        Credentials = Credential
                    };
                }

                var root = Path.GetDirectoryName(AppContext.BaseDirectory);
                var sink = Path.Combine(root, Name.Trim(), DateTime.Now.ToString("yyyy-MM-dd"));

                // Verify that the sink directory exists, and create it otherwise.
                if (!Directory.Exists(sink))
                {
                    Directory.CreateDirectory(sink);
                }

                sink = Path.Combine(sink, fileName);

                await client.ConnectAsync().ConfigureAwait(false);

                // Verify that the source directory exists, bail if it doesn't.
                var checkExists = await client.FileExistsAsync(sourceFile).ConfigureAwait(false);

                if (!checkExists)
                {
                    Downloaded = false;
                    return false;
                }

                var result = await client.DownloadFileAsync(sink, sourceFile).ConfigureAwait(false);

                if (result.IsFailure())
                {
                    Downloaded = false;
                    Succeeded = false;
                    return false;
                }
                else
                {
                    RawFilePath = sink;
                }

                // Verify that we we successfully captured all of the files, and retry the download if we didn't.
                var check = await client.CompareFileAsync(sink, sourceFile);

                if (!(check == FtpCompareResult.Equal || check == FtpCompareResult.ChecksumNotSupported))
                {
                    result = await client.DownloadFileAsync(sink, sourceFile);

                    var recheck = await client.CompareFileAsync(sink, sourceFile);

                    if (!(recheck == FtpCompareResult.Equal || recheck == FtpCompareResult.ChecksumNotSupported))
                    {
                        Log.Error($"Failed to retrive {sourceFile} - {recheck}");
                    }
                    else
                    {
                        Log.Information($"Retrived {sourceFile} - {recheck}");
                        Downloaded = true;
                    }
                }
                else
                {
                    Log.Information($"Retrived {sourceFile} - {check}");
                    Downloaded = true;
                }

                await client.DisconnectAsync();

                var fileExtension = Path.GetExtension(sink);
                if (fileExtension == ".zip")
                {
                    Zipped = true;
                }

                return true;
            }

            public async Task<bool> TryDownloadFile()
            {
                var address = new Uri(DataSource);
                var host = address.Host;
                var scheme = address.Scheme;

                // Bail if its not a file.
                if (!((scheme == Uri.UriSchemeHttp) || (scheme == Uri.UriSchemeHttps)))
                {
                    Downloaded = false;
                    return false;
                }

                var root = Path.GetDirectoryName(AppContext.BaseDirectory);
                var sink = Path.Combine(root, Name.Trim(), DateTime.Now.ToString("yyyy-MM-dd"));

                // Verify that the sink directory exists, and create it otherwise.
                if (!Directory.Exists(sink))
                {
                    Directory.CreateDirectory(sink);
                }

                try
                {
                    RawFilePath = await DataSource.DownloadFileAsync(sink).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to download data from {Name}.");
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace.ToString());
                }

                Log.Information(RawFilePath);
                var fileExtension = Path.GetExtension(RawFilePath);

                if (fileExtension == ".zip" || string.IsNullOrWhiteSpace(fileExtension))
                {
                    Zipped = true;
                    Downloaded = true;
                }
                else
                {
                    Downloaded = true;
                }

                return !string.IsNullOrWhiteSpace(RawFilePath);
            }

            public async Task<bool> TryUnzipFile()
            {
                try
                {
                    await Task.Run(() =>
                        ZipFile.ExtractToDirectory(RawFilePath, Path.GetDirectoryName(RawFilePath), true));

                    Succeeded = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to unzip: {RawFilePath}");
                    Log.Error(ex.Message);
                    Log.Error(ex.InnerException.ToString());

                    Succeeded = false;
                    return false;
                }
            }
        }
    }
}
