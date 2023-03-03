using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WS.SMSCBN.IC.Model;
using System.Data;
using System.Security.Claims;
using System.Text;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using RestSharp;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WS.SMSCBN.IC.Controllers
{
    // Modificar la ruta
    [Route("Hook.asmx/send")]
    [ApiController]
    public class SendController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SendController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //POST 
        [HttpPost]
        public IActionResult Post([FromBody] JSONOCC body)
        //public string Post()
        {
            //Conexión a BD para guardar log del proceso, modificar datos en appsettings.json
            string BDIP = _configuration.GetValue<string>("BDIP");
            string BDName = _configuration.GetValue<string>("BDName");
            string BDusr = _configuration.GetValue<string>("BDusr");
            string BDpss = _configuration.GetValue<string>("BDpss");
            string CadenaSQL = "Data Source=" + BDIP + ";Initial Catalog=" + BDName + ";User ID=" + BDusr + "; Password=" + BDpss;
            string spLogEnvioSMSID = _configuration.GetValue<string>("spLogEnvioSMSID");

            // Configuración servicio del proveedor 
            string url = _configuration.GetValue<string>("urlSMSProveedor");
            string token = _configuration.GetValue<string>("token");

            // Timeout de consumo del servicio de SMS
            int Timeout = _configuration.GetValue<int>("Timeout");

            string respuesta;
            string Phone = "";
            string RespuestaSend = "";
            string addresses = "";
            string ID = "";
            int count = 0;
            try
            {
                foreach (string tel in body.addresses) {

                    Phone = tel;
                    // Ejecutamos el API de SMS del Proveedor
                    respuesta = wSMSPROVEEDOR(url, Timeout, token, tel, body.message.text);

                    ResponseSMSOutInfobip responseSMSOutInfobip = new ResponseSMSOutInfobip();
                    responseSMSOutInfobip = JsonConvert.DeserializeObject<ResponseSMSOutInfobip>(respuesta);
                    // Guardamos el resultado en BD

                    ID = responseSMSOutInfobip.messages[0].messageId;

                    executeSPLogID(CadenaSQL, spLogEnvioSMSID, tel, body.message.text, respuesta, ID, "");
                    // Se estructura la respuesta para OCC, en este caso el cliente siempre responde enviado. Adecuar según su PROVEEDOR
                    if (count == 0)
                    {
                        addresses = "\"" + Phone + "\": { \"status\": \"SENDING\", \"id\": \"" + ID + "\"}";
                    }
                    else {
                        addresses = addresses + "," + "\"" + Phone + "\": { \"status\": \"SENDING\", \"id\": \"" + ID + "\"}";
                    }
                    count += 1;

                }
                RespuestaSend = "{\"status\": true, \"reason\": \"\", \"addresses\": { " + addresses + "}}"; //"{\"status\": true, \"reason\": \"\", \"addresses\": { \"" + Phone + "\": { \"status\": \"SENDING\", \"id\": \"" + ID + "\"}}}";
                return Ok(RespuestaSend);
            }
            catch (Exception ex)
            {
                // Guardamos el resultado en BD
                executeSPLogID(CadenaSQL, spLogEnvioSMSID, Phone, body.message.text, "", ID, ex.Message + ex.StackTrace + ex.InnerException);
                // Se estructura la respuesta para OCC, de error. 
                RespuestaSend = "{\"status\": false, \"reason\": \"\", \"addresses\": { \"" + Phone + "\": { \"status\": \"FAIL\"}}}";

                return BadRequest(RespuestaSend);
            }
            

        }

        

        private void executeSPLogID(string SQLConexion, string SP, string Phone, string Message, string APIResponse, string ID, string Error)
        {
            SqlConnection con = null;


            try
            {
                SqlDataReader dr;

                using (con = new SqlConnection(SQLConexion))
                {
                    using (SqlCommand cmd = new SqlCommand(SP, con))
                    {
                        cmd.CommandTimeout = 20000;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@pTelefono", SqlDbType.VarChar).Value = Phone;
                        cmd.Parameters.Add("@pMensaje", SqlDbType.VarChar).Value = Message;
                        cmd.Parameters.Add("@pRespuestaAPI", SqlDbType.VarChar).Value = APIResponse;
                        cmd.Parameters.Add("@pMessageID", SqlDbType.VarChar).Value = ID;
                        cmd.Parameters.Add("@pError", SqlDbType.VarChar).Value = Error;
                        cmd.Parameters.Add("@pTipo", SqlDbType.VarChar).Value = "SALIENTE";
                        con.Open();
                        cmd.ExecuteNonQuery();

                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                con.Close();
            }
        }


        private string wSMSPROVEEDOR(string Url, int Timeout, string Token, string Phone, string Message)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.ServerCertificateValidationCallback = delegate (object se, System.Security.Cryptography.X509Certificates.X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            
            List<Destination> destinations = new List<Destination>();
            List<Message> pmessages = new List<Message>();
            Destination destination = new Destination
            {
                to = Phone
            };
            destinations.Add(destination);
            Message message = new Message
            {
                destinations = destinations,
                //message.destinations[0].to = Phone;
                text = Message,
                from = "CXC",
                language = new Language { 
                    languageCode = "ES"
                }
            };
            pmessages.Add(message);
            MessageSalientINFOBIP bodyMessage = new MessageSalientINFOBIP
            {
                messages = pmessages
            };
            var body = JsonConvert.SerializeObject(bodyMessage);


            System.Net.HttpWebRequest webrequest = (HttpWebRequest)System.Net.WebRequest.Create(Url);

            webrequest.Method = "POST";
            webrequest.Timeout = Timeout;
            webrequest.Headers.Add("Authorization", Token);
            webrequest.ContentType = "application/json";
            //webrequest.ContentLength = body.Length;

            using (var streamWriter = new StreamWriter(webrequest.GetRequestStream()))
            {
                streamWriter.Write(body);
                streamWriter.Flush();
                streamWriter.Close();
            }
            WebResponse res;
            string response;
            using (res = webrequest.GetResponse())
            {
                using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                {
                    response = reader.ReadToEnd();
                    string contenido = "response: " + response + " ,StatusCode: " + ((System.Net.HttpWebResponse)res).StatusCode + ", StatusDescription" + ((System.Net.HttpWebResponse)res).StatusDescription;
                }
            }

            return response;


        }

    }
}

