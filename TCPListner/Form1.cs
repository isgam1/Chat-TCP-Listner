using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace TCPListner
{
    public partial class Form1 : Form
    {
        int cuenta = 0;
        int archivoPorArchivo = 0;
        private const int BufferSize = 1024;
        TcpListener TCPServer = null;
        IPAddress direccionIP = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1]; //Cambiado -//OJO: resulto con AddressList[1]

        public Form1()
        {
            InitializeComponent();
            listBox1.DisplayMember = "direccion";
            LblMiIP.Text = direccionIP.ToString();
        }

		//Este boton escuchar lo que le haya llegado, aqui son los cambios que debes resalizar al recibir algo
        private void BtnEscuchar_Click(object sender, EventArgs e)
        {
            
            
            int puerto = Convert.ToInt32(TxtPuerto.Text);
            if (TCPServer == null)
            {
                TCPServer = new TcpListener(IPAddress.Any, puerto);
                TCPServer.Start();
                timerClientes.Start();
            }
            else {
                timerClientes.Stop();
                TCPServer.Stop();
                TCPServer = null;
                TCPServer = new TcpListener(IPAddress.Any, puerto);                
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null) {
                ClaseCliente elcliente = (ClaseCliente)listBox1.SelectedItem;
                if (elcliente.cliente.Connected) {
                    elcliente.enviaMensaje(textBox1.Text);
                    //Elimina lo que se encuentra en textBox1
                    textBox1.Text = "";
                }
            }
        }

        private void timerClientes_Tick(object sender, EventArgs e)
        {
            if (TCPServer != null)
            {
                if (TCPServer.Pending()) {
                    TcpClient tcpClient = TCPServer.AcceptTcpClient();
                    cuenta++;
                    string llega = String.Format("{0}", tcpClient.Client.RemoteEndPoint);
                    listBox1.Items.Add(new ClaseCliente{ direccion = "Cliente "+ cuenta.ToString() +" (" + llega +")", cliente = tcpClient });
               }
            }

            for (int i = 0; i < listBox1.Items.Count; i++) {
                ClaseCliente elcliente = (ClaseCliente)listBox1.Items[i];
                if (!elcliente.cliente.Connected) {
                    listBox1.Items.RemoveAt(i);
                    elcliente = null;
                }         
            }
        }

        public String VerSiHayMensajesCliente()
        {

            ClaseCliente elcliente;
            String Mensaje = "";
            for (int i = 0; i < listBox1.Items.Count; i++)
            {
                elcliente = (ClaseCliente)listBox1.Items[i];
                if (elcliente.cliente.Connected)
                {
                    if (elcliente.algoPorLeer())
                    {
                        Mensaje += elcliente.lee();
                    }
                }
            }

            return Mensaje;
        }

        private void timerbuscarMensajeCliente_Tick(object sender, EventArgs e)
        {

            byte[] RecData = new byte[BufferSize];
                int RecBytes;

                try
                {

                    NetworkStream netstream = null;
                    ClaseCliente elcliente;
                    elcliente = (ClaseCliente)listBox1.Items[0];

                    if (elcliente.cliente.Connected && elcliente.algoPorLeer())
                    {
                        pictureBox1.Image = null;
                        netstream = elcliente.obtenerStreaming();

                            string SaveFileName = string.Empty;
                            String myresourcefullPath;
                            myresourcefullPath = System.AppDomain.CurrentDomain.BaseDirectory;
                            string path2 = Path.GetRandomFileName();
                            path2 = path2.Replace(".", "");

                            SaveFileName = myresourcefullPath + path2 + ".bin";
                            int totalrecbytes = 0;
                            FileStream Fs = new FileStream(SaveFileName, FileMode.OpenOrCreate, FileAccess.Write);
                            //muestras haya datos que leer
                            do
                            {
                                //leemos 1024 bytes
                                RecBytes = netstream.Read(RecData, 0, RecData.Length);

                                //escribimos los bytes contenidos en RecData (1204 bytes) en un archivo nuevo
                                //de 1024 en 1024 hasta terminar el archivo
                                Fs.Write(RecData, 0, RecBytes);
                                totalrecbytes += RecBytes;

                            }
                            while (netstream.DataAvailable);
                            //mientras haya datos

                            Fs.Close();


                        //leer el arhicov guardado en el listener y mostrar en el picture box

                        BinaryReader br = new BinaryReader(File.Open(SaveFileName, FileMode.Open, FileAccess.Read));
                        //obtenermos los primeros 2 bytes y despues los siguientes 2
                        UInt16 primeros2Bytes = br.ReadUInt16();
                        UInt16 Siguientes2Bytes = br.ReadUInt16();
                        String ext = "";

                        //todos los archivos son datos binarios
                        //siempre tienen una marca en comun
                        //que los identifica, para saber cuales son se consulta una tabla de numeros magicos
                        Debug.WriteLine(primeros2Bytes);

                        if (primeros2Bytes == 0xd8ff && (Siguientes2Bytes & 0xe0ff) == 0xe0ff)
                        {
                            //una imagen jpg siempre tiene los primeros bytes
                            // con la marca especial 0xd8ff y 0xe0ff
                            //en el disco se leen al revez que en memoria
                            ext = ".jpg";
                        }
                        else {
                            if (primeros2Bytes == 0x5025 && (Siguientes2Bytes & 0x4644) == 0x4644)
                            {
                                ext = ".pdf";
                            }
                            else {

                            String msj = System.Text.Encoding.ASCII.GetString(RecData, 0, RecBytes);
                            if (msj.Trim() != "")
                            {
                                textBox2.AppendText(Environment.NewLine);
                                textBox2.AppendText(msj.Trim());
                            }

                        }
                    }
                    string path = Path.GetRandomFileName();
                    path = path.Replace(".", "");

                    string archivoRecibido = myresourcefullPath + path + ext;
                        br.Close();

                        //guardar archivo con extencion en el listener
                        try {

                            File.Move(SaveFileName, archivoRecibido);

                        }
                        catch (IOException ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }

                        //escribir archivo imagen o icono indicando que es pdf
                        //en el picture box

                        switch (ext)
                        {
                            case ".jpg":
                                System.Drawing.Image img =
                      System.Drawing.Image.FromFile(archivoRecibido, true);
                                pictureBox1.Image = img;
                                break;
                            case ".png":
                                System.Drawing.Image png =
                      System.Drawing.Image.FromFile(archivoRecibido, true);
                                pictureBox1.Image = png;
                                break;
                        case ".pdf":
                            pictureBox1.Image = TCPListner.Properties.Resources.pdf.ToBitmap();
                            break;
                       }

                    }

                }
                catch
                {

                }

        }
    }
}
