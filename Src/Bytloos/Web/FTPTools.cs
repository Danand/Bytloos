using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Bytloos.Web
{
    /// <summary>
    /// Simple FTP client.
    /// </summary>
    public class FTPTools
    {
        private const int BUFFER_SIZE = 2048;
        private const string PROTOCOL = "ftp://";

        private readonly string host;
        private readonly string username;
        private readonly string password;
        private readonly List<Exception> exceptions;

        private Stream stream;
        private FtpWebRequest request;
        private FtpWebResponse response;

        /// <summary>
        /// Creates FTP tools object.
        /// </summary>
        /// <param name="host">Host.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        public FTPTools(string host, string username, string password)
        {
            this.host = host.Trim('/');
            this.host = !this.host.Contains(PROTOCOL) ? PROTOCOL + this.host : this.host;
            this.username = username;
            this.password = password;
            this.exceptions = new List<Exception>();
        }

        /// <summary>
        /// Collection of occured exceptions.
        /// </summary>
        public ReadOnlyCollection<Exception> Exceptions
        {
            get { return exceptions.AsReadOnly(); }
        }

        /// <summary>
        /// Downloads remote file.
        /// </summary>
        /// <param name="remoteFilePath">Path to remote file.</param>
        /// <param name="localFilePath">Path for file to save.</param>
        public void Download(string remoteFilePath, string localFilePath)
        {
            try
            {
                this.request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", host, remoteFilePath));

                this.request.Credentials = new NetworkCredential(username, password);
                this.request.UseBinary = true;
                this.request.UsePassive = true;
                this.request.KeepAlive = true;
                this.request.Method = WebRequestMethods.Ftp.DownloadFile;

                this.response = (FtpWebResponse)this.request.GetResponse();

                this.stream = this.response.GetResponseStream();

                if (this.stream == null)
                    throw new NullReferenceException("Response stream is null.");

                var localFileStream = new FileStream(localFilePath, FileMode.Create);
                var byteBuffer = new byte[BUFFER_SIZE];
                var bytesRead = this.stream.Read(byteBuffer, 0, BUFFER_SIZE);

                try
                {
                    while (bytesRead > 0)
                    {
                        localFileStream.Write(byteBuffer, 0, bytesRead);
                        bytesRead = this.stream.Read(byteBuffer, 0, BUFFER_SIZE);
                    }
                }
                catch (Exception xcptn)
                {
                    this.exceptions.Add(xcptn);
                }

                localFileStream.Close();

                this.stream.Close();
                this.response.Close();
                this.request = null;
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }
        }

        /// <summary>
        /// Uploads local file.
        /// </summary>
        /// <param name="remoteFilePath">Path to remote file.</param>
        /// <param name="localFilePath">Path for file to save.</param>
        public void Upload(string remoteFilePath, string localFilePath)
        {
            try
            {
                this.request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", host, remoteFilePath));

                this.request.Credentials = new NetworkCredential(username, password);
                this.request.UseBinary = true;
                this.request.UsePassive = true;
                this.request.KeepAlive = true;
                this.request.Method = WebRequestMethods.Ftp.UploadFile;

                this.stream = this.request.GetRequestStream();

                var localFileStream = new FileStream(localFilePath, FileMode.Create);
                var byteBuffer = new byte[BUFFER_SIZE];
                var bytesSent = localFileStream.Read(byteBuffer, 0, BUFFER_SIZE);

                try
                {
                    while (bytesSent != 0)
                    {
                        this.stream.Write(byteBuffer, 0, bytesSent);
                        bytesSent = localFileStream.Read(byteBuffer, 0, BUFFER_SIZE);
                    }
                }
                catch (Exception xcptn)
                {
                    this.exceptions.Add(xcptn);
                }

                localFileStream.Close();

                this.stream.Close();
                this.request = null;
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }
        }

        /// <summary>
        /// Deletes remote file.
        /// </summary>
        /// <param name="remoteFilePath">Path to remote file.</param>
        public void Delete(string remoteFilePath)
        {
            try
            {
                this.request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", host, remoteFilePath));

                this.request.Credentials = new NetworkCredential(username, password);
                this.request.UseBinary = true;
                this.request.UsePassive = true;
                this.request.KeepAlive = true;
                this.request.Method = WebRequestMethods.Ftp.DeleteFile;

                this.response = (FtpWebResponse)this.request.GetResponse();

                this.response.Close();
                this.request = null;
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }
        }

        /// <summary>
        /// Renames remote file.
        /// </summary>
        /// <param name="currentFilePath">Path to file.</param>
        /// <param name="newFileName">New name of file.</param>
        public void Rename(string currentFilePath, string newFileName)
        {
            try
            {
                this.request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", host, currentFilePath));

                this.request.Credentials = new NetworkCredential(username, password);
                this.request.UseBinary = true;
                this.request.UsePassive = true;
                this.request.KeepAlive = true;
                this.request.Method = WebRequestMethods.Ftp.Rename;
                this.request.RenameTo = newFileName;

                this.response = (FtpWebResponse)this.request.GetResponse();

                this.response.Close();

                this.request = null;
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }
        }

        /// <summary>
        /// Creates remote directory.
        /// </summary>
        /// <param name="relativePath">Directory path.</param>
        public void CreateDirectory(string relativePath)
        {
            try
            {
                this.request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", host, relativePath));

                this.request.Credentials = new NetworkCredential(username, password);
                this.request.UseBinary = true;
                this.request.UsePassive = true;
                this.request.KeepAlive = true;
                this.request.Method = WebRequestMethods.Ftp.MakeDirectory;

                this.response = (FtpWebResponse)this.request.GetResponse();

                this.response.Close();

                this.request = null;
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }
        }

        /// <summary>
        /// Gets time when file was created.
        /// </summary>
        /// <param name="filePath">Path to remote file.</param>
        /// <returns>Time when file was created.</returns>
        public string GetFileCreatedDateTime(string filePath)
        {
            try
            {
                this.request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", host, filePath));

                this.request.Credentials = new NetworkCredential(username, password);
                this.request.UseBinary = true;
                this.request.UsePassive = true;
                this.request.KeepAlive = true;
                this.request.Method = WebRequestMethods.Ftp.GetDateTimestamp;

                this.response = (FtpWebResponse)this.request.GetResponse();

                this.stream = this.response.GetResponseStream();

                if (this.stream == null)
                    throw new NullReferenceException("Response stream is null.");

                var ftpReader = new StreamReader(this.stream);

                string fileInfo = null;

                try
                {
                    fileInfo = ftpReader.ReadToEnd();
                }
                catch (Exception xcptn)
                {
                    this.exceptions.Add(xcptn);
                }

                ftpReader.Close();

                this.stream.Close();
                this.response.Close();
                this.request = null;

                return fileInfo;
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }

            return null;
        }

        /// <summary>
        /// Gets file size.
        /// </summary>
        /// <param name="filePath">Path to remote file.</param>
        /// <returns>File size.</returns>
        public string GetFileSize(string filePath)
        {
            try
            {
                this.request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", host, filePath));

                this.request.Credentials = new NetworkCredential(username, password);
                this.request.UseBinary = true;
                this.request.UsePassive = true;
                this.request.KeepAlive = true;
                this.request.Method = WebRequestMethods.Ftp.GetFileSize;

                this.response = (FtpWebResponse)this.request.GetResponse();

                this.stream = this.response.GetResponseStream();

                if (this.stream == null)
                    throw new NullReferenceException("Response stream is null.");

                var ftpReader = new StreamReader(this.stream);
                string fileInfo = null;

                try
                {
                    while (ftpReader.Peek() != -1)
                        fileInfo = ftpReader.ReadToEnd();
                }
                catch (Exception xcptn)
                {
                    this.exceptions.Add(xcptn);
                }

                ftpReader.Close();

                this.stream.Close();
                this.response.Close();
                this.request = null;

                return fileInfo;
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }

            return null;
        }

        /// <summary>
        /// Lists subdirectories.
        /// </summary>
        /// <param name="directory">Path to directory.</param>
        /// <param name="recursive">Recursive search.</param>
        /// <param name="inclusivePattern">Get only matched elements.</param>
        /// <param name="exclusivePattern">Get only mismatched elements.</param>
        /// <param name="strongMatch">Strong match.</param>
        /// <returns>List of subdirectories.</returns>
        public string[] ListDirectory(string directory, bool recursive = false, string inclusivePattern = null, string exclusivePattern = null, bool strongMatch = false)
        {
            var list = ListContents(directory, recursive, inclusivePattern, exclusivePattern, strongMatch);

            return list.Where(cl => !Regex.IsMatch(cl, @".+\.[^\,]+$")).ToArray();
        }

        /// <summary>
        /// Lists files of remote directory.
        /// </summary>
        /// <param name="directory">Path to directory.</param>
        /// <param name="recursive">Recursive search.</param>
        /// <param name="inclusivePattern">Get only matched elements.</param>
        /// <param name="exclusivePattern">Get only mismatched elements.</param>
        /// <param name="strongMatch">Strong match.</param>
        /// <returns>List of files.</returns>
        public string[] ListFiles(string directory, bool recursive = false, string inclusivePattern = null, string exclusivePattern = null, bool strongMatch = false)
        {
            var list = ListContents(directory, recursive, inclusivePattern, exclusivePattern, strongMatch);

            return list.Where(cl => Regex.IsMatch(cl, @".+\.[^\,]+$")).ToArray();
        }

        /// <summary>
        /// Lists content of remote directory.
        /// </summary>
        /// <param name="directory">Path to directory.</param>
        /// <param name="recursive">Recursive search.</param>
        /// <param name="inclusivePattern">Get only matched elements.</param>
        /// <param name="exclusivePattern">Get only mismatched elements.</param>
        /// <param name="strongMatch">Strong match.</param>
        /// <returns>Content of remote directory.</returns>
        public string[] ListContents(string directory, bool recursive = false, string inclusivePattern = null, string exclusivePattern = null, bool strongMatch = false)
        {
            var directoryParts = directory.Split('/');

            var upperLevels = string.Join("/", directoryParts.Take(directoryParts.Length > 1 ? directoryParts.Length - 1 : 1));

            var list = new List<string>();

            try
            {
                this.request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", host, directory));

                this.request.Credentials = new NetworkCredential(username, password);

                this.request.UseBinary = true;
                this.request.UsePassive = true;
                this.request.KeepAlive = true;
                this.request.Method = WebRequestMethods.Ftp.ListDirectory;

                this.response = (FtpWebResponse)this.request.GetResponse();

                this.stream = this.response.GetResponseStream();

                if (this.stream == null)
                    throw new NullReferenceException("Response stream is null.");

                var ftpReader = new StreamReader(this.stream);

                var directoryRaw = string.Empty;

                try
                {
                    while (ftpReader.Peek() != -1)
                        directoryRaw += ftpReader.ReadLine() + "|";
                }
                catch (Exception xcptn)
                {
                    this.exceptions.Add(xcptn);
                }

                ftpReader.Close();

                this.stream.Close();
                this.response.Close();
                this.request = null;

                try
                {
                    var contentLines =
                        directoryRaw.Split("|".ToCharArray()).Select(e => upperLevels + "/" + e).ToArray();

                    contentLines = contentLines.Take(contentLines.Length > 1 ? contentLines.Length - 1 : 1).ToArray();

                    if (inclusivePattern != null && strongMatch)
                        contentLines = contentLines.Where(cl => Regex.IsMatch(cl, inclusivePattern)).ToArray();

                    if (exclusivePattern != null)
                        contentLines = contentLines.Where(cl => !Regex.IsMatch(cl, exclusivePattern)).ToArray();

                    if (contentLines.Any())
                        strongMatch = true;

                    list.AddRange(contentLines);

                    if (recursive)
                    {
                        foreach (var contentLine in contentLines)
                        {
                            if (Regex.IsMatch(contentLine, @".+\.[^\,]+$"))
                                continue;

                            var innerContentLines = ListContents(contentLine, true, inclusivePattern, exclusivePattern,
                                strongMatch);

                            if (innerContentLines != null)
                                list.AddRange(innerContentLines);
                        }
                    }

                    if (inclusivePattern != null)
                        list = list.Where(cl => Regex.IsMatch(cl, inclusivePattern)).ToList();

                    return list.ToArray();
                }
                catch (Exception xcptn)
                {
                    this.exceptions.Add(xcptn);
                }
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }

            return null;
        }

        /// <summary>
        /// Gets info for each item in directory.
        /// </summary>
        /// <param name="directoryPath">Path to remote directory.</param>
        /// <returns>Listed items with info.</returns>
        public string[] DetailedList(string directoryPath)
        {
            try
            {
                this.request = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", host, directoryPath));

                this.request.Credentials = new NetworkCredential(username, password);
                this.request.UseBinary = true;
                this.request.UsePassive = true;
                this.request.KeepAlive = true;
                this.request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

                this.response = (FtpWebResponse)this.request.GetResponse();

                this.stream = this.response.GetResponseStream();

                if (this.stream == null)
                    throw new NullReferenceException("Response stream is null.");

                var ftpReader = new StreamReader(this.stream);
                var directoryRaw = string.Empty;

                try
                {
                    while (ftpReader.Peek() != -1)
                        directoryRaw += ftpReader.ReadLine() + "|";
                }
                catch (Exception xcptn)
                {
                    this.exceptions.Add(xcptn);
                }

                ftpReader.Close();

                this.stream.Close();
                this.response.Close();
                this.request = null;

                try
                {
                    var directoryList = directoryRaw.Split("|".ToCharArray());
                    return directoryList;
                }
                catch (Exception xcptn)
                {
                    this.exceptions.Add(xcptn);
                }
            }
            catch (Exception exception)
            {
                this.exceptions.Add(exception);
            }

            return null;
        }
    }
}
