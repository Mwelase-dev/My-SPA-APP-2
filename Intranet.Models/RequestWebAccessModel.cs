using System;

namespace Intranet.Models
{
    //TODO: Ref Jay: Is this passed to the front end? Should not be a required model?
    public class RequestWebAccessModel
    {
        public string NameSurname    { get; set; }
        public string EmailAddress   { get; set; }
        public string WebsiteAddress { get; set; }
        public string Motivation     { get; set; }
    }
}
