using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS.SMSCBN.IC.Model
{
    // PROVEEDOR

    public class PriceIn
    {
        public int pricePerMessage { get; set; }
        public string currency { get; set; }
    }

    public class ResultIn
    {
        public string messageId { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string text { get; set; }
        public string cleanText { get; set; }
        public string keyword { get; set; }
        public string receivedAt { get; set; }
        public int smsCount { get; set; }
        public PriceIn price { get; set; }
        public string callbackData { get; set; }
    }

    public class JsonProveedor
    {
        public List<ResultIn> results { get; set; }
        public int messageCount { get; set; }
        public int pendingMessageCount { get; set; }
    }

    // OCC

    public class EstructuraOCC
    {
        public List<MensajesOCC> messages { get; set; }
    }

    public class MensajesOCC
    {
        public string address { get; set; }
        public MensajeOCC message { get; set; }
    }



    public class Rsp2OCCEntrante
    {
        public string status { get; set; }
        public string reason { get; set; }
    }
}
