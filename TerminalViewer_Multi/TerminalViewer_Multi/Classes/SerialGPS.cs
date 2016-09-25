using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;

namespace TerminalViewer
{
    class SerialGPS
    {
        private SerialPort serialPort;
        private GPSPoint parent;
        
        public SerialGPS(GPSPoint parent, String portname)
        {
            this.parent = parent;

            this.serialPort = new SerialPort();

            this.serialPort.BaudRate = 19200;
            this.serialPort.PortName = portname;
            this.serialPort.Parity = Parity.None;
            this.serialPort.DataBits = 8;
            this.serialPort.StopBits = StopBits.One;
            this.serialPort.Handshake = Handshake.None;

            this.serialPort.ReadTimeout = 2000;

        }

        public bool initGPS()
        {
            try
            {
                this.serialPort.Open();
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public void startReceive()
        {
            while (this.parent.Connected && this.serialPort.IsOpen)
            {
                //Console.WriteLine("Running = " + this.parent.isRunning());
                try
                {
                    String data = this.serialPort.ReadLine();
                    if (data.Contains("$GPGGA"))
                        this.parent.OnPositionReceived(this.convertData(data));
                }
                catch (Exception e)
                {
                    this.serialPort.Close();
                    this.parent.Connected = false;
                    //this.parent.gpsProblem(e.ToString());
                }
                finally
                {
                    Thread.Sleep(200);
                }
            }

            this.parent.Connected = false;
            this.serialPort.Close();
        }

        private double[] convertData(String line)
        {
            
            String strLat = "";
            String strLon = "";
            
            try
            {
                String[] lineSplit = line.Split(',');

                if (String.Compare(lineSplit[0], "$GPGGA") == 0)
                {
                    strLat = lineSplit[2];
                    strLon = lineSplit[4];
                }
                else
                {
                    strLat = lineSplit[3];
                    strLon = lineSplit[5];
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            double lat = convertCoord(strLat);
            double lon = convertCoord(strLon);

            return new double[] { lon, lat };
        }

        private double convertCoord(String strLat)
        {
            double tmp_lat;
            double latitude_int, latitude_dec;

            strLat = strLat.Replace('.', ',');
            Double.TryParse(strLat, out tmp_lat);

            latitude_int = Math.Floor(tmp_lat / 100);
            latitude_dec = (tmp_lat - latitude_int * 100) / 60;

            return latitude_int + latitude_dec;
        }

    }
}
