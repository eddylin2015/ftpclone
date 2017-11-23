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
        private static string ftpServerIP = "127.0.0.1";
        private static string ftpUserID = "usr";
        private static string ftpPassword = "pwd";
        private static String remoteDir = @"/";
        private static String localDestnDir = "d:\temp";
        private static String opts = "ls"; //ls //cp //md
        private static int  sub_dir_level = 1; //0 1
        private static string filestartwith = "1617_3_";

        static void Main(string[] args)
        {
            foreach(string s in args)
            {
                if (s.StartsWith("ip=")) ftpServerIP = s.Split('=')[1];
                if (s.StartsWith("u=")) ftpUserID = s.Split('=')[1];
                if (s.StartsWith("p=")) ftpPassword = s.Split('=')[1];
                if (s.StartsWith("r=")) remoteDir = s.Split('=')[1];
                if (s.StartsWith("l=")) localDestnDir = s.Split('=')[1];
                if (s.StartsWith("opts=")) opts = s.Split('=')[1];
                if (s.StartsWith("sdl=")) sub_dir_level = int.Parse( s.Split('=')[1]);
                if (s.StartsWith("filestartwith=")) filestartwith = s.Split('=')[1];
            }
            FileInfo info =new  FileInfo("config.txt");
            if (info.Exists)
            {
                Console.WriteLine("load config");
                string[] lines = System.IO.File.ReadAllLines(@"config.txt");
                foreach (string s in lines)
                {
                    if (s.StartsWith("ip=")) ftpServerIP = s.Split('=')[1];
                    if (s.StartsWith("u=")) ftpUserID = s.Split('=')[1];
                    if (s.StartsWith("p=")) ftpPassword = s.Split('=')[1];
                    if (s.StartsWith("r=")) remoteDir = s.Split('=')[1];
                    if (s.StartsWith("l=")) localDestnDir = s.Split('=')[1];
                    if (s.StartsWith("opts=")) opts = s.Split('=')[1];
                    if (s.StartsWith("sdl=")) sub_dir_level = int.Parse(s.Split('=')[1]);
                    if (s.StartsWith("filestartwith=")) filestartwith = s.Split('=')[1];
                }
            }
            Console.WriteLine("ip={0};\n u={1}\n p={2}\n r={3};remotedir\n l={4};localdir\n sdl={5}\n;ite_sub_dir:1;opts={6};ls md cp\n filestartwith={7}",
                ftpServerIP,ftpUserID,ftpPassword,remoteDir,localDestnDir,opts, sub_dir_level,opts,filestartwith);
            Console.WriteLine("press any key..");
            Console.ReadLine();
            _credential=new NetworkCredential(ftpUserID, ftpPassword);
            IteDir(remoteDir);
            Console.ReadLine();
        }
        
        private static NetworkCredential _credential = null;
        static void IteDir(string pathname)
        {
            if (pathname.EndsWith(@"./")) return; 
            Console.WriteLine(pathname);
            // Get the object used to communicate with the server.
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(@"ftp://"+ ftpServerIP + pathname);
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
                    //Console.WriteLine(temp__);
                    String[] dirfile_List = temp__.Split('\r');
                    int i = 0;
                    int default_split_index = 56;
                    while (i < dirfile_List.Length)
                    {
                        String s = dirfile_List[i].Trim();
                        if (s.StartsWith("total")) { i++; continue; }
                        if (i<3 && s.Length<56 && s.Substring(45).Trim().Equals(".")) default_split_index = 45;
                        if ((i + 1) < dirfile_List.Length)
                        {
                            if (s.StartsWith("drw") || s.StartsWith("-rw"))
                            { }
                            else
                            {
                                i++;
                                s += dirfile_List[i].Trim();
                            }
                        }
                        if (s.Equals("")) break;
                        if (s.StartsWith("drw"))
                        {
                            string fn = s.Substring(default_split_index);
                            dirs.Add(fn);
                        }
                        else if (s.StartsWith("-rw"))
                        {
                            string fn = s.Substring(default_split_index);
                            files.Add(fn);
                        }
                        else
                        {
                            Console.WriteLine("error:{0}",s);
                        }
                        i++;
                    }
                    Console.WriteLine("rem List Complete count{1}, status {0} ",response.StatusDescription,i);
                    reader.Close();
                    response.Close();
                }
                foreach (string s in files)
                {

                    if (!filestartwith.Equals(""))
                    {
                        if (s.Length > filestartwith.Length && s.StartsWith(filestartwith)) Console.WriteLine("rem " + s);
                    }
                    else if (opts.Equals("ls"))
                    {
                        Console.WriteLine("rem " + s);
                    }
                }
                foreach (string s in dirs)
                {
                    string temp = pathname + s+ @"/";
                    if (opts.Equals("md")) Console.WriteLine("md {0}",temp);
                    if (sub_dir_level > 0) {
                        IteDir(temp);
                    }
                    
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

