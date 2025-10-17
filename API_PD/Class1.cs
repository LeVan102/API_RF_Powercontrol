using System;
using System.IO.Ports;
using System.Threading;

namespace API_PD
{
    public enum ControlCmd
    {
        On,        // relay on x
        Off,       // relay off x
        Read,      // relay read x
        ReadAll,   // relay readall
        WriteAll   // relay writeall XX
    }

    public class Relay
    {
        private readonly SerialPort _port;

        // comPort ví dụ: "COM3"; baud mặc định 9600
        public Relay(string comPort, int baudRate = 9600)
        {
            _port = new SerialPort(comPort, baudRate)
            {
                NewLine = "\r",      // module dùng CR kết thúc lệnh
                ReadTimeout = 1500,
                WriteTimeout = 1500,
                DtrEnable = true,    // nếu thiết bị không cần, có thể để false
                RtsEnable = true
            };
        }

        // 1) Connect: mở cổng
        public bool Connect()
        {
            try
            {
                if (!_port.IsOpen) _port.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // 2) Disconnect: đóng cổng
        public void Disconnect()
        {
            if (_port.IsOpen) _port.Close();
        }

        // 3) CheckConnect: kiểm tra trạng thái cổng
        public bool CheckConnect() => _port.IsOpen;

        // 4) GetData: đọc một dòng phản hồi (nếu có)
        //    delayMs: tạm dừng ngắn để thiết bị kịp trả lời
        public string GetData()
        {
            _port.DiscardInBuffer();                                                                    	  //discard input buffer
            if (!_port.IsOpen)                                                                              //condition
                _port.Open();
            {
                try                                                                                               //do or act
                {
                    _port.DiscardInBuffer();                                                                //discard input buffer
                    _port.Write("ver\r");                                                                   //writing "ver" command to serial port
                    System.Threading.Thread.Sleep(10);                                                            //system sleep
                    string response = _port.ReadExisting();                                                 //read response string
                    //verBox.Text = response.Substring(5, 8);                                                       //extracting string from front end
                    _port.DiscardOutBuffer();                                                               //discard output buffer
                    return response.Substring(5, 8);
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }
        public string ReadAll()
        {
            _port.DiscardInBuffer();                                                                   //discard input buffer
            if (!_port.IsOpen)                                                                         //condition
                _port.Open();
            {
                try                                                                                          //do or act
                {
                    _port.DiscardInBuffer();                                                           //discard input buffer
                    _port.Write("relay readall\r");                                                    //writing "relay readall" command to serial port
                    System.Threading.Thread.Sleep(10);                                                       //system sleep
                    string response = _port.ReadExisting();                                            //read response string
                    //relayReadallStstusBox.Text = response.Substring(15, 4);                                  //extracting string from front end
                    _port.DiscardOutBuffer();                                                          //discard output buffer
                    return response.Substring(15, 4);
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }

        public string Read(string readRelayNumber)
        {
            _port.DiscardInBuffer();                                                                   //discard input buffer
            if (!_port.IsOpen)                                                                         //condition
                _port.Open();
                try                                                                                          //do or act
                {
                    _port.DiscardInBuffer();                                                           //discard input buffer
                    _port.Write("relay read " + readRelayNumber + "\r");                                                     //writing "relay readall" command to serial port
                    System.Threading.Thread.Sleep(10);                                                       //system sleep
                    string response = _port.ReadExisting();                                            //read response string
                    //relayReadallStstusBox.Text = response.Substring(15, 4);                                  //extracting string from front end
                    _port.DiscardOutBuffer();                                                          //discard output buffer
                    return response.Substring(15, 4);
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
        }
        // On
        public bool On(string onRelayNumber)
        {
            _port.DiscardInBuffer();                                                                    //discard input buffer
            if (!_port.IsOpen)                                                                          //condition
                _port.Open();
                if (onRelayNumber.Length != 0)                                                        //condition
                {
                    try                                                                                       //do or act
                    {
                        _port.DiscardInBuffer();                                                        //discard input buffer
                        _port.Write("relay on " + onRelayNumber + "\r");                        //writing "relay on X" command to serial port
                        System.Threading.Thread.Sleep(10);                                                    //system sleep
                        _port.DiscardOutBuffer();                                                    //discard output buffer
                        return true;    
                    }
                    catch                                                                                     //exception
                    {
                        return false;
                    }
                }
                else { return false; }
        }
        // OFF
        private bool OFF(string offRelayNumber)
        {
            _port.DiscardInBuffer();                                                                    //discard input buffer
            if (!_port.IsOpen)                                                                          //condition
                _port.Open();
                if (offRelayNumber.Length != 0)                                                        //condition
                {
                    try                                                                                       //do or act
                    {
                        _port.DiscardInBuffer();                                                        //discard input buffer
                        _port.Write("relay off " + offRelayNumber + "\r");                        //writing "relay on X" command to serial port
                        System.Threading.Thread.Sleep(10);                                                    //system sleep
                        _port.DiscardOutBuffer();                                                    //discard output buffer
                        return true;
                    }
                    catch                                                                                     //exception
                    {
                        return false;
                    }
                }
                else { return false; }
        }

        // 5) Reset: đưa tất cả relay về OFF
        public bool Reset()
        {
            if (!_port.IsOpen) return false;
            try
            {
                _port.DiscardInBuffer();                                                           //discard input buffer
                _port.Write("reset\r");                                                            //writing "reset" command to serial port
                System.Threading.Thread.Sleep(10);                                                       //system sleep
                string response = _port.ReadExisting();                                            //read response string
                _port.DiscardOutBuffer();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        // 6) Control: thực thi các lệnh relay theo giao thức
        public string Control(ControlCmd cmd, int? channel = null, int delayMs = 10)
        {
            if (!_port.IsOpen) return "Port not open";
            try
            {
                switch (cmd)
                {
                    case ControlCmd.On:
                        if (channel is null) return "Missing channel";
                        return On($"{channel}").ToString();

                    case ControlCmd.Off:
                        if (channel is null) return "Missing channel";
                        return OFF($"{channel}").ToString();

                    case ControlCmd.Read:
                        if (channel is null) return "Missing channel";
                        return Read($"{channel}"); // VD: "ON" hoặc "OFF"

                    case ControlCmd.ReadAll:
                        return ReadAll(); // VD: "FF", "0F", "00", v.v.

                    case ControlCmd.WriteAll:
                        return Reset().ToString();
                    default:
                        return "Unknown command";
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }



        // ——— thêm vài tiện ích theo bảng lệnh bạn có ———
        /*
        
        public string GetFirmwareVersion()
        {
            if (!_port.IsOpen) return "Port not open";
            On("ver");
            return GetData(); // tuỳ thiết bị: "Firmware Version 1.3", v.v.
        }

        public string IdGet()
        {
            if (!_port.IsOpen) return "Port not open";
            On("id get");
            return GetData();
        }

        public string IdSet(string eightChars)
        {
            if (string.IsNullOrWhiteSpace(eightChars) || eightChars.Length != 8)
                return "ID must be exactly 8 characters";
            if (!_port.IsOpen) return "Port not open";
            On($"id set {eightChars}");
            return "OK";
        }
        */

    }
}