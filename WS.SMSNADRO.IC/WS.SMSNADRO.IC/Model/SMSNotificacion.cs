using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS.SMSCBN.IC.Model
{
 
    // Estructura Notificacion Response de OCC
    public class Rsp2OCCEntranteNoti
    {
        public string status { get; set; }
        public string reason { get; set; }
    }

    // Estructura Notificacion Status a OCC

    public class MessageNoti
    {
        public string id { get; set; }
        public string status { get; set; }
    }

    public class EstructuraOCCNoti
    {
        public List<MessageNoti> messages { get; set; }
    }

    // Estructura cliente

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Error
    {
        public int groupId { get; set; }
        public string groupName { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool permanent { get; set; }
    }

    public class Price
    {
        public double pricePerMessage { get; set; }
        public string currency { get; set; }
    }

    public class Result
    {
        public string bulkId { get; set; }
        public string messageId { get; set; }
        public string to { get; set; }
        public string sentAt { get; set; }
        public string doneAt { get; set; }
        public int smsCount { get; set; }
        public Price price { get; set; }
        public StatusNoti status { get; set; }
        public Error error { get; set; }
    }

    public class JsonNotiProveedor
    {
        public List<Result> results { get; set; }
    }

    public class StatusNoti
    {
        public int groupId { get; set; }
        public string groupName { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }



}
