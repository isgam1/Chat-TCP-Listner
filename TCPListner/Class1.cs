using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.IO;

namespace TCPListner
{
    public class ClaseCliente
    {

        public string direccion { get; set; }
        public TcpClient cliente { get; set; }

        private NetworkStream stream;

        public ClaseCliente()
        {
            
        }

        public void enviaMensaje(string mensaje)
        {
            if (stream == null) stream = cliente.GetStream();

            if (cliente.Connected)
            {                
                if (stream.CanWrite)
                {
                    Byte[] TempOutStringData;
                    TempOutStringData = System.Text.Encoding.ASCII.GetBytes(mensaje + "\r\n");
                    stream.Write(TempOutStringData, 0, TempOutStringData.Length);
                }
            }
        }

        public bool algoPorLeer() {
            if (stream == null) stream = cliente.GetStream();
            return stream.DataAvailable;        
        }

        public string lee() {
            if (stream == null) stream = cliente.GetStream();
            if (stream.DataAvailable)
            {
                Byte[] data = new Byte[cliente.ReceiveBufferSize];  
                Int32 bytes = stream.Read(data, 0, data.Length);
                String mensajePendiente =  System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //guardamos el mensaje y limpiamos el buffer..
                stream.Flush();
                return mensajePendiente;
            }
            else {
                return "";
            }

        }

        public NetworkStream obtenerStreaming()
        {
            if (stream == null) stream = cliente.GetStream();
            NetworkStream mensajeImagen = stream;
            stream.Flush();
            return mensajeImagen;
        }
       
    }

}
