﻿using System.Net.Mail;
using System.Net;
using System.Text;
using System.Web;

namespace AutoPartsServiceWebApi.Tools
{
    public class SMSC
    {
        // Константы с параметрами отправки
        const string SMSC_LOGIN = "MaCaLLaN";          // логин клиента
        const string SMSC_PASSWORD = "MaCaLLaN111418!!";    // пароль или MD5-хеш пароля в нижнем регистре
        bool SMSC_POST = false;                     // использовать метод POST
        const bool SMSC_HTTPS = false;              // использовать HTTPS протокол
        const string SMSC_CHARSET = "utf-8";        // кодировка сообщения (windows-1251 или koi8-r), по умолчанию используется utf-8
        const bool SMSC_DEBUG = false;              // флаг отладки

        // Константы для отправки SMS по SMTP
        const string SMTP_FROM = "api@smsc.ru";     // e-mail адрес отправителя
        const string SMTP_SERVER = "send.smsc.ru";  // адрес smtp сервера
        const string SMTP_LOGIN = "";               // логин для smtp сервера
        const string SMTP_PASSWORD = "";            // пароль для smtp сервера

        public string[][] D2Res;

        // Метод отправки SMS
        //
        // обязательные параметры:
        //
        // phones - список телефонов через запятую или точку с запятой
        // message - отправляемое сообщение
        //
        // необязательные параметры:
        //
        // translit - переводить или нет в транслит
        // time - необходимое время доставки в виде строки (DDMMYYhhmm, h1-h2, 0ts, +m)
        // id - идентификатор сообщения. Представляет собой 32-битное число в диапазоне от 1 до 2147483647.
        // format - формат сообщения (0 - обычное sms, 1 - flash-sms, 2 - wap-push, 3 - hlr, 4 - bin, 5 - bin-hex, 6 - ping-sms, 7 - mms, 8 - mail, 9 - call)
        // sender - имя отправителя (Sender ID). Для отключения Sender ID по умолчанию необходимо в качестве имени
        // передать пустую строку или точку.
        // query - строка дополнительных параметров, добавляемая в URL-запрос ("valid=01:00&maxsms=3")
        //
        // возвращает массив строк (<id>, <количество sms>, <стоимость>, <баланс>) в случае успешной отправки
        // либо массив строк (<id>, -<код ошибки>) в случае ошибки

        public string[] send_sms(string phones, string message, int translit = 0, string time = "", int id = 0, int format = 0, string sender = "", string query = "", string[] files = null)
        {
            if (files != null)
                SMSC_POST = true;

            string[] formats = { "flash=1", "push=1", "hlr=1", "bin=1", "bin=2", "ping=1", "mms=1", "mail=1", "call=1" };

            string[] m = _smsc_send_cmd("send", "cost=3&phones=" + _urlencode(phones)
                            + "&mes=" + _urlencode(message) + "&id=" + id.ToString() + "&translit=" + translit.ToString()
                            + (format > 0 ? "&" + formats[format - 1] : "") + (sender != "" ? "&sender=" + _urlencode(sender) : "")
                            + (time != "" ? "&time=" + _urlencode(time) : "") + (query != "" ? "&" + query : ""), files);

            // (id, cnt, cost, balance) или (id, -error)

            if (SMSC_DEBUG)
            {
                if (Convert.ToInt32(m[1]) > 0)
                    _print_debug("Сообщение отправлено успешно. ID: " + m[0] + ", всего SMS: " + m[1] + ", стоимость: " + m[2] + ", баланс: " + m[3]);
                else
                    _print_debug("Ошибка №" + m[1].Substring(1, 1) + (m[0] != "0" ? ", ID: " + m[0] : ""));
            }

            return m;
        }

        // SMTP версия метода отправки SMS

        public void send_sms_mail(string phones, string message, int translit = 0, string time = "", int id = 0, int format = 0, string sender = "")
        {
            MailMessage mail = new MailMessage();

            mail.To.Add("send@send.smsc.ru");
            mail.From = new MailAddress(SMTP_FROM, "");

            mail.Body = SMSC_LOGIN + ":" + SMSC_PASSWORD + ":" + id.ToString() + ":" + time + ":"
                        + translit.ToString() + "," + format.ToString() + "," + sender
                        + ":" + phones + ":" + message;

            mail.BodyEncoding = Encoding.GetEncoding(SMSC_CHARSET);
            mail.IsBodyHtml = false;

            SmtpClient client = new SmtpClient(SMTP_SERVER, 25);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = false;
            client.UseDefaultCredentials = false;

            if (SMTP_LOGIN != "")
                client.Credentials = new NetworkCredential(SMTP_LOGIN, SMTP_PASSWORD);

            client.Send(mail);
        }

        // Метод получения стоимости SMS
        //
        // обязательные параметры:
        //
        // phones - список телефонов через запятую или точку с запятой
        // message - отправляемое сообщение 
        //
        // необязательные параметры:
        //
        // translit - переводить или нет в транслит
        // format - формат сообщения (0 - обычное sms, 1 - flash-sms, 2 - wap-push, 3 - hlr, 4 - bin, 5 - bin-hex, 6 - ping-sms, 7 - mms, 8 - mail, 9 - call)
        // sender - имя отправителя (Sender ID)
        // query - строка дополнительных параметров, добавляемая в URL-запрос ("list=79999999999:Ваш пароль: 123\n78888888888:Ваш пароль: 456")
        //
        // возвращает массив (<стоимость>, <количество sms>) либо массив (0, -<код ошибки>) в случае ошибки

        public string[] get_sms_cost(string phones, string message, int translit = 0, int format = 0, string sender = "", string query = "")
        {
            string[] formats = { "flash=1", "push=1", "hlr=1", "bin=1", "bin=2", "ping=1", "mms=1", "mail=1", "call=1" };

            string[] m = _smsc_send_cmd("send", "cost=1&phones=" + _urlencode(phones)
                            + "&mes=" + _urlencode(message) + translit.ToString() + (format > 0 ? "&" + formats[format - 1] : "")
                            + (sender != "" ? "&sender=" + _urlencode(sender) : "") + (query != "" ? "&query" : ""));

            // (cost, cnt) или (0, -error)

            if (SMSC_DEBUG)
            {
                if (Convert.ToInt32(m[1]) > 0)
                    _print_debug("Стоимость рассылки: " + m[0] + ". Всего SMS: " + m[1]);
                else
                    _print_debug("Ошибка №" + m[1].Substring(1, 1));
            }

            return m;
        }

        // Метод проверки статуса отправленного SMS или HLR-запроса
        //
        // id - ID cообщения или список ID через запятую
        // phone - номер телефона или список номеров через запятую
        // all - вернуть все данные отправленного SMS, включая текст сообщения (0,1 или 2)
        //
        // возвращает массив (для множественного запроса возвращается массив с единственным элементом, равным 1. В этом случае статусы сохраняются в
        //					двумерном динамическом массиве класса D2Res):
        //
        // для одиночного SMS-сообщения:
        // (<статус>, <время изменения>, <код ошибки доставки>)
        //
        // для HLR-запроса:
        // (<статус>, <время изменения>, <код ошибки sms>, <код IMSI SIM-карты>, <номер сервис-центра>, <код страны регистрации>, <код оператора>,
        // <название страны регистрации>, <название оператора>, <название роуминговой страны>, <название роумингового оператора>)
        //
        // при all = 1 дополнительно возвращаются элементы в конце массива:
        // (<время отправки>, <номер телефона>, <стоимость>, <sender id>, <название статуса>, <текст сообщения>)
        //
        // при all = 2 дополнительно возвращаются элементы <страна>, <оператор> и <регион>
        //
        // при множественном запросе (данные по статусам сохраняются в двумерном массиве D2Res):
        // если all = 0, то для каждого сообщения или HLR-запроса дополнительно возвращается <ID сообщения> и <номер телефона>
        //
        // если all = 1 или all = 2, то в ответ добавляется <ID сообщения>
        //
        // либо массив (0, -<код ошибки>) в случае ошибки

        public string[] get_status(string id, string phone, int all = 0)
        {
            string[] m = _smsc_send_cmd("status", "phone=" + _urlencode(phone) + "&id=" + _urlencode(id) + "&all=" + all.ToString());

            // (status, time, err, ...) или (0, -error)

            if (id.IndexOf(',') == -1)
            {
                if (SMSC_DEBUG)
                {
                    if (m[1] != "" && Convert.ToInt32(m[1]) >= 0)
                    {
                        int timestamp = Convert.ToInt32(m[1]);
                        DateTime offset = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                        DateTime date = offset.AddSeconds(timestamp);

                        _print_debug("Статус SMS = " + m[0] + (timestamp > 0 ? ", время изменения статуса - " + date.ToLocalTime() : ""));
                    }
                    else
                        _print_debug("Ошибка №" + m[1].Substring(1, 1));
                }

                int idx = all == 1 ? 9 : 12;

                if (all > 0 && m.Length > idx && (m.Length < idx + 5 || m[idx + 5] != "HLR"))
                    m = String.Join(",", m).Split(",".ToCharArray(), idx);
            }
            else
            {
                if (m.Length == 1 && m[0].IndexOf('-') == 2)
                    return m[0].Split(',');

                Array.Resize(ref D2Res, 0);
                Array.Resize(ref D2Res, m.Length);

                for (int i = 0; i < D2Res.Length; i++)
                    D2Res[i] = m[i].Split(',');

                Array.Resize(ref m, 1);
                m[0] = "1";
            }

            return m;
        }

        // Метод получения баланса
        //
        // без параметров
        //
        // возвращает баланс в виде строки или пустую строку в случае ошибки

        public string get_balance()
        {
            string[] m = _smsc_send_cmd("balance", ""); // (balance) или (0, -error)

            if (SMSC_DEBUG)
            {
                if (m.Length == 1)
                    _print_debug("Сумма на счете: " + m[0]);
                else
                    _print_debug("Ошибка №" + m[1].Substring(1, 1));
            }

            return m.Length == 1 ? m[0] : "";
        }

        // ПРИВАТНЫЕ МЕТОДЫ

        // Метод вызова запроса. Формирует URL и делает 3 попытки чтения

        private string[] _smsc_send_cmd(string cmd, string arg, string[] files = null)
        {
            string url, _url;

            arg = "login=" + _urlencode(SMSC_LOGIN) + "&psw=" + _urlencode(SMSC_PASSWORD) + "&fmt=1&charset=" + SMSC_CHARSET + "&" + arg;

            url = _url = (SMSC_HTTPS ? "https" : "http") + "://smsc.ru/sys/" + cmd + ".php" + (SMSC_POST ? "" : "?" + arg);

            string ret;
            int i = 0;
            HttpWebRequest request;
            StreamReader sr;
            HttpWebResponse response;

            do
            {
                if (i++ > 0)
                    url = _url.Replace("smsc.ru/", "www" + i.ToString() + ".smsc.ru/");

                request = (HttpWebRequest)WebRequest.Create(url);

                if (SMSC_POST)
                {
                    request.Method = "POST";

                    string postHeader, boundary = "----------" + DateTime.Now.Ticks.ToString("x");
                    byte[] postHeaderBytes, boundaryBytes = Encoding.ASCII.GetBytes("--" + boundary + "--\r\n"), tbuf;
                    StringBuilder sb = new StringBuilder();
                    int bytesRead;

                    byte[] output = new byte[0];

                    if (files == null)
                    {
                        request.ContentType = "application/x-www-form-urlencoded";
                        output = Encoding.UTF8.GetBytes(arg);
                        request.ContentLength = output.Length;
                    }
                    else
                    {
                        request.ContentType = "multipart/form-data; boundary=" + boundary;

                        string[] par = arg.Split('&');
                        int fl = files.Length;

                        for (int pcnt = 0; pcnt < par.Length + fl; pcnt++)
                        {
                            sb.Clear();

                            sb.Append("--");
                            sb.Append(boundary);
                            sb.Append("\r\n");
                            sb.Append("Content-Disposition: form-data; name=\"");

                            bool pof = pcnt < fl;
                            String[] nv = new String[0];

                            if (pof)
                            {
                                sb.Append("File" + (pcnt + 1));
                                sb.Append("\"; filename=\"");
                                sb.Append(Path.GetFileName(files[pcnt]));
                            }
                            else
                            {
                                nv = par[pcnt - fl].Split('=');
                                sb.Append(nv[0]);
                            }

                            sb.Append("\"");
                            sb.Append("\r\n");
                            sb.Append("Content-Type: ");
                            sb.Append(pof ? "application/octet-stream" : "text/plain; charset=\"" + SMSC_CHARSET + "\"");
                            sb.Append("\r\n");
                            sb.Append("Content-Transfer-Encoding: binary");
                            sb.Append("\r\n");
                            sb.Append("\r\n");

                            postHeader = sb.ToString();
                            postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);

                            output = _concatb(output, postHeaderBytes);

                            if (pof)
                            {
                                FileStream fileStream = new FileStream(files[pcnt], FileMode.Open, FileAccess.Read);

                                // Write out the file contents
                                byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)fileStream.Length))];

                                bytesRead = 0;
                                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                                {
                                    tbuf = buffer;
                                    Array.Resize(ref tbuf, bytesRead);

                                    output = _concatb(output, tbuf);
                                }
                            }
                            else
                            {
                                byte[] vl = Encoding.UTF8.GetBytes(nv[1]);
                                output = _concatb(output, vl);
                            }

                            output = _concatb(output, Encoding.UTF8.GetBytes("\r\n"));
                        }
                        output = _concatb(output, boundaryBytes);

                        request.ContentLength = output.Length;
                    }

                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(output, 0, output.Length);
                }

                try
                {
                    response = (HttpWebResponse)request.GetResponse();

                    sr = new StreamReader(response.GetResponseStream());
                    ret = sr.ReadToEnd();
                }
                catch (WebException)
                {
                    ret = "";
                }
            }
            while (ret == "" && i < 5);

            if (ret == "")
            {
                if (SMSC_DEBUG)
                    _print_debug("Ошибка чтения адреса: " + url);

                ret = ","; // фиктивный ответ
            }

            char delim = ',';

            if (cmd == "status")
            {
                string[] par = arg.Split('&');

                for (i = 0; i < par.Length; i++)
                {
                    string[] lr = par[i].Split("=".ToCharArray(), 2);

                    if (lr[0] == "id" && lr[1].IndexOf("%2c") > 0) // запятая в id - множественный запрос
                        delim = '\n';
                }
            }

            return ret.Split(delim);
        }

        // кодирование параметра в http-запросе
        private string _urlencode(string str)
        {
            if (SMSC_POST) return str;

            return HttpUtility.UrlEncode(str);
        }

        // объединение байтовых массивов
        private byte[] _concatb(byte[] farr, byte[] sarr)
        {
            int opl = farr.Length;

            Array.Resize(ref farr, farr.Length + sarr.Length);
            Array.Copy(sarr, 0, farr, opl, sarr.Length);

            return farr;
        }

        // вывод отладочной информации
        private void _print_debug(string str)
        {
            System.Diagnostics.Debug.WriteLine(str);
        }
    }
}