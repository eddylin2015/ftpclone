using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
namespace ftpclone
{
    class Program
    {
        private static string ftpServerIP = "192.168.102.24";
        private static string ftpUserID = "primary";
        private static string ftpPassword = "";
        private static String remoteDir = "";
        private static String localDestnDir = "";

        static void Main(string[] args)
        {
            _credential=new NetworkCredential(ftpUserID, "janeDoe@contoso.com");
            IteDir(@"/");
            Console.ReadLine();
        }
        
        private static NetworkCredential _credential = null;
        static void IteDir(string pathname)
        {
            Console.WriteLine(pathname);
            
            // Get the object used to communicate with the server.
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(@"ftp://"+ ftpServerIP + pathname);
                
                //+ pathname);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = _credential;
                request.Proxy = null;
                request.KeepAlive = false;
                request.UsePassive = false;
                List<string> dirs = new List<string>();
                List<string> files = new List<string>();
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream responseStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responseStream);
                    string temp__ = reader.ReadToEnd();
                    String[] dirfile_List = temp__.Split('\r');
                    int i = 0;
                    while (i < dirfile_List.Length)
                    {
                        String s = dirfile_List[i].Trim();
                        if ((i + 1) < dirfile_List.Length)
                        {
                            if (s.StartsWith("drwxr-xr-x") || s.StartsWith("-rw-r--r--"))
                            { }
                            else
                            {
                                i++;
                                s += dirfile_List[i].Trim();
                            }
                        }

                        if (s.Equals("")) break;
                        if (s.StartsWith("drwxr-xr-x"))
                        {
                            string fn = s.Substring(56);
                            dirs.Add(fn);
                        }
                        else if (s.StartsWith("-rw-r--r--"))
                        {
                            string fn = s.Substring(56);
                            files.Add(fn);
                        }
                        else
                        {
                            Console.WriteLine("error");
                        }
                        i++;
                    }
                     Console.WriteLine("INFO:Directory List Complete count{1}, status {0} ",response.StatusDescription,i);
                    reader.Close();
                    response.Close();
                }
                foreach (string s in files) Console.WriteLine(s);
                foreach (string s in dirs)
                {
                    string temp = pathname + s+ @"/";
                    IteDir(temp);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void Download(string file)
        {
          
            try
            {
                string uri = "ftp://" + ftpServerIP + "/" + remoteDir + "/" + file;
                Uri serverUri = new Uri(uri);
                if (serverUri.Scheme != Uri.UriSchemeFtp)
                {
                    return;
                }
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(serverUri);
                reqFTP.Credentials = _credential;
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Proxy = null;
                reqFTP.UsePassive = false;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream responseStream = response.GetResponseStream();
                FileStream writeStream = new FileStream(localDestnDir + @"\" + file, FileMode.Create);
                int Length = 2048;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);
                while (bytesRead > 0)
                {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                }
                writeStream.Close();
                response.Close();
            }
            catch (WebException wEx)
            {
                Console.WriteLine("{0}{1}",wEx.Message, "Download Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine("{0}{1}", ex.Message, "Download Error");
            }
        }
        public static void ftpupload(string file)
        {
            string uri = "ftp://" + ftpServerIP + "/" + remoteDir + "/" + file;
            Uri serverUri = new Uri(uri);
            if (serverUri.Scheme != Uri.UriSchemeFtp)
            {
                return;
            }
            // Get the object used to communicate with the server.
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            // This example assumes the FTP site uses anonymous logon.
            request.Credentials = _credential;

            // Copy the contents of the file to the request stream.
            StreamReader sourceStream = new StreamReader("testfile.txt");
            byte[] fileContents = Encoding.UTF8.GetBytes(sourceStream.ReadToEnd());
            sourceStream.Close();
            request.ContentLength = fileContents.Length;

            Stream requestStream = request.GetRequestStream();
            requestStream.Write(fileContents, 0, fileContents.Length);
            requestStream.Close();

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
        }
    }
}

