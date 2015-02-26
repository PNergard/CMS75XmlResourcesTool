<%@ Page Language="c#" EnableViewState="true" CodeBehind="XmlResourceManager.aspx.cs" AutoEventWireup="False" Inherits="Nergard.EPi.Plugins.XmlResourceManager.Plugins.XmlResourceManager" Title="XmlResourceManager" %>

<%@ Register TagPrefix="EPiServerUI" Namespace="EPiServer.UI.WebControls" Assembly="EPiServer.UI" %>
<%@ Register TagPrefix="EPiServerScript" Namespace="EPiServer.ClientScript.WebControls" Assembly="EPiServer" %>

<asp:content contentplaceholderid="MainRegion" runat="server">

    <script type="text/javascript">
        $(function () {
            CheckTranslations();

            $("input").change(function (e) {
                CheckTranslations();
            });

        });

        function CheckTranslations() {
            $("tr").each(function () {
                if (!$(this).find(':first-child').is('th')) {
                    var col1 = $(this).find('span:first');
                    var col2 = $(this).find('input:first');

                    var nameFromCode = col1.text();
                    var translatedName = col2.val();

                    if ((nameFromCode.toUpperCase() == translatedName.toUpperCase()) || !(/\S/.test(translatedName.toUpperCase()))) {
                        $(this).css('background-color', '#A4EDE6');
                    }
                    else {
                        $(this).css('background-color', '');
                    }
                }
            });
        }

    </script>

    <div class="epi-formArea">
        <div class="epi-buttonDefault epi-size25">
            <asp:DropDownList ID="DdlSelectLanguage" runat="server" AutoPostback="false" EnableViewState="true" />
            <EPiServerUI:ToolButton id="ToolButton11" DisablePageLeaveCheck="true" OnClick="Refresh" runat="server" SkinID="File" text="<%$ Resources: EPiServer, xmlresourcemanager.buttonrefresh %>" ToolTip="<%$ Resources: EPiServer, xmlresourcemanager.buttonrefresh %>" />
            <asp:TextBox EnableViewState="true" Columns="50" ID="txtPath" runat="server"></asp:TextBox>
            <EPiServerUI:ToolButton id="ToolButton18" DisablePageLeaveCheck="true" OnClick="SavePath" runat="server" SkinID="File" text="<%$ Resources: EPiServer, xmlresourcemanager.buttonsavepath %>" ToolTip="<%$ Resources: EPiServer, xmlresourcemanager.buttonsavepath %>" />
        </div>
    </div>

    <EPiServerUI:TabStrip  runat="server" id="actionTab" EnableViewState="true" GeneratesPostBack="False" targetid="tabView" supportedpluginarea="SystemSettings">
        <EPiServerUI:Tab Text="<%$ Resources: EPiServer, xmlresourcemanager.tabpagetypes %>" runat="server" ID="Tab1" />
        <EPiServerUI:Tab Text="<%$ Resources: EPiServer, xmlresourcemanager.tabpropertiesunique %>" runat="server" ID="Tab2" />
        <EPiServerUI:Tab Text="<%$ Resources: EPiServer, xmlresourcemanager.tabblocks %>" runat="server" ID="Tab4" />
        <%--<EPiServerUI:Tab Text="<%$ Resources: EPiServer, xmlresourcemanager.tabblockpropertiesunique %>" runat="server" ID="Tab10" />--%>
        <EPiServerUI:Tab Text="<%$ Resources: EPiServer, xmlresourcemanager.tabblockpropertiesperproperty %>" runat="server" ID="Tab7" />
        <EPiServerUI:Tab Text="<%$ Resources: EPiServer, xmlresourcemanager.tabviews %>" runat="server" ID="Tab5" />
        <EPiServerUI:Tab Text="<%$ Resources: EPiServer, xmlresourcemanager.tabcategories %>" runat="server" ID="Tab6" />
        <EPiServerUI:Tab Text="<%$ Resources: EPiServer, xmlresourcemanager.tabtabs %>" runat="server" ID="Tab3" />
        <EPiServerUI:Tab Text="<%$ Resources: EPiServer, xmlresourcemanager.tabdisplaychannels %>" runat="server" ID="Tab8" />
        <EPiServerUI:Tab Text="<%$ Resources: EPiServer, xmlresourcemanager.tabdisplayresolution %>" runat="server" ID="Tab9" />
    </EPiServerUI:TabStrip>

    <asp:Panel runat="server" ID="tabView" CssClass="epi-padding">
        <div class="epi-formArea" ID="Pagetypes" runat="server">
            <div class="epi-size25">
                <div>
                    <asp:GridView ID="PageTypeViewControl" runat="server"  AutoGenerateColumns="false" >
                        <Columns>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnname %>" ItemStyle-Wrap="false">                
                                <ItemTemplate>
                                    <asp:Label Text="<%#PType.Name %> " ID="LblPTypeName" runat="server" CssClass="firstcol"/>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columndisplayname %>" ItemStyle-Wrap="false">                
                                <ItemTemplate>
                                    <asp:TextBox ID="TxtTypeDisplayName" Text='<%# PType.LocalizedName %>'  runat="server" CssClass="secondcol"/>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columndescription  %>" ItemStyle-Wrap="false">                
                                <ItemTemplate>
                                    <asp:TextBox ID="TxtTypeDescription" Text='<%# PType.LocalizedDescription %>' CssClass="EP-requiredField" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                </div>
                <div class="epi-buttonContainer">
                    <EPiServerUI:ToolButton id="Save" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="PageTypes"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="Cancel" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                    <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent1" EventTargetID="Cancel" EventType="click" runat="server" />
                </div>
            </div>

        </div>
        <div class="epi-formArea" ID="Properties" runat="server">
            <div class="epi-size25"> 
                <div>
                    <asp:GridView
                        ID="PropertiesViewControl"
                        runat="server" 
                        AutoGenerateColumns="false" 
                        EnableViewState="true">
                        <Columns>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnname %>" ItemStyle-Wrap="false">                
                                <ItemTemplate>
                                    <asp:Label Text="<%#PDefinition.Name %> " ID="LblProp" runat="server"/>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columncaption %>" ItemStyle-Wrap="false">                
                                <ItemTemplate>
                                    <asp:TextBox ID="TxtPropCaption" Text='<%# PDefinition.TranslateDisplayName() %>' CssClass="EP-requiredField" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnhelptext %>" ItemStyle-Wrap="false">                
                                <ItemTemplate>
                                    <asp:TextBox ID="TxtPropHelp" Text='<%# PDefinition.TranslateDescription() %>' CssClass="EP-requiredField" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>               
                </div>
                <div class="epi-buttonContainer">
                    <EPiServerUI:ToolButton id="ToolButton1" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="PagetypePropertyNames"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="ToolButton2" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                    <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent2" EventTargetID="Cancel" EventType="click" runat="server" />
                </div>
            </div>
        </div>
        <div class="epi-formArea" ID="Blocks" runat="server">
            <div class="epi-size25"> 
                <asp:GridView
                    ID="BlockViewControl"
                    runat="server" 
                    AutoGenerateColumns="false" >
                    <Columns>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnname %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:Label Text="<%#BType.Name %> " ID="LblBlockName" runat="server"/>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columndisplayname %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:TextBox ID="BlockTypeDisplayName" Text='<%# BType.LocalizedName %>' CssClass="EP-requiredField" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columndescription  %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:TextBox ID="BlockTypeDescription" Text='<%# BType.LocalizedDescription %>' CssClass="EP-requiredField" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div class="epi-buttonContainer">
                <EPiServerUI:ToolButton id="ToolButton5" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="Blocks"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="ToolButton6" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent4" EventTargetID="Cancel" EventType="click" runat="server" />
            </div>
        </div>
        <%--     <div class="epi-formArea" ID="Blockproperties" runat="server">
            <div class="epi-size25"> 
                <div>
                    <asp:GridView
                    ID="BlockPropertiesUnique"
                    runat="server" 
                    AutoGenerateColumns="false" 
                     EnableViewState="true">
                    <Columns>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnname %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                               <asp:Label Text="<%#PDefinition.Name%> " ID="Label1" runat="server"/>
                            </ItemTemplate>
                        </asp:TemplateField>
                         <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columncaption %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:TextBox ID="TextBox1" Text='<%# PDefinition.TranslateDisplayName() %>' CssClass="EP-requiredField" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnhelptext %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:TextBox ID="TextBox2" Text='<%# PDefinition.TranslateDescription() %>' CssClass="EP-requiredField" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>               
                </div>
                <div class="epi-buttonContainer">
                    <EPiServerUI:ToolButton id="ToolButton18" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="BlocktypePropertyUniqueNames"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="ToolButton19" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                    <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent10" EventTargetID="Cancel" EventType="click" runat="server" />
                </div>
            </div>
        </div>--%>
        <div class="epi-formArea" ID="blockpropertiespertype" runat="server">
            <div class="epi-size25"> 
                <div>
                    <asp:GridView
                        ID="BlockPropertiesViewControl"
                        runat="server" 
                        AutoGenerateColumns="false" 
                        EnableViewState="true">
                        <Columns>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnblocktype %>" ItemStyle-Wrap="false">        
                                <ItemTemplate>
                                    <b><asp:Label id="LblBlockType" runat="server" Text="<%#BDefinition.TypeName %>" /></b>
                                </ItemTemplate>        
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnname %>" ItemStyle-Wrap="false">
                                <ItemTemplate>
                                    <asp:Label Text="<%#BDefinition.PropertyName %> " ID="LblBlockPropertyName" runat="server"/>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columncaption %>" ItemStyle-Wrap="false">                
                                <ItemTemplate>
                                    <asp:TextBox ID="TxtBlockPropCaption" Text='<%# BDefinition.DisplayName %>' CssClass="EP-requiredField" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnhelptext %>" ItemStyle-Wrap="false">                
                                <ItemTemplate>
                                    <asp:TextBox ID="TxtBlockPropDescription" Text='<%# BDefinition.Description %>' CssClass="EP-requiredField" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>               
                </div>
                <div class="epi-buttonContainer">
                    <EPiServerUI:ToolButton id="ToolButton12" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="BlocktypePropertyNames"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="ToolButton13" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                    <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent7" EventTargetID="Cancel" EventType="click" runat="server" />
                </div>
            </div>
        </div>
        <div class="epi-formArea" ID="Views" runat="server">
            <div class="epi-size25"> 
                <asp:GridView
                    ID="ViewsControl"
                    runat="server" 
                    AutoGenerateColumns="false" >
                    <Columns>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnview %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <b><asp:Label id="LblView" Text="<%#Element.ContainingElementNameForDisplay %>" runat="server" /></b>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="ElementContainer" ItemStyle-Wrap="false" Visible="false">
                            <ItemTemplate>
                                <asp:Label id="LblElementContainer" Text="<%#Element.ContainingElementName %>" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnviewelement %>" ItemStyle-Wrap="false">
                            <ItemTemplate>
                                <asp:Label id="LblElement" Text="<%#Element.ElementName %>" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnviewelementvalue %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:TextBox ID="ElementValue" Text='<%#Element.ElementValue %>' CssClass="EP-requiredField" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div class="epi-buttonContainer">
                <EPiServerUI:ToolButton id="ToolButton9" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="Views"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="ToolButton10" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent6" EventTargetID="Cancel" EventType="click" runat="server" />
            </div>
        </div>
        <div class="epi-formArea" ID="Categories" runat="server">
            <div class="epi-size25">
                <div>
                    <asp:GridView
                        ID="CategoriesViewControl"
                        runat="server" 
                        AutoGenerateColumns="false" >
                        <Columns>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnname %>" ItemStyle-Wrap="false">                
                                <ItemTemplate>
                                    <asp:Label Text="<%#CategoryType.Name %> " ID="LblCategoryName" runat="server"/>                                
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columndescription %>" ItemStyle-Wrap="false">                
                                <ItemTemplate>
                                    <asp:TextBox ID="TxtCategory" Text='<%# CategoryType.LocalizedDescription %>' CssClass="EP-requiredField" runat="server" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div> 
            </div>
            <div class="epi-buttonContainer">
                <EPiServerUI:ToolButton id="ToolButton7" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="Categories"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="ToolButton8" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent5" EventTargetID="Cancel" EventType="click" runat="server" />
            </div>
        </div>
        <div class="epi-formArea" ID="Groups" runat="server">
            <div class="epi-size25"> 
                <asp:GridView
                    ID="TabViewControl"
                    runat="server" 
                    AutoGenerateColumns="false" >
                    <Columns>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnname %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:Label Text="<%#TabDefinition.Name %> " ID="LblTabName" runat="server"/>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columndisplayname %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:TextBox ID="TabDisplayName" Text='<%# TabDefinition.LocalizedName %>' CssClass="EP-requiredField" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div class="epi-buttonContainer">
                <EPiServerUI:ToolButton id="ToolButton3" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="GroupNames"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="ToolButton4" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent3" EventTargetID="Cancel" EventType="click" runat="server" />
            </div>
        </div>
        <div class="epi-formArea" ID="DisplayChannels" runat="server">
            <div class="epi-size25">
                <asp:GridView
                    ID="ChannelsViewControl"
                    runat="server" 
                    AutoGenerateColumns="false" >
                    <Columns>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnname %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:Label Text="<%#Channel.ChannelName %> " ID="DisplayChannelName" runat="server"/>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columndisplayname %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:TextBox ID="DisplayChannelDisplayName" Text='<%# Channel.DisplayName %>' CssClass="EP-requiredField" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            
                <div class="epi-buttonContainer">
                    <EPiServerUI:ToolButton id="ToolButton14" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="displaychannels"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="ToolButton15" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                    <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent8" EventTargetID="Cancel" EventType="click" runat="server" />
                </div>                 
            </div>
        </div>  
        <div class="epi-formArea" ID="DisplayResolutions" runat="server">
            <div class="epi-size25">
                <asp:GridView
                    ID="ResolutionsViewControl"
                    runat="server" 
                    AutoGenerateColumns="false" >
                    <Columns>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columnname %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:Label Text="<%#Resolution.Id %> " ID="ResolutionName" runat="server"/>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="<%$ Resources: EPiServer, xmlresourcemanager.columndisplayname %>" ItemStyle-Wrap="false">                
                            <ItemTemplate>
                                <asp:TextBox ID="ResolutionDisplayName" Text='<%# Resolution.Name %>' CssClass="EP-requiredField" runat="server" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            
                <div class="epi-buttonContainer">
                    <EPiServerUI:ToolButton id="ToolButton16" DisablePageLeaveCheck="true" OnClick="CreateXml" CommandName="displayresolutions"  runat="server" SkinID="Save" text="<%$ Resources: EPiServer, button.save %>" ToolTip="<%$ Resources: EPiServer, button.save %>" /><EPiServerUI:ToolButton id="ToolButton17" runat="server" CausesValidation="false" SkinID="Cancel" text="<%$ Resources: EPiServer, button.cancel %>" ToolTip="<%$ Resources: EPiServer, button.cancel %>" />
                    <EPiServerScript:ScriptReloadPageEvent ID="ScriptReloadPageEvent9" EventTargetID="Cancel" EventType="click" runat="server" />
                </div>                 
            </div>
        </div>  
    </asp:Panel>

</asp:content>
