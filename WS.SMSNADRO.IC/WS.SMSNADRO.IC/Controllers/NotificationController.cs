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
    [Route("Hook.asmx/notification")]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        private readonly IConfiguration _configuration;

        public NotificationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //POST
        [HttpPost]
        public IActionResult Post([FromBody] JsonNotiProveedor body)
        //public string Post()
        {
            //Conexión a BD para guardar log del proceso, modificar datos en appsettings.json
            string BDIP = _configuration.GetValue<string>("BDIP");
            string BDName = _configuration.GetValue<string>("BDName");
            string BDusr = _configuration.GetValue<string>("BDusr");
            string BDpss = _configuration.GetValue<string>("BDpss");
            string CadenaSQL = "Data Source=" + BDIP + ";Initial Catalog=" + BDName + ";User ID=" + BDusr + "; Password=" + BDpss;
            string spLogEnvioSMSNotifiID = _configuration.GetValue<string>("spLogEnvioSMSNotifiID");

            // URL WebHook de la campaña de SMS OCC
            string url = _configuration.GetValue<string>("urlWebHookNotiOCC");
            
            int Timeout = _configuration.GetValue<int>("Timeout");

            // Respuesta de OCC al ingresar un SMS
            Rsp2OCCEntranteNoti rspNotificacion = new Rsp2OCCEntranteNoti();
            string respuesta = "";

            // String de body

            string vBody = JsonConvert.SerializeObject(body);

            try
            {
                // Se arma el objeto para ingresar el SMS al Webhook de OCC
                EstructuraOCCNoti estructuraOCCNoti = new EstructuraOCCNoti();

                foreach (Result result in body.results) {
                    string descriptionStatus = "";

                    MessageNoti message = new MessageNoti();
                    message.id = result.messageId;
                    descriptionStatus = result.status.name;

                    switch (descriptionStatus) {
                        case "PENDING_WAITING_DELIVERY": case "PENDING_ENROUTE": case "PENDING_ACCEPTED":
                            message.status = "SENDING"; break;
                        case "DELIVERED_TO_OPERATOR": case "DELIVERED_TO_HANDSET":
                            message.status = "SENT"; break;
                        default:
                            message.status = "FAIL"; break;
                    }
                    estructuraOCCNoti.messages = new List<MessageNoti>();
                    estructuraOCCNoti.messages.Add(message);

                }
                

                // Se ejecuta el Webhook de OCC
                respuesta = wWebhookOCC(url, Timeout, estructuraOCCNoti);
                
                // Se guarda em BD el resultado
                executeSPLog(CadenaSQL, spLogEnvioSMSNotifiID, respuesta, vBody, "");

                rspNotificacion = JsonConvert.DeserializeObject<Rsp2OCCEntranteNoti>(respuesta);
                
                return Ok(rspNotificacion);
            }
            catch (Exception ex)
            {
                rspNotificacion.reason = ex.Message + ex.StackTrace + ex.InnerException;
                rspNotificacion.status = "false";
                // Se guarda em BD el resultado
                executeSPLog(CadenaSQL, spLogEnvioSMSNotifiID, respuesta, vBody, ex.Message + ex.StackTrace + ex.InnerException);
                return BadRequest(rspNotificacion);
            }
        }

        private void executeSPLog(string SQLConexion, string SP, string APIResponse, string APIRequestNoti, string Error)
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
                        cmd.Parameters.Add("@pRespuestaAPI", SqlDbType.VarChar).Value = APIResponse;
                        cmd.Parameters.Add("@pRequestNotiAPI", SqlDbType.VarChar).Value = APIRequestNoti;
                        cmd.Parameters.Add("@pError", SqlDbType.VarChar).Value = Error;
                        cmd.Parameters.Add("@pTipo", SqlDbType.VarChar).Value = "NOTIFICACION";
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


        private string wWebhookOCC(string Url, int Timeout, EstructuraOCCNoti bodyMessage)
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
