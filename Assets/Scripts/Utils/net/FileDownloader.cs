using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class FileDownloadProgressChangedEventArgs : EventArgs
{
    public FileDownloadProgressChangedEventArgs(object userState, long? totalBytesToReceive)
    {
        this.UserState = UserState;       
        this.TotalBytesToReceive = totalBytesToReceive;
    }

    public object UserState { get; private set; }

    public float Progress { get; internal set; }

    public long BytesReceived { get; internal set; }

    public long? TotalBytesToReceive { get; private set; }

    public bool Flushed { get; internal set; }
}

public class FileDownloadStartedEventArgs : EventArgs
{
    public FileDownloadStartedEventArgs(bool resumed, long? totalBytes)
    {
        this.Resumed = resumed;
        this.TotalBytesToReceive = totalBytes;
    }

    public bool Resumed { get; private set; }

    public long? TotalBytesToReceive { get; private set; }

}

public delegate void FileDownloadProgressChangedEventHandler(object sender, FileDownloadProgressChangedEventArgs e);
public delegate void FileDownloadStartedEventHandler(object sender, FileDownloadStartedEventArgs e);

public class FileDownloader
{
    public event FileDownloadStartedEventHandler DownloadStarted;

    /// <summary>
    /// NOTE: currently the callback is synchronous with respect to downloading, so do not block too long
    /// </summary>
    public event FileDownloadProgressChangedEventHandler DownloadProgressChanged;
    public event AsyncCompletedEventHandler DownloadFileCompleted;

    private Uri m_url;
    private string m_downloadPath;
    private int m_offset;
    private int m_flushSize;
    private object m_userState;

    private HttpWebRequest m_webRequest;
    // Mono does not throw WebException if Abort is called while reading,
    // we have to manually check for abortion
    private bool m_aborted;

    /// <summary>
    /// download the file from url from the given offset
    /// </summary>
    /// <param name="url"></param>
    /// <param name="downloadPath"></param>
    /// <param name="offset">position where to resume download, only a hint</param>
    /// <param name="flushSize">when to flush the file buffer, &le; 0 for no flushing</param>
    public void Start(string url, string downloadPath, int offset = 0, int flushSize = 0, object userState = null)
    {
        if (m_offset < 0)
        {
            throw new ArgumentOutOfRangeException("offset must >= 0");
        }

        if (m_webRequest != null)
        {
            throw new InvalidOperationException();
        }

        m_url = new Uri(url);
        m_downloadPath = downloadPath;
        m_offset = offset;
        m_flushSize = flushSize;
        m_userState = userState;
        m_aborted = false;

        m_webRequest = (HttpWebRequest)WebRequest.Create(m_url);
        new Thread(Download).Start(m_webRequest);
    }

    public void Cancel()
    {
        var request = m_webRequest;
        if (request != null)
        {
            Debug.Log("cancel");

            request.Abort();
            m_aborted = true;
        }
    }

    void Download(object state)
    {
        var request = (HttpWebRequest)state;
        if (m_offset > 0)
        {
            request.AddRange(m_offset);
        }

        try
        {
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                ContentRangeHeader contentRange = null;
                var contentRangeValue = response.Headers[HttpResponseHeader.ContentRange];
                if (contentRangeValue != null)
                {
                    contentRange = ContentRangeHeader.Parse(contentRangeValue);
                }
                else if (m_offset > 0)
                {
                    Debug.Log("server does not support range request");
                }

                long? totalBytes = contentRange != null ? contentRange.Length : long.Parse(response.Headers[HttpRequestHeader.ContentLength]);
                long receivedBytes = contentRange != null ? m_offset : 0;
                bool resumed = contentRange != null && m_offset > 0;

                using (var responseStream = response.GetResponseStream())
                using (var fileStream = File.Open(m_downloadPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    if (resumed)
                    {
                        fileStream.Seek(m_offset, SeekOrigin.Begin);
                        Debug.Log("resume downloading from: " + m_offset);
                    }
                    else
                    {
                        Debug.Log("start new downloading");
                    }
                    fileStream.SetLength(receivedBytes);

                    try
                    {
                        if (DownloadStarted != null)
                        {
                            DownloadStarted(this, new FileDownloadStartedEventArgs(resumed, totalBytes));
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }

                    var eventArgs = new FileDownloadProgressChangedEventArgs(m_userState, totalBytes);

                    var buf = new byte[32 * 1024];
                    int readBytes;
                    int bytesCounter = 0;
                    bool flushed = false;

                    while (!m_aborted && (readBytes = responseStream.Read(buf, 0, buf.Length)) > 0)
                    {
                        fileStream.Write(buf, 0, readBytes);
                        receivedBytes += readBytes;
                        bytesCounter += readBytes;

                        if (m_flushSize > 0 && bytesCounter >= m_flushSize)
                        {
                            bytesCounter -= m_flushSize;

                            fileStream.Flush();
                            flushed = true;
                        }

                        try
                        {
                            if (DownloadProgressChanged != null)
                            {
                                if (eventArgs.TotalBytesToReceive != null)
                                {
                                    eventArgs.Progress = (float)receivedBytes / eventArgs.TotalBytesToReceive.Value;
                                }
                                eventArgs.BytesReceived = receivedBytes;
                                eventArgs.Flushed = flushed;
                                flushed = false;
                                DownloadProgressChanged(this, eventArgs);
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                    }
                }

                var cancelled = totalBytes != null ? receivedBytes < totalBytes : m_aborted;
                DownloadCompleted(new AsyncCompletedEventArgs(null, cancelled, m_userState));
            }
        }
        catch (ObjectDisposedException)
        {
            Debug.Log("aborted");

            DownloadCompleted(new AsyncCompletedEventArgs(null, true, m_userState));
        }
        catch (WebException e)
        {
            Debug.LogException(e);
            Debug.Log(e.Status);

            DownloadCompleted(new AsyncCompletedEventArgs(e, e.Status == WebExceptionStatus.RequestCanceled, m_userState));
        }
        catch (Exception e)
        {
            Debug.LogException(e);

            DownloadCompleted(new AsyncCompletedEventArgs(e, false, m_userState));
        }
    }

    void DownloadCompleted(AsyncCompletedEventArgs e)
    {
        m_webRequest = null;

        try
        {
            if (DownloadFileCompleted != null)
            {
                DownloadFileCompleted(this, e);
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}
