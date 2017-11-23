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
        static void Main(string[] args)
        {
            IteDir(@"/");
            Console.ReadLine();
        }
        private static string host = "ftp://192.168.102.24";
        private static string usr = "primary";
        private static string pwd = "";
        static void IteDir(string pathname)
        {
            Console.WriteLine(pathname);
            // Get the object used to communicate with the server.
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(host+pathname);
                
                //+ pathname);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                // This example assumes the FTP site uses anonymous logon.
                request.Credentials = new NetworkCredential(usr, pwd);
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
    }
}
