using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.UI.WebControls;
using System.Xml;
using EPiServer;
using EPiServer.Data.Dynamic;
using EPiServer.DataAbstraction;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;
using EPiServer.Shell.WebForms;
using EPiServer.UI.WebControls;
using EPiServer.Web;
using Nergard.EPi.Plugins.XmlResourceManager.DDS;

namespace Nergard.EPi.Plugins.XmlResourceManager.Plugins
{
    [GuiPlugIn(DisplayName = "Xml resource manager", Area = PlugInArea.AdminMenu, Url = "~/Plugins/XmlResourceManager.aspx")]
    public partial class XmlResourceManager : WebFormsBase
    {
        #region vars

        private const string viewsfilenamepostfix = "_";
        private const string viewsxmlfilenametemplate = "Views{0}.xml";
        private const string xmlfilenametemplate = "{0}_{1}.xml";
        private string _captionText = "";
        private string _description = "";
        private string _helpText = "";
        private string _propName = "";
        private string _typeName = "";
        private string viewsxmlfilename = "";
        private string xmlfilename = "";

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

        protected class TypePropertyResultItem
        {
            public string TypeName { get; set; }
            public string PropertyName { get; set; }
            public string DisplayName { get; set; }
            public string Description { get; set; }
        }

        protected class ViewResultItem
        {
            public string ContainingElementNameForDisplay { get; set; }
            public string ContainingElementName { get; set; }
            public string ElementName { get; set; }
            public string ElementValue { get; set; }
        }

        #endregion

        #region Overrides

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                var store = DynamicDataStoreFactory.Instance.GetStore(typeof(XmlResourceManagerSettings));

                var settings = store.Items<XmlResourceManagerSettings>().FirstOrDefault();

                if (settings != null)
                    txtPath.Text = ((XmlResourceManagerSettings)settings).ResourceFolderPath;
            }

            var path = txtPath.Text; 

            if (string.IsNullOrEmpty(path))
            {
                path = "/lang/";
            }

            this.xmlfilename = path + xmlfilenametemplate;
            this.viewsxmlfilename = path + viewsxmlfilenametemplate;

            var repository = ServiceLocator.Current.GetInstance<ILanguageBranchRepository>();

            if (IsPostBack)
            {
                return;
            }

            this.DdlSelectLanguage.DataSource = repository.ListEnabled();
            this.DdlSelectLanguage.DataTextField = "Name";
            this.DdlSelectLanguage.DataValueField = "LanguageID";
            this.DdlSelectLanguage.DataBind();

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.DdlSelectLanguage.SelectedValue);
            Inits();
        }

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            MasterPageFile = UriSupport.ResolveUrlFromUIBySettings("MasterPages/EPiServerUI.master");
            SystemMessageContainer.Heading = Translate("/xmlresourcemanager/heading");
            SystemMessageContainer.Description = Translate("/xmlresourcemanager/description");
        }

        #endregion

        #region Eventshandlers

        protected void CreateXml(object sender, EventArgs e)
        {
            CreateLangXmlFiles(((ToolButton)sender).CommandName);
            Thread.Sleep(2000);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.DdlSelectLanguage.SelectedValue);
            Inits();
        }

        protected void Refresh(object sender, EventArgs e)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(this.DdlSelectLanguage.SelectedValue);
            Inits();
        }

        protected void SavePath(object sender, EventArgs e)
        {
            var store = DynamicDataStoreFactory.Instance.GetStore(typeof(XmlResourceManagerSettings));

            var settings = store.Items<XmlResourceManagerSettings>().FirstOrDefault();

            if (settings != null)
            {
                store.DeleteAll();
                settings.ResourceFolderPath = Request.Form[txtPath.UniqueID];
                store.Save(settings);
            }
            else
            {
                settings = new XmlResourceManagerSettings();
                settings.ResourceFolderPath = txtPath.Text;
                store.Save(settings);
            }
        }

        #endregion

        #region Methods

        private void AddBlocksTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode contentTypes = doc.CreateElement("contenttypes");
            root.AppendChild(contentTypes);

            foreach (GridViewRow item in this.BlockViewControl.Rows)
            {
                var displayName = ((TextBox)item.FindControl("BlockTypeDisplayName")).Text;
                var descriptionText = ((TextBox)item.FindControl("BlockTypeDescription")).Text;
                var typeName = ((Label)item.FindControl("LblBlockName")).Text;

                XmlNode contentTypeNode = doc.CreateElement(typeName);

                XmlNode name = doc.CreateElement("name");
                name.AppendChild(doc.CreateTextNode(displayName));
                contentTypeNode.AppendChild(name);

                XmlNode description = doc.CreateElement("description");
                description.AppendChild(doc.CreateTextNode(descriptionText));
                contentTypeNode.AppendChild(description);

                contentTypes.AppendChild(contentTypeNode);
            }
        }

        private void AddCategoriesTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode categoriesNode = doc.CreateElement("categories");
            root.AppendChild(categoriesNode);

            foreach (GridViewRow item in this.CategoriesViewControl.Rows)
            {
                this._description = ((TextBox)item.FindControl("TxtCategory")).Text;
                this._propName = ((Label)item.FindControl("LblCategoryName")).Text;

                XmlNode catNode = doc.CreateElement("category");
                var pageTypename = doc.CreateAttribute("name");
                pageTypename.Value = this._propName;

                if (catNode.Attributes != null)
                {
                    catNode.Attributes.Append(pageTypename);
                }

                XmlNode description = doc.CreateElement("description");
                description.AppendChild(doc.CreateTextNode(this._description));
                catNode.AppendChild(description);
                categoriesNode.AppendChild(catNode);
            }
        }

        private void AddContentTypesTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode contentTypes = doc.CreateElement("contenttypes");
            root.AppendChild(contentTypes);

            foreach (GridViewRow item in this.PageTypeViewControl.Rows)
            {
                var displayName = ((TextBox)item.FindControl("TxtTypeDisplayName")).Text;
                var descriptionText = ((TextBox)item.FindControl("TxtTypeDescription")).Text;
                var typeName = ((Label)item.FindControl("LblPTypeName")).Text;

                XmlNode pageTypeNode = doc.CreateElement(typeName);

                XmlNode name = doc.CreateElement("name");
                name.AppendChild(doc.CreateTextNode(displayName));
                pageTypeNode.AppendChild(name);

                XmlNode description = doc.CreateElement("description");
                description.AppendChild(doc.CreateTextNode(descriptionText));
                pageTypeNode.AppendChild(description);

                contentTypes.AppendChild(pageTypeNode);
            }
        }

        private void AddDisplayChannelsTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode channels = doc.CreateElement("displaychannels");
            root.AppendChild(channels);

            foreach (GridViewRow item in this.ChannelsViewControl.Rows)
            {
                var channelDisplayName = ((TextBox)item.FindControl("DisplayChannelDisplayName")).Text;
                var channelName = ((Label)item.FindControl("DisplayChannelName")).Text;

                XmlNode channel = doc.CreateElement("displaychannel");
                var nameAttribute = doc.CreateAttribute("name");
                nameAttribute.Value = channelName;
                if (channel.Attributes != null)
                {
                    channel.Attributes.Append(nameAttribute);
                }

                XmlNode name = doc.CreateElement("name");
                name.AppendChild(doc.CreateTextNode(channelDisplayName));
                channel.AppendChild(name);
                channels.AppendChild(channel);
            }
        }

        private void AddDisplayResolutionsTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode resolutions = doc.CreateElement("resolutions");
            root.AppendChild(resolutions);

            foreach (GridViewRow item in this.ResolutionsViewControl.Rows)
            {
                var resolutionDisplayName = ((TextBox)item.FindControl("ResolutionDisplayName")).Text;
                var resolutionlName = ((Label)item.FindControl("ResolutionName")).Text;

                XmlNode name = doc.CreateElement(resolutionlName);
                name.AppendChild(doc.CreateTextNode(resolutionDisplayName));

                resolutions.AppendChild(name);
            }
        }

        private void AddGroupNameTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode groupsNode = doc.CreateElement("groups");
            root.AppendChild(groupsNode);

            foreach (GridViewRow item in this.TabViewControl.Rows)
            {
                this._description = ((TextBox)item.FindControl("TabDisplayName")).Text;
                this._propName = ((Label)item.FindControl("LblTabName")).Text;

                XmlNode group = doc.CreateElement(this._propName);
                group.AppendChild(doc.CreateTextNode(this._description));
                groupsNode.AppendChild(group);
            }
        }

        private void AddPropertiesTranslations(XmlDocument doc, XmlNode root, PropertyListType type)
        {
            XmlNode contentTypes = doc.CreateElement("contenttypes");

            switch (type)
            {
                case PropertyListType.PageTypeGeneral:
                
                    XmlNode pageData = doc.CreateElement("icontentdata");
                    XmlNode properties = doc.CreateElement("properties");
                    pageData.AppendChild(properties);
                    contentTypes.AppendChild(pageData);
                    root.AppendChild(contentTypes);

                    foreach (GridViewRow item in this.PropertiesViewControl.Rows)
                    {
                        this._captionText = ((TextBox)item.FindControl("TxtPropCaption")).Text;
                        this._helpText = ((TextBox)item.FindControl("TxtPropHelp")).Text;
                        this._propName = (((Label)item.FindControl("LblProp")).Text);

                        XmlNode propertyNode = doc.CreateElement(this._propName.ToLower());

                        XmlNode caption = doc.CreateElement("caption");
                        caption.AppendChild(doc.CreateTextNode(this._captionText));
                        propertyNode.AppendChild(caption);

                        XmlNode helptext = doc.CreateElement("help");
                        helptext.AppendChild(doc.CreateTextNode(this._helpText));
                        propertyNode.AppendChild(helptext);

                        properties.AppendChild(propertyNode);
                    }
                
                break;
                case PropertyListType.BlockGeneral:
                break;
                case PropertyListType.Block:
                
                    XmlNode typeNode = null;
                    XmlNode propertiesElement = null;

                    foreach (GridViewRow item in this.BlockPropertiesViewControl.Rows)
                    {
                        this._captionText = ((TextBox)item.FindControl("TxtBlockPropCaption")).Text;
                        this._helpText = ((TextBox)item.FindControl("TxtBlockPropDescription")).Text;
                        this._propName = ((Label)item.FindControl("LblBlockPropertyName")).Text;
                        this._typeName = ((Label)item.FindControl("LblBlockType")).Text;

                        if (!string.IsNullOrEmpty(this._typeName))
                        {
                            typeNode = doc.CreateElement(this._typeName);
                            propertiesElement = doc.CreateElement("properties");
                            typeNode.AppendChild(propertiesElement);
                        }

                        XmlNode propertytypeNode = doc.CreateElement(this._propName);
                        if (propertiesElement != null)
                        {
                            propertiesElement.AppendChild(propertytypeNode);
                        }

                        XmlNode caption = doc.CreateElement("caption");
                        caption.AppendChild(doc.CreateTextNode(this._captionText));
                        propertytypeNode.AppendChild(caption);

                        XmlNode helptext = doc.CreateElement("help");
                        helptext.AppendChild(doc.CreateTextNode(this._helpText));
                        propertytypeNode.AppendChild(helptext);

                        contentTypes.AppendChild(typeNode);
                    }

                    root.AppendChild(contentTypes);
                
                break;
            }
        }

        private void AddViewsTranslations(XmlDocument doc, XmlNode root)
        {
            XmlNode currentnode = null;
            foreach (GridViewRow item in this.ViewsControl.Rows)
            {
                var view = ((Label)item.FindControl("LblView")).Text;
                var container = ((Label)item.FindControl("LblElementContainer")).Text;
                var element = ((Label)item.FindControl("LblElement")).Text;
                var elementvalue = ((TextBox)item.FindControl("ElementValue")).Text;

                if (!string.IsNullOrEmpty(view))
                {
                    currentnode = doc.CreateElement(container);
                    root.AppendChild(currentnode);
                }

                if (currentnode != null)
                {
                    XmlNode node = doc.CreateElement(element);
                    node.AppendChild(doc.CreateTextNode(elementvalue));
                    currentnode.AppendChild(node);
                }
            }
        }

        private void CreateLangXmlFiles(string type)
        {
            var typeUpperCase = type;

            var doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(docNode);

            XmlNode languagesNode = doc.CreateElement("languages");
            doc.AppendChild(languagesNode);

            XmlNode languageNode = doc.CreateElement("language");
            var name = doc.CreateAttribute("name");
            name.Value = this.DdlSelectLanguage.SelectedItem.Text;

            var id = doc.CreateAttribute("id");
            id.Value = this.DdlSelectLanguage.SelectedItem.Value;

            if (languageNode.Attributes != null)
            {
                languageNode.Attributes.Append(name);
                languageNode.Attributes.Append(id);
            }

            languagesNode.AppendChild(languageNode);

            type = type.ToLower();

            switch (type)
            {
                case "BlocktypePropertyUniqueNames":
                    AddPropertiesTranslations(doc, languageNode, PropertyListType.BlockGeneral);
                    break;
                case "pagetypepropertynames":
                    AddPropertiesTranslations(doc, languageNode, PropertyListType.PageTypeGeneral);
                    break;
                case "blocktypepropertynames":
                    AddPropertiesTranslations(doc, languageNode, PropertyListType.Block);
                    break;
                case "pagetypes":
                    AddContentTypesTranslations(doc, languageNode);
                    break;
                case "blocks":
                    AddBlocksTranslations(doc, languageNode);
                    break;
                case "groupnames":
                    AddGroupNameTranslations(doc, languageNode);
                    break;
                case "categories":
                    AddCategoriesTranslations(doc, languageNode);
                    break;
                case "views":
                    AddViewsTranslations(doc, languageNode);
                    break;
                case "displaychannels":
                    AddDisplayChannelsTranslations(doc, languageNode);
                    break;
                case "displayresolutions":
                    AddDisplayResolutionsTranslations(doc, languageNode);
                    break;
            }

            doc.Save(type == "views"
                    ? Server.MapPath(string.Format(this.viewsxmlfilename, viewsfilenamepostfix + this.DdlSelectLanguage.SelectedItem.Value))
                    : Server.MapPath(string.Format(this.xmlfilename, typeUpperCase, this.DdlSelectLanguage.SelectedItem.Value)));
        }
        
        private IEnumerable<BlockType> GetBlockTypes()
        {
            var repository = new BlockTypeRepository(Locate.Advanced.GetInstance<IContentTypeRepository>());
            return repository.List().OrderBy(p => p.Name);
        }

        private IList GetCategories()
        {
            return Category.GetRoot().GetList();
        }

        private IList<DisplayChannel> GetDisplayChannels()
        {
            return Locate.DisplayChannelService().Channels;
        }

        private IEnumerable<IDisplayResolution> GetDisplayResolutions()
        {
            var helper = new ServiceLocationHelper(ServiceLocator.Current);
            var resolutionservice = helper.Advanced.GetInstance<DisplayResolutionService>();
            return resolutionservice.Resolutions;
        }

        private IEnumerable<PageType> GetPageTypes()
        {
            return ServiceLocator.Current.GetInstance<PageTypeRepository>().List().OrderBy(p => p.Name);
        }

        private List<TypePropertyResultItem> GetPropertiesByBlock()
        {
            var types = GetBlockTypes();
            var blockproperties = new List<TypePropertyResultItem>();

            foreach (var type in types)
            {
                blockproperties.AddRange(type.PropertyDefinitions.Select((def, i) =>
                        new TypePropertyResultItem
                        {
                                TypeName = i == 0 ? type.Name : string.Empty,
                                PropertyName = def.Name,
                                Description = def.TranslateDescription(),
                                DisplayName = def.TranslateDisplayName()
                        }));
            }

            return blockproperties;
        }

        private SortedDictionary<string, PropertyDefinition> GetPropertyDefinitions()
        {
            var definitions = new SortedDictionary<string, PropertyDefinition>();
            var pageTypes = GetPageTypes();

            foreach (var pType in pageTypes)
            {
                foreach (var propertyDefinition in pType.PropertyDefinitions)
                {
                    if (!definitions.ContainsKey(propertyDefinition.Name.ToLower()))
                    {
                        definitions.Add(propertyDefinition.Name.ToLower(), propertyDefinition);
                    }
                }
            }

            return definitions;
        }

        private IEnumerable<TabDefinition> GetTabNames()
        {
            return ServiceLocator.Current.GetInstance<ITabDefinitionRepository>().List();
        }

        private List<ViewResultItem> GetViewElements()
        {
            var result = new List<ViewResultItem>();
            var viewElements = new List<string>();
            const string Expression = "languages/language";
            var doc = new XmlDocument();

            var suffix = Server.MapPath(string.Format(this.viewsxmlfilename, viewsfilenamepostfix + this.DdlSelectLanguage.SelectedItem.Value));
            var nosuffix = Server.MapPath(string.Format(this.viewsxmlfilename, ""));
            try
            {
                if (File.Exists(suffix))
                {
                    doc.Load(suffix);
                }
                else if (File.Exists(nosuffix))
                {
                    doc.Load(nosuffix);
                }
                else
                {
                    return result;
                }

                viewElements.AddRange(from XmlElement rootelement in doc.SelectNodes(Expression + "/*") select rootelement.Name);

                foreach (var relement in viewElements)
                {
                    var nodes = doc.SelectNodes(Expression + "/" + relement + "/*");
                    if (nodes != null && nodes.Count == 0)
                    {
                        result.Add(new ViewResultItem { ContainingElementNameForDisplay = relement });
                    }
                    else
                    {
                        int x = 0;

                        foreach (XmlElement element in nodes)
                        {
                            x++;
                            result.Add(new ViewResultItem
                                       {
                                               ContainingElementNameForDisplay = (x == 1 ? relement : ""),
                                               ContainingElementName = relement,
                                               ElementName = element.Name,
                                               ElementValue = element.InnerText
                                       });
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

        private void Inits()
        {
            //PageTypes
            this.PageTypeViewControl.DataSource = GetPageTypes();
            this.PageTypeViewControl.DataBind();

            //Page type properties
            this.PropertiesViewControl.DataSource = GetPropertyDefinitions();
            this.PropertiesViewControl.DataBind();

            //Tabs
            this.TabViewControl.DataSource = GetTabNames();
            this.TabViewControl.DataBind();

            //Block types
            this.BlockViewControl.DataSource = GetBlockTypes();
            this.BlockViewControl.DataBind();

            //Block properties per type
            this.BlockPropertiesViewControl.DataSource = GetPropertiesByBlock();
            this.BlockPropertiesViewControl.DataBind();

            //Views
            this.ViewsControl.DataSource = GetViewElements();
            this.ViewsControl.DataBind();

            //Categories
            this.CategoriesViewControl.DataSource = GetCategories();
            this.CategoriesViewControl.DataBind();

            //Display channels
            this.ChannelsViewControl.DataSource = GetDisplayChannels();
            this.ChannelsViewControl.DataBind();

            //Display resolutions
            this.ResolutionsViewControl.DataSource = GetDisplayResolutions();
            this.ResolutionsViewControl.DataBind();
        }

        #endregion

        #region GetDataItems

        protected ContentType PType
        {
            get
            {
                return Page.GetDataItem() as ContentType;
            }
        }
        protected PropertyDefinition PDefinition
        {
            get
            {
                return ((KeyValuePair<string, PropertyDefinition>)Page.GetDataItem()).Value as PropertyDefinition;
            }
        }
        protected TabDefinition TabDefinition
        {
            get
            {
                return Page.GetDataItem() as TabDefinition;
            }
        }
        protected BlockType BType
        {
            get
            {
                return Page.GetDataItem() as BlockType;
            }
        }
        protected Category CategoryType
        {
            get
            {
                return Page.GetDataItem() as Category;
            }
        }
        protected ViewResultItem Element
        {
            get
            {
                return Page.GetDataItem() as ViewResultItem;
            }
        }
        protected TypePropertyResultItem BDefinition
        {
            get
            {
                return Page.GetDataItem() as TypePropertyResultItem;
            }
        }
        protected DisplayChannel Channel
        {
            get
            {
                return Page.GetDataItem() as DisplayChannel;
            }
        }
        protected IDisplayResolution Resolution
        {
            get
            {
                return Page.GetDataItem() as IDisplayResolution;
            }
        }

        #endregion
    }
}
