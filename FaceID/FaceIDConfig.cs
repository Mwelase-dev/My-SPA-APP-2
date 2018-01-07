using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace Data.FaceID
{
    // Clock Device setup
    public class ClockDevice : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"] as string; }
        }

        [ConfigurationProperty("number", IsRequired = true)]
        public string Number
        {
            get { return this["number"] as string; }
        }
        
        [ConfigurationProperty("ipAddress", IsRequired = true)]
        public string ipAddress
        {
            get { return this["ipAddress"] as string; }
        }
    }
        
    // Collection of clocking Devices
    public class ClockDeviceCollection : ConfigurationElementCollection
    {
        public ClockDevice this[int index]
        {
            get
            {
                return base.BaseGet(index) as ClockDevice;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ClockDevice();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ClockDevice)element).Name;
        }
    }
    
    // Reader for Config file section
    public class FaceIDConfig : ConfigurationSection
    {
        [ConfigurationProperty("daysBack", IsRequired = false, DefaultValue = 0)]
        public int daysBack
        {
            get
            {
                return (int)this["daysBack"];
            }
            set
            { 
                this["daysBack"] = value; 
            }
        }

        [ConfigurationProperty("devices")]
        public ClockDeviceCollection ClockingDevices
        {
            get
            {
                return this["devices"] as ClockDeviceCollection;
            }
        }
    }
}
