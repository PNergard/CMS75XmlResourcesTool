using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using EPiServer;
using EPiServer.PlugIn;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using System.Collections;
using System.Threading;
using System.Globalization;
using EPiServer.UI.WebControls;
using System.Xml;
using System.Text;
using System.IO;
using System.Web.Configuration; //ServiceLocator
using EPiServer.ServiceLocation;
using System.Linq;

namespace Nergard.EPi.Plugins
{
    [GuiPlugIn(DisplayName = "Xml resource manager", Area = PlugInArea.AdminMenu, Url = "~/Plugins/XmlResourceManager.aspx")]
    public partial class XmlResourceManager : EPiServer.Shell.WebForms.WebFormsBase 
    {
        #region vars
        private string _captionText = "";
        private string _helpText = "";
        private string _propName = "";
        private string _description = "";
        private string _typeName = "";
        private string xmlfilename = "/Resources/LanguageFiles/{0}_{1}.xml";
        private string viewsxmlfilename = "/Resources/LanguageFiles/Views{0}.xml";
        private const string xmlfilenametemplate = "{0}_{1}.xml";
        private const string viewsxmlfilenametemplate = "Views{0}.xml";
        private const string viewsfilenamepostfix = "_";
        #endregion

        #region enumbs
        private enum PropertyListType
        {
            Block,
            BlockGeneral,
            PageTypeGeneral
        }


        #endregion

        #region Inner classes
        protected class ViewResultItem
        {
            public string ContainingElementNameForDisplay { get; set; }
            public string ContainingElementName { get; set; }
            public string ElementName { get; set; }
            public string ElementValue { get; set; }
        }

        protected class TypePropertyResultItem
        {
            public string TypeName { get; set; }
            public string PropertyName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
        }
        #endregion

        #region Overrides

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            this.MasterPageFile = UriSupport.ResolveUrlFromUIBySettings("MasterPages/EPiServerUI.master"); 
            this.SystemMessageContainer.Heading = this.Translate("/xmlresourcemanager/heading");
            this.SystemMessageContainer.Description = this.Translate("/xmlresourcemanager/description");
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var path = WebConfigurationManager.AppSettings.Get("resourcemanagerpath");
            if (string.IsNullOrEmpty(path))
                path = "/lang/";

            xmlfilename = path + xmlfilenametemplate;
            viewsxmlfilename = path + viewsxmlfilenametemplate;

            var repository = ServiceLocator.Current.GetInstance<ILanguageBranchRepository>();

            if (!IsPostBack)
            {
                DdlSelectLanguage.DataSource = repository.ListEnabled();
                DdlSelectLanguage.DataTextField = "Name";
                DdlSelectLanguage.DataValueField = "LanguageID";
                DdlSelectLanguage.DataBind();

                Thread.CurrentThread.CurrentUICulture = new CultureInfo(DdlSelectLanguage.SelectedValue);

                Inits();
            }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            //DdlSelectLanguage.SelectedIndexChanged += new EventHandler(DdlSelectLanguage_SelectedIndexChanged);
        }
        #endregion

        #region Eventshandlers
        protected void CreateXml(object sender, EventArgs e)
        {
            CreateLangXmlFiles(((ToolButton)sender).CommandName);
            Thread.Sleep(2000);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(DdlSelectLanguage.SelectedValue);
            Inits();

        }

        protected void Refresh(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(DdlSelectLanguage.SelectedValue);
            Inits();
        }

        private void DdlSelectLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(((DropDownList)sender).SelectedValue);
            Inits();
        }

        #endregion

        #region Methods

        private void Inits()
        {
            //PageTypes
            PageTypeViewControl.DataSource = GetPageTypes(); // ServiceLocator.Current.GetInstance<PageTypeRepository>().List();
            PageTypeViewControl.DataBind();

            //Page type properties
            PropertiesViewControl.DataSource = GetPropertyDefinitions();
            PropertiesViewControl.DataBind();

            //Tabs
            TabViewControl.DataSource = GetTabNames();
            TabViewControl.DataBind();

            //Block types
            BlockViewControl.DataSource = GetBlockTypes();
            BlockViewControl.DataBind();

            ////Block properties unique
            //BlockPropertiesUnique.DataSource = GetBlockProperties();
            //BlockPropertiesUnique.DataBind();
             
            //Block properties per type
            BlockPropertiesViewControl.DataSource = GetPropertiesByBlock();
            BlockPropertiesViewControl.DataBind();

            //Views
            ViewsControl.DataSource = GetViewElements();
            ViewsControl.DataBind();

            //Categories
            CategoriesViewControl.DataSource = GetCategories();
            CategoriesViewControl.DataBind();

            //Display channels
            ChannelsViewControl.DataSource = GetDisplayChannels();
            ChannelsViewControl.DataBind();

            //Display resolutions
            ResolutionsViewControl.DataSource = GetDisplayResolutions();
            ResolutionsViewControl.DataBind();
        }

        private void CreateLangXmlFiles(string type)
        {
            string typeUpperCase = type;

            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(docNode);

            XmlNode languagesNode = doc.CreateElement("languages");
            doc.AppendChild(languagesNode);

            XmlNode languageNode = doc.CreateElement("language");
            XmlAttribute name = doc.CreateAttribute("name");
            name.Value = DdlSelectLanguage.SelectedItem.Text;

            XmlAttribute id = doc.CreateAttribute("id");
            id.Value = DdlSelectLanguage.SelectedItem.Value;

            languageNode.Attributes.Append(name);
            languageNode.Attributes.Append(id);

            languagesNode.AppendChild(languageNode);

            type = type.ToLower();

            if (type == "BlocktypePropertyUniqueNames")
                AddPropertiesTranslations(doc, languageNode, PropertyListType.BlockGeneral);
            else if (type == "pagetypepropertynames")
                AddPropertiesTranslations(doc, languageNode, PropertyListType.PageTypeGeneral);
            else if (type == "blocktypepropertynames")
                AddPropertiesTranslations(doc, languageNode, PropertyListType.Block);
            else if (type == "pagetypes")
                AddContenttTypesTranslations(doc, languageNode);
            else if (type == "blocks")
                AddBlocksTranslations(doc, languageNode);
            else if (type == "groupnames")
                AddGroupNameTranslations(doc, languageNode);
            else if (type == "categories")
                AddCategoriesTranslations(doc, languageNode);
            else if (type == "views")
                AddViewsTranslations(doc, languageNode);
            else if (type == "displaychannels")
                AddDisplayChannelsTranslations(doc,languageNode);
            else if (type == "displayresolutions")
                AddDisplayResolutionsTranslations(doc,languageNode);

            if (type == "views")
                doc.Save(Server.MapPath(string.Format(viewsxmlfilename, viewsfilenamepostfix + DdlSelectLanguage.SelectedItem.Value)));
            else
                doc.Save(Server.MapPath(string.Format(xmlfilename, typeUpperCase, DdlSelectLanguage.SelectedItem.Value)));
        }

        private void AddDisplayResolutionsTranslations(XmlDocument doc, XmlNode root)
        {
            string resolutionlName = "";
            string resolutionDispName = "";

            XmlNode resolutions = doc.CreateElement("resolutions");
            root.AppendChild(resolutions);

            foreach (GridViewRow item in ResolutionsViewControl.Rows)
            {
                resolutionDispName = ((TextBox)item.FindControl("ResolutionDisplayName")).Text;
                resolutionlName = ((Label)item.FindControl("ResolutionName")).Text;

                XmlNode name = doc.CreateElement(resolutionlName);
                name.AppendChild(doc.CreateTextNode(resolutionDispName));

                resolutions.AppendChild(name);
            }    
        }

        private void AddDisplayChannelsTranslations(XmlDocument doc, XmlNode root)
        {
            string channelName = "";
            string channelDispName = "";

            XmlNode channels = doc.CreateElement("displaychannels");
            root.AppendChild(channels);

            foreach (GridViewRow item in ChannelsViewControl.Rows)
            {
                channelDispName = ((TextBox)item.FindControl("DisplayChannelDisplayName")).Text;
                channelName = ((Label)item.FindControl("DisplayChannelName")).Text;


                XmlNode chnl = doc.CreateElement("displaychannel");

                XmlAttribute attrname = doc.CreateAttribute("name");
                attrname.Value = channelName;
                chnl.Attributes.Append(attrname);

                XmlNode name = doc.CreateElement("name");
                name.AppendChild(doc.CreateTextNode(channelDispName));

                chnl.AppendChild(name);

                channels.AppendChild(chnl);
            }
        }

        private void AddViewsTranslations(XmlDocument doc, XmlNode root)
        {
            string view = "";
            string container = "";
            string element = "";
            string elementvalue = "";
            XmlNode currentnode = null;
            XmlNode node = null;

            foreach (GridViewRow item in ViewsControl.Rows)
            {
                view = ((Label)item.FindControl("LblView")).Text;
                container = ((Label)item.FindControl("LblElementContainer")).Text;
                element = ((Label)item.FindControl("LblElement")).Text;
                elementvalue = ((TextBox)item.FindControl("ElementValue")).Text;

                if (!string.IsNullOrEmpty(view))
                {
                    currentnode = doc.CreateElement(container);
                    root.AppendChild(currentnode);
                }

                if (currentnode != null)
                {
                    node = doc.CreateElement(element);
                    node.AppendChild(doc.CreateTextNode(elementvalue));
                    currentnode.AppendChild(node);
                }
            }
        }

        private void AddCategoriesTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode categoriesNode = doc.CreateElement("categories");
            root.AppendChild(categoriesNode);

            foreach (GridViewRow item in CategoriesViewControl.Rows)
            {
                _description = ((TextBox)item.FindControl("TxtCategory")).Text;
                _propName = ((Label)item.FindControl("LblCategoryName")).Text;

                XmlNode catNode = doc.CreateElement("category");
                XmlAttribute pageTypename = doc.CreateAttribute("name");
                pageTypename.Value = _propName;

                catNode.Attributes.Append(pageTypename);
                XmlNode description = doc.CreateElement("description");
                description.AppendChild(doc.CreateTextNode(_description));
                catNode.AppendChild(description);
                categoriesNode.AppendChild(catNode);
            }
        }

        private void AddGroupNameTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode groupsNode = doc.CreateElement("groups");
            root.AppendChild(groupsNode);

            foreach (GridViewRow item in TabViewControl.Rows)
            {
                _description = ((TextBox)item.FindControl("TabDisplayName")).Text;
                _propName = ((Label)item.FindControl("LblTabName")).Text;

                XmlNode group = doc.CreateElement(_propName);
                group.AppendChild(doc.CreateTextNode(_description));
                groupsNode.AppendChild(group);
            }
        }

        private void AddContenttTypesTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode contentTypes = doc.CreateElement("contenttypes");
            root.AppendChild(contentTypes);

            foreach (GridViewRow item in PageTypeViewControl.Rows)
            {
                string dispName = ((TextBox)item.FindControl("TxtTypeDisplayName")).Text;
                string descText = ((TextBox)item.FindControl("TxtTypeDescription")).Text;
                string typeName = ((Label)item.FindControl("LblPTypeName")).Text;

                XmlNode pageTypeNode = doc.CreateElement(typeName);

                XmlNode name = doc.CreateElement("name");
                name.AppendChild(doc.CreateTextNode(dispName));
                pageTypeNode.AppendChild(name);
                
                XmlNode description = doc.CreateElement("description");
                description.AppendChild(doc.CreateTextNode(descText));
                pageTypeNode.AppendChild(description);

                contentTypes.AppendChild(pageTypeNode);
            }
        }

        private void AddPropertiesTranslations(XmlDocument doc, XmlNode root, PropertyListType type)
        {
            XmlNode contentTypes = doc.CreateElement("contenttypes");

            if (type == PropertyListType.PageTypeGeneral)
            {

                XmlNode pageData = doc.CreateElement("icontentdata");
                XmlNode properties = doc.CreateElement("properties");
                pageData.AppendChild(properties);
                contentTypes.AppendChild(pageData);
                root.AppendChild(contentTypes);

                foreach (GridViewRow item in PropertiesViewControl.Rows)
                {
                    _captionText = ((TextBox)item.FindControl("TxtPropCaption")).Text;
                    _helpText = ((TextBox)item.FindControl("TxtPropHelp")).Text;
                    _propName = ((Label)item.FindControl("LblProp")).Text;

                    XmlNode propertyNode = doc.CreateElement(_propName.ToLower());

                    XmlNode caption = doc.CreateElement("caption");
                    caption.AppendChild(doc.CreateTextNode(_captionText));
                    propertyNode.AppendChild(caption);

                    XmlNode helptext = doc.CreateElement("help");
                    helptext.AppendChild(doc.CreateTextNode(_helpText));
                    propertyNode.AppendChild(helptext);

                    properties.AppendChild(propertyNode);
                }
            }
            else if (type == PropertyListType.BlockGeneral)
            {
                

            }
            else if (type == PropertyListType.Block)
            {
                XmlNode typeNode = null;
                XmlNode properties = null;

                foreach (GridViewRow item in BlockPropertiesViewControl.Rows)
                {
                    _captionText = ((TextBox)item.FindControl("TxtBlockPropCaption")).Text;
                    _helpText = ((TextBox)item.FindControl("TxtBlockPropDescription")).Text;
                    _propName = ((Label)item.FindControl("LblBlockPropertyName")).Text;
                    _typeName = ((Label)item.FindControl("LblBlockType")).Text;

                    if (!string.IsNullOrEmpty(_typeName))
                    {
                        typeNode = doc.CreateElement(_typeName);
                        properties = doc.CreateElement("properties");
                        typeNode.AppendChild(properties);
                    }
                    

                    XmlNode propertytypeNode = doc.CreateElement(_propName);
                    properties.AppendChild(propertytypeNode);           
                    
                    XmlNode caption = doc.CreateElement("caption");
                    caption.AppendChild(doc.CreateTextNode(_captionText));
                    propertytypeNode.AppendChild(caption);

                    XmlNode helptext = doc.CreateElement("help");
                    helptext.AppendChild(doc.CreateTextNode(_helpText));
                    propertytypeNode.AppendChild(helptext);


                    contentTypes.AppendChild(typeNode);
                }

                root.AppendChild(contentTypes);

            }

        }

        private void AddBlocksTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode contentTypes = doc.CreateElement("contenttypes");
            root.AppendChild(contentTypes);

            foreach (GridViewRow item in BlockViewControl.Rows)
            {
                string dispName = ((TextBox)item.FindControl("BlockTypeDisplayName")).Text;
                string descText = ((TextBox)item.FindControl("BlockTypeDescription")).Text;
                string typeName = ((Label)item.FindControl("LblBlockName")).Text;

                XmlNode contentTypeNode = doc.CreateElement(typeName);

                XmlNode name = doc.CreateElement("name");
                name.AppendChild(doc.CreateTextNode(dispName));
                contentTypeNode.AppendChild(name);

                XmlNode description = doc.CreateElement("description");
                description.AppendChild(doc.CreateTextNode(descText));
                contentTypeNode.AppendChild(description);

                contentTypes.AppendChild(contentTypeNode);

            }
        }

        private List<ViewResultItem> GetViewElements()
        {
            StringBuilder strB = new StringBuilder();
            List<ViewResultItem> result = new List<ViewResultItem>();
            List<string> viewelements = new List<string>();
            string xpathexpression = "languages/language";
            XmlDocument doc = new XmlDocument();

            string suffix = Server.MapPath(string.Format(viewsxmlfilename, viewsfilenamepostfix + DdlSelectLanguage.SelectedItem.Value));
            string nosuffix = Server.MapPath(string.Format(viewsxmlfilename, ""));
            try
            {
                if (File.Exists(suffix))
                    doc.Load(suffix);
                else if (File.Exists(nosuffix))
                    doc.Load(nosuffix);
                else
                    return result;

                foreach (XmlElement rootelement in doc.SelectNodes(xpathexpression + "/*"))
                {
                    viewelements.Add(rootelement.Name);
                }

                foreach (string relement in viewelements)
                {
                    XmlNodeList nodes = doc.SelectNodes(xpathexpression + "/" + relement + "/*");
                    if (nodes.Count == 0)
                    {
                        result.Add(new ViewResultItem { ContainingElementNameForDisplay = relement });
                    }
                    else
                    {
                        int x = 0;

                        foreach (XmlElement element in nodes)
                        {
                            x++;
                            result.Add(new ViewResultItem { ContainingElementNameForDisplay = (x == 1 ? relement : ""), ContainingElementName = relement, ElementName = element.Name, ElementValue = element.InnerText });
                        }
                    }
                }
            }
            finally
            {
                doc = null;
            }


            return result;
        }
        
        private IEnumerable<IDisplayResolution> GetDisplayResolutions()
        {
            ServiceLocationHelper helper = new ServiceLocationHelper(ServiceLocator.Current);
            DisplayResolutionService resolutionservice = helper.Advanced.GetInstance<DisplayResolutionService>();
            return resolutionservice.Resolutions;
        }

        private IList<DisplayChannel> GetDisplayChannels()
        {
            return Locate.DisplayChannelService().Channels;// GetActiveChannels(new HttpContextWrapper(HttpContext.Current));
        }

        private IEnumerable<PageType> GetPageTypes()
        {
            return ServiceLocator.Current.GetInstance<PageTypeRepository>().List().OrderBy(p => p.Name);
        }

        private SortedDictionary<string, PropertyDefinition> GetPropertyDefinitions()
        {
            SortedDictionary<string, PropertyDefinition> definitions = new SortedDictionary<string, PropertyDefinition>();
            IEnumerable<PageType> pageTypes = GetPageTypes();

            foreach (PageType pType in pageTypes)
            {
                foreach (PropertyDefinition propertyDefenition in pType.PropertyDefinitions)
                {
                    if (!definitions.ContainsKey(propertyDefenition.Name.ToLower()))
                        definitions.Add(propertyDefenition.Name.ToLower(), propertyDefenition);
                }
            }

            return definitions;
        }

        private List<TypePropertyResultItem> GetPropertyDefinitionsByPageType()
        {
            IEnumerable<PageType> ptypes = GetPageTypes();
            List<TypePropertyResultItem> blockproperties = new List<TypePropertyResultItem>();
            int x = 0;

            foreach (PageType type in ptypes)
            {
                x = 0;

                foreach (PropertyDefinition def in type.PropertyDefinitions)
                {
                    x++;
                    blockproperties.Add(new TypePropertyResultItem { TypeName = x == 1 ? type.Name : "", PropertyName = def.Name, Description = def.TranslateDescription(), DisplayName = def.TranslateDisplayName() });

                }
            }

            return blockproperties;
        }

        private IEnumerable<BlockType> GetBlockTypes()
        {
            var repo = new BlockTypeRepository(Locate.Advanced.GetInstance<IContentTypeRepository>());
            return repo.List().OrderBy(p=>p.Name);
        }

        private List<TypePropertyResultItem> GetPropertiesByBlock()
        {
            IEnumerable<BlockType> btypes = GetBlockTypes();
            List<TypePropertyResultItem> blockproperties = new List<TypePropertyResultItem>();
            int x = 0;

            foreach (BlockType type in btypes)
            {
                x = 0;

                foreach (PropertyDefinition def in type.PropertyDefinitions)
                {
                    x++;
                    blockproperties.Add(new TypePropertyResultItem { TypeName = x == 1 ? type.Name : "", PropertyName = def.Name, Description = def.TranslateDescription(), DisplayName = def.TranslateDisplayName() });

                }
            }

            return blockproperties;
        }

        private SortedDictionary<string, PropertyDefinition> GetBlockProperties()
        {
            SortedDictionary<string, PropertyDefinition> definitions = new SortedDictionary<string, PropertyDefinition>();
            IEnumerable<BlockType> blockTypes = GetBlockTypes();

            foreach (BlockType bType in blockTypes)
            {
                foreach (PropertyDefinition propertyDefenition in bType.PropertyDefinitions)
                {
                    if (!definitions.ContainsKey(propertyDefenition.Name.ToLower()))
                        definitions.Add(propertyDefenition.Name.ToLower(), propertyDefenition);
                }
            }

            return definitions;
        }



        private IEnumerable<TabDefinition> GetTabNames()
        {
            return ServiceLocator.Current.GetInstance<ITabDefinitionRepository>().List();
        }

        private IList GetCategories()
        {
            return Category.GetRoot().GetList();
        }

        #endregion

        #region GetDataItems

        protected ContentType PType { get { return Page.GetDataItem() as ContentType; } }
        protected PropertyDefinition PDefinition { get { return ((KeyValuePair<string, PropertyDefinition>)Page.GetDataItem()).Value as PropertyDefinition; } }
        protected TabDefinition TabDefinition { get { return Page.GetDataItem() as TabDefinition; } }
        protected BlockType BType { get { return Page.GetDataItem() as BlockType; } }
        protected Category CategoryType { get { return Page.GetDataItem() as Category; } }
        protected ViewResultItem Element { get { return Page.GetDataItem() as ViewResultItem; } }
        protected TypePropertyResultItem BDefinition { get { return Page.GetDataItem() as TypePropertyResultItem; } }
        protected DisplayChannel Channel { get { return Page.GetDataItem() as DisplayChannel; } }
        protected IDisplayResolution Resolution { get { return Page.GetDataItem() as IDisplayResolution; } }

        #endregion


        //public PlugInDescriptor[] List()
        //{
        //    PlugInDescriptor[] aDescriptors = null;
        //    string param = WebConfigurationManager.AppSettings.Get("DebugSettings");
        //    if (param.ToLower() == "true")
        //    {
        //        aDescriptors = new PlugInDescriptor[1];
        //        aDescriptors[0] = PlugInDescriptor.Load(this.GetType());
        //    }

        //    return aDescriptors;
        //}
    }
}