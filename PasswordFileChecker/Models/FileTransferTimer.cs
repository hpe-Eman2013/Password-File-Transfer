using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using PasswordCore.Enums;
using PasswordCore.Interfaces;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace PasswordFileChecker.Models
{
    public class FileTransferTimer : IFileTransferTimer
    {
        private readonly ITextFile _textFile;
        private readonly IDbConnector _connector;
        private readonly IPasswordRepository _password;
        public FileTransferTimer(ITextFile textfile, IDbConnector connector, IPasswordRepository passwordRepository)
        {
            _textFile = textfile;
            _connector = connector;
            _password = passwordRepository;
        }
        public void StartFileChecker()
        {
            var pin = "";
            try
            {
                //check for files
                var filename = CheckforFiles();
                if (!string.IsNullOrEmpty(filename))
                {
                    //get the pin
                    pin = _textFile.GetPinFromUserId(0, filename);
                    //save file to database
                    if (_textFile.AddPasswordListToDb() == 0)
                        throw new Exception("Make sure that your email address "
                        + "is on the first line by itself and your UserId has "
                        + "a value of 0.");

                    //place passwords in output file
                    _textFile.PlacePasswordRecordsInFile(pin, ShiftDirection.ShiftNone);
                    //email the user
                    EmailUser(pin, @"File Successfully Added.");
                }
            }
            catch (Exception ex)
            {
                EmailUser(pin, ex.Message);
            }
        }

        private string CheckforFiles()
        {
            var file = Directory.GetFiles(_textFile.SourceFolder).FirstOrDefault();
            return File.Exists(file) ? file : string.Empty;
        }

        private void EmailUser(string pin, string errorMsg = "")
        {

            var userId = _textFile.GetUserIdFromPin(pin);
            _password.UserId = userId;
            var credentials = _password.GetRecords()
                .Where(x => x.Name.ToLower() == "google" && x.Username.Contains("epym7pfs"))
                .ToDictionary(x => x.Username, x => x.Password)
                .FirstOrDefault();
            var emailTable = _connector.ConfigDictionary.FirstOrDefault(x => x.Key.Contains("EmailAccount")).Value;
            var accTable = _connector.ConfigDictionary.FirstOrDefault(x => x.Key.Contains("UserAccount")).Value;
            var query = $"Select Email From {emailTable} Join {accTable} on {emailTable}.UserId = {accTable}.UserId " +
                        $"Where {emailTable}.UserId = {userId}";
            var cmd = new SqlCommand(query, _connector.GetConnectionObject());
            var email = _connector.ExecuteSqlScalarStatement(cmd).ToString();
            SendEmail(email, credentials, errorMsg);
        }

        private void SendEmail(string emailAddress, KeyValuePair<string, string> recordCredentials, string errorMsg = "")
        {
            SmtpClient client = new SmtpClient();
            MailMessage mail = new MailMessage(recordCredentials.Key, emailAddress);
            client.Port = 587;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Host = "smtp.googlemail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.Credentials = new NetworkCredential(recordCredentials.Key, recordCredentials.Value);
            mail.BodyEncoding = Encoding.UTF8;
            mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
            mail.To.Add(new MailAddress(emailAddress));
            mail.Subject = "File Transfer Status";
            mail.Body = string.IsNullOrEmpty(errorMsg) ? "File successfully added to database." : errorMsg;
            client.Send(mail);
        }
    }
}
