using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Data.Dynamic;

namespace Nergard.EPi.Plugins.XmlResourceManager.DDS
{
    [EPiServerDataStore(AutomaticallyCreateStore = true)]
    public class XmlResourceManagerSettings
    {
        public string ResourceFolderPath { get; set; }
    }
}