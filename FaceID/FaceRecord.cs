using System;
using System.Collections.Generic;

namespace Data.FaceID
{
    public class FaceRecord
    {
        public String   RecordID       { get; set; }
        public DateTime RecordDateTime { get; set; }
        public String   RecordName     { get; set; }
        public Boolean  RecordPopulated
        {
            get
            {
                return ((RecordDateTime != DateTime.MinValue) && (!String.IsNullOrEmpty(RecordID)) && (!String.IsNullOrEmpty(RecordName)));
            }
        }

        public FaceRecord()
        {
            RecordID       = String.Empty;
            RecordDateTime = DateTime.MinValue;
            RecordName     = String.Empty;
        }

        public override string ToString()
        {
            return String.Format("Date: {0} - ID: {1} - Name: {2}", this.RecordDateTime.ToString(), this.RecordID, this.RecordName);
        }
    }

    public class FaceUser
    {
        public int FaceID               { get; set; }
        public String FaceName          { get; set; }
        public String FaceAuthority     { get; set; }
        public String FaceCardNumber    { get; set; }
        public String FaceCalID         { get; set; }
        public String FaceDoorType      { get; set; }
        public String FaceCheckType     { get; set; }
        public IList<FaceData> FaceData { get; set; }
        public Boolean  RecordPopulated
        {
            get
            {
                return ((FaceID > 0) && (!String.IsNullOrEmpty(FaceName)) && (!String.IsNullOrEmpty(FaceCheckType)) && (FaceData.Count > 0));
            }
        }

        public FaceUser()
        {
            this.FaceData = new List<FaceData>();
        }        
    }

    public class FaceData
    {
        public string Data { get; set; }

         
    }
}