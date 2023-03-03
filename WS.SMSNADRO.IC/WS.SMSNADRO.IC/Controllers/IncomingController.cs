using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using WS.SMSCBN.IC.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WS.SMSCBN.IC.Controllers
{
    // Modificar la ruta
    [Route("Hook.asmx/entrante")]
    [ApiController]
    public class IncomingController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public IncomingController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //POST
        [HttpPost]
        public IActionResult Post([FromBody] JsonProveedor body)
        //public string Post()
        {
            //Conexión a BD para guardar log del proceso, modificar datos en appsettings.json
            string BDIP = _configuration.GetValue<string>("BDIP");
            string BDName = _configuration.GetValue<string>("BDName");
            string BDusr = _configuration.GetValue<string>("BDusr");
            string BDpss = _configuration.GetValue<string>("BDpss");
            string CadenaSQL = "Data Source=" + BDIP + ";Initial Catalog=" + BDName + ";User ID=" + BDusr + "; Password=" + BDpss;
            string spLogReciboSMS = _configuration.GetValue<string>("spLogReciboSMS");

            // URL WebHook de la campaña de SMS OCC
            string url = _configuration.GetValue<string>("urlWebHookOCC");
            
            int Timeout = _configuration.GetValue<int>("Timeout");
            
            // Respuesta de OCC al ingresar un SMS
            Rsp2OCCEntrante rspEntrante = new Rsp2OCCEntrante();
            string respuesta = "";

            string lastRemintente = "";
            string lastText = "";

            // String de body

            string vBody = JsonConvert.SerializeObject(body);


            try
            {
                // Se arma el objeto para ingresar el SMS al Webhook de OCC
                EstructuraOCC estructuraOCC = new EstructuraOCC();

                foreach (ResultIn resultIn in body.results) {
                    MensajesOCC messages = new MensajesOCC();
                    MensajeOCC message = new MensajeOCC();
                    messages.address = resultIn.from;
                    lastRemintente = resultIn.from;
                    message.text = resultIn.text;
                    lastText = resultIn.text;
                    messages.message = message;
                    estructuraOCC.messages = new List<MensajesOCC>();
                    estructuraOCC.messages.Add(messages);

                    // Se ejecuta el Webhook de OCC
                    respuesta = wWebhookOCC(url, Timeout, estructuraOCC);

                    // Se guarda em BD el resultado
                    executeSPLog(CadenaSQL, spLogReciboSMS, resultIn.from, resultIn.text, respuesta, vBody, "");
                }

                

                rspEntrante = JsonConvert.DeserializeObject<Rsp2OCCEntrante>(respuesta);
                
                return Ok(rspEntrante);
            }
            catch (Exception ex)
            {
                rspEntrante.reason = ex.Message + ex.StackTrace + ex.InnerException;
                rspEntrante.status = "false";
                // Se guarda em BD el resultado
                executeSPLog(CadenaSQL, spLogReciboSMS, lastRemintente, lastText, "", vBody, ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(rspEntrante);
            }
        }

        private void executeSPLog(string SQLConexion, string SP, string Phone, string Message, string APIResponse,string APIRequest, string Error)
        {
            SqlConnection con = null;
            try
            {
                using (con = new SqlConnection(SQLConexion))
                {
                    using (SqlCommand cmd = new SqlCommand(SP, con))
                    {
                        cmd.CommandTimeout = 20000;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@pTelefono", SqlDbType.VarChar).Value = Phone;
                        cmd.Parameters.Add("@pMensaje", SqlDbType.VarChar).Value = Message;
                        cmd.Parameters.Add("@pRespuestaAPI", SqlDbType.VarChar).Value = APIResponse;
                        cmd.Parameters.Add("@pRequestAPI", SqlDbType.VarChar).Value = APIRequest;
                        cmd.Parameters.Add("@pError", SqlDbType.VarChar).Value = Error;
                        cmd.Parameters.Add("@pTipo", SqlDbType.VarChar).Value = "ENTRANTE";
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

     

        private string wWebhookOCC(string Url, int Timeout, EstructuraOCC bodyMessage)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.ServerCertificateValidationCallback = delegate (object se, System.Security.Cryptography.X509Certificates.X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var client = new RestClient(Url);
            var request = new RestRequest("", Method.Post);
            request.Timeout = Timeout;
            request.AddHeader("Content-Type", "application/json");
            var body = JsonConvert.SerializeObject(bodyMessage);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            RestResponse response = client.Execute(request);
            return response.Content;

        }
    }
}
