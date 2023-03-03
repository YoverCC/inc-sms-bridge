using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WS.SMSCBN.IC.Model
{
    public class JSONOCC
    {
        public MensajeOCC message { get; set; }
        public List<string> addresses { get; set; }
    }

    public class MensajeOCC
    {
        public string text { get; set; }
    }

    public class Destination
    {
        public string to { get; set; }
    }

    public class Language
    {
        public string languageCode { get; set; }
    }

    public class Message
    {
        public List<Destination> destinations { get; set; }
        public Language language { get; set; }
        public string from { get; set; }
        public string text { get; set; }
    }

    public class MessageSalientINFOBIP
    {
        public List<Message> messages { get; set; }
    }


    //RESPUESTA MENSAJE SALIENTE

    public class MessageSaliente
    {
        public string messageId { get; set; }
        public Status status { get; set; }
        public string to { get; set; }
    }

    public class ResponseSMSOutInfobip
    {
        public string bulkId { get; set; }
        public List<MessageSaliente> messages { get; set; }
    }

    public class Status
    {
        public string description { get; set; }
        public int groupId { get; set; }
        public string groupName { get; set; }
        public int id { get; set; }
        public string name { get; set; }
    }

}
