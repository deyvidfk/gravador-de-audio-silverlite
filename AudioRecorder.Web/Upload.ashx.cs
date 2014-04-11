using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace AudioRecorder.Web
{
    /// <summary>
    /// Summary description for Upload
    /// </summary>
    public class Upload : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.InputStream == null || context.Request.InputStream.Length==0)
            {
                throw new ArgumentException("Arquivo nulo");
            }

            using (Stream sr=context.Request.InputStream)
            {
                FileInfo _fileInfo=new FileInfo(context.Request.QueryString["fileName"]);
                string _filename=_fileInfo.Name;
                string _extensao=_fileInfo.Extension;
                string _caminhoNoServidor=String.Concat("UploadWAV", "/", _filename);
                if (String.Compare(_extensao, ".wav", true)!=0)
                {
                    throw new ArgumentException("Extenção de arquivo invalido.");
                }
                // Cria o arquivo na pasta do sistema.
                byte[] buffer=new byte[4096];
                int bytesRead=0;
                using (FileStream fs=File.Create(context.Server.MapPath(_caminhoNoServidor), 4096))
                {
                    while ((bytesRead=sr.Read(buffer, 0, buffer.Length))>0)
                    {
                        // Esceve os dados no arquivo.
                        fs.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}