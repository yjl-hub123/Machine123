﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// 
// 此源代码是由 Microsoft.VSDesigner 4.0.30319.42000 版自动生成。
// 
#pragma warning disable 1591

namespace Machine.DataCollectForResourceFAIService {
    using System.Diagnostics;
    using System;
    using System.Xml.Serialization;
    using System.ComponentModel;
    using System.Web.Services.Protocols;
    using System.Web.Services;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="DataCollectForResourceFAIServiceBinding", Namespace="http://machine.ws.atlmes.com/")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(baseObject))]
    public partial class DataCollectForResourceFAIServiceService : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback dataCollectForResourceFAIOperationCompleted;
        
        private System.Threading.SendOrPostCallback erroAvoidOperationCompleted;
        
        private System.Threading.SendOrPostCallback getCustomDataValueExOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public DataCollectForResourceFAIServiceService() {
            this.Url = global::Machine.Properties.Settings.Default.Machine_DataCollectForResourceFAIService_DataCollectForResourceFAIServiceService;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event dataCollectForResourceFAICompletedEventHandler dataCollectForResourceFAICompleted;
        
        /// <remarks/>
        public event erroAvoidCompletedEventHandler erroAvoidCompleted;
        
        /// <remarks/>
        public event getCustomDataValueExCompletedEventHandler getCustomDataValueExCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        [return: System.Xml.Serialization.XmlElementAttribute("dataCollectForResourceFAIResponse", Namespace="http://machine.ws.atlmes.com/")]
        public dataCollectForResourceFAIResponse dataCollectForResourceFAI([System.Xml.Serialization.XmlElementAttribute("dataCollectForResourceFAI", Namespace="http://machine.ws.atlmes.com/")] dataCollectForResourceFAI dataCollectForResourceFAI1) {
            object[] results = this.Invoke("dataCollectForResourceFAI", new object[] {
                        dataCollectForResourceFAI1});
            return ((dataCollectForResourceFAIResponse)(results[0]));
        }
        
        /// <remarks/>
        public void dataCollectForResourceFAIAsync(dataCollectForResourceFAI dataCollectForResourceFAI1) {
            this.dataCollectForResourceFAIAsync(dataCollectForResourceFAI1, null);
        }
        
        /// <remarks/>
        public void dataCollectForResourceFAIAsync(dataCollectForResourceFAI dataCollectForResourceFAI1, object userState) {
            if ((this.dataCollectForResourceFAIOperationCompleted == null)) {
                this.dataCollectForResourceFAIOperationCompleted = new System.Threading.SendOrPostCallback(this.OndataCollectForResourceFAIOperationCompleted);
            }
            this.InvokeAsync("dataCollectForResourceFAI", new object[] {
                        dataCollectForResourceFAI1}, this.dataCollectForResourceFAIOperationCompleted, userState);
        }
        
        private void OndataCollectForResourceFAIOperationCompleted(object arg) {
            if ((this.dataCollectForResourceFAICompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.dataCollectForResourceFAICompleted(this, new dataCollectForResourceFAICompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        [return: System.Xml.Serialization.XmlElementAttribute("erroAvoidResponse", Namespace="http://machine.ws.atlmes.com/")]
        public erroAvoidResponse erroAvoid([System.Xml.Serialization.XmlElementAttribute("erroAvoid", Namespace="http://machine.ws.atlmes.com/")] erroAvoid erroAvoid1) {
            object[] results = this.Invoke("erroAvoid", new object[] {
                        erroAvoid1});
            return ((erroAvoidResponse)(results[0]));
        }
        
        /// <remarks/>
        public void erroAvoidAsync(erroAvoid erroAvoid1) {
            this.erroAvoidAsync(erroAvoid1, null);
        }
        
        /// <remarks/>
        public void erroAvoidAsync(erroAvoid erroAvoid1, object userState) {
            if ((this.erroAvoidOperationCompleted == null)) {
                this.erroAvoidOperationCompleted = new System.Threading.SendOrPostCallback(this.OnerroAvoidOperationCompleted);
            }
            this.InvokeAsync("erroAvoid", new object[] {
                        erroAvoid1}, this.erroAvoidOperationCompleted, userState);
        }
        
        private void OnerroAvoidOperationCompleted(object arg) {
            if ((this.erroAvoidCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.erroAvoidCompleted(this, new erroAvoidCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        [return: System.Xml.Serialization.XmlElementAttribute("getCustomDataValueExResponse", Namespace="http://machine.ws.atlmes.com/")]
        public getCustomDataValueExResponse getCustomDataValueEx([System.Xml.Serialization.XmlElementAttribute("getCustomDataValueEx", Namespace="http://machine.ws.atlmes.com/")] getCustomDataValueEx getCustomDataValueEx1) {
            object[] results = this.Invoke("getCustomDataValueEx", new object[] {
                        getCustomDataValueEx1});
            return ((getCustomDataValueExResponse)(results[0]));
        }
        
        /// <remarks/>
        public void getCustomDataValueExAsync(getCustomDataValueEx getCustomDataValueEx1) {
            this.getCustomDataValueExAsync(getCustomDataValueEx1, null);
        }
        
        /// <remarks/>
        public void getCustomDataValueExAsync(getCustomDataValueEx getCustomDataValueEx1, object userState) {
            if ((this.getCustomDataValueExOperationCompleted == null)) {
                this.getCustomDataValueExOperationCompleted = new System.Threading.SendOrPostCallback(this.OngetCustomDataValueExOperationCompleted);
            }
            this.InvokeAsync("getCustomDataValueEx", new object[] {
                        getCustomDataValueEx1}, this.getCustomDataValueExOperationCompleted, userState);
        }
        
        private void OngetCustomDataValueExOperationCompleted(object arg) {
            if ((this.getCustomDataValueExCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.getCustomDataValueExCompleted(this, new getCustomDataValueExCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class dataCollectForResourceFAI {
        
        private dataCollectForResourceFAIRequest resourceRequestField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public dataCollectForResourceFAIRequest resourceRequest {
            get {
                return this.resourceRequestField;
            }
            set {
                this.resourceRequestField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class dataCollectForResourceFAIRequest {
        
        private string siteField;
        
        private string dcGroupField;
        
        private string dcModeField;
        
        private string sfcField;
        
        private string materialField;
        
        private string materialRevisionField;
        
        private string dcGroupRevisionField;
        
        private string resourceField;
        
        private string operationField;
        
        private string operationRevisionField;
        
        private string dcGroupSequenceField;
        
        private string userField;
        
        private machineIntegrationParametricData[] parametricDataArrayField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string site {
            get {
                return this.siteField;
            }
            set {
                this.siteField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dcGroup {
            get {
                return this.dcGroupField;
            }
            set {
                this.dcGroupField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dcMode {
            get {
                return this.dcModeField;
            }
            set {
                this.dcModeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string sfc {
            get {
                return this.sfcField;
            }
            set {
                this.sfcField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string material {
            get {
                return this.materialField;
            }
            set {
                this.materialField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string materialRevision {
            get {
                return this.materialRevisionField;
            }
            set {
                this.materialRevisionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dcGroupRevision {
            get {
                return this.dcGroupRevisionField;
            }
            set {
                this.dcGroupRevisionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string resource {
            get {
                return this.resourceField;
            }
            set {
                this.resourceField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string operation {
            get {
                return this.operationField;
            }
            set {
                this.operationField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string operationRevision {
            get {
                return this.operationRevisionField;
            }
            set {
                this.operationRevisionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dcGroupSequence {
            get {
                return this.dcGroupSequenceField;
            }
            set {
                this.dcGroupSequenceField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string user {
            get {
                return this.userField;
            }
            set {
                this.userField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("parametricDataArray", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public machineIntegrationParametricData[] parametricDataArray {
            get {
                return this.parametricDataArrayField;
            }
            set {
                this.parametricDataArrayField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class machineIntegrationParametricData {
        
        private string nameField;
        
        private string valueField;
        
        private ParameterDataType dataTypeField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ParameterDataType dataType {
            get {
                return this.dataTypeField;
            }
            set {
                this.dataTypeField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sap.com/me/datacollection")]
    public enum ParameterDataType {
        
        /// <remarks/>
        NUMBER,
        
        /// <remarks/>
        TEXT,
        
        /// <remarks/>
        FORMULA,
        
        /// <remarks/>
        BOOLEAN,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class getCustomDataValueExResponse {
        
        private string returnField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string @return {
            get {
                return this.returnField;
            }
            set {
                this.returnField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class getCustomDataValueEx {
        
        private string arg0Field;
        
        private ObjectAliasEnum arg1Field;
        
        private bool arg1FieldSpecified;
        
        private string arg2Field;
        
        private string arg3Field;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string arg0 {
            get {
                return this.arg0Field;
            }
            set {
                this.arg0Field = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ObjectAliasEnum arg1 {
            get {
                return this.arg1Field;
            }
            set {
                this.arg1Field = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool arg1Specified {
            get {
                return this.arg1FieldSpecified;
            }
            set {
                this.arg1FieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string arg2 {
            get {
                return this.arg2Field;
            }
            set {
                this.arg2Field = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string arg3 {
            get {
                return this.arg3Field;
            }
            set {
                this.arg3Field = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.sap.com/me/common")]
    public enum ObjectAliasEnum {
        
        /// <remarks/>
        ACTIVITY,
        
        /// <remarks/>
        ACTIVITY_GROUP,
        
        /// <remarks/>
        ACTIVITY_LOG,
        
        /// <remarks/>
        ALARM,
        
        /// <remarks/>
        ALARM_LOG,
        
        /// <remarks/>
        APPLICATION_SETTING,
        
        /// <remarks/>
        ATTACHMENT,
        
        /// <remarks/>
        ATTENDANCE_LOG,
        
        /// <remarks/>
        BACKGROUND_PROCESS,
        
        /// <remarks/>
        BOM,
        
        /// <remarks/>
        BOM_COMPONENT,
        
        /// <remarks/>
        BUYOFF,
        
        /// <remarks/>
        BUYOFF_LOG,
        
        /// <remarks/>
        CERTIFICATION,
        
        /// <remarks/>
        CNC_PROGRAM,
        
        /// <remarks/>
        CONTAINER,
        
        /// <remarks/>
        CONTAINER_DATA,
        
        /// <remarks/>
        COST_CENTER,
        
        /// <remarks/>
        CUSTOMER,
        
        /// <remarks/>
        DATA_FIELD,
        
        /// <remarks/>
        DATA_TYPE,
        
        /// <remarks/>
        DC_GROUP,
        
        /// <remarks/>
        DOCUMENT,
        
        /// <remarks/>
        INVENTORY,
        
        /// <remarks/>
        INVENTORY_LOG,
        
        /// <remarks/>
        ITEM_GROUP,
        
        /// <remarks/>
        LABOR_CHARGE_CODE,
        
        /// <remarks/>
        MESSAGE,
        
        /// <remarks/>
        MESSAGE_LOG,
        
        /// <remarks/>
        MESSAGE_TYPE,
        
        /// <remarks/>
        NC_CODE,
        
        /// <remarks/>
        NEXT_NUMBER,
        
        /// <remarks/>
        OPERATION,
        
        /// <remarks/>
        PROCESS_LOT,
        
        /// <remarks/>
        PRODUCTION_LOG,
        
        /// <remarks/>
        REASON_CODE,
        
        /// <remarks/>
        SAMPLE_PLAN,
        
        /// <remarks/>
        WORK_INSTRUCTION,
        
        /// <remarks/>
        ITEM,
        
        /// <remarks/>
        RESOURCE,
        
        /// <remarks/>
        ROUTER,
        
        /// <remarks/>
        SHOP_ORDER,
        
        /// <remarks/>
        ROUTER_STEP,
        
        /// <remarks/>
        ROUTER_OPERATION,
        
        /// <remarks/>
        SFC,
        
        /// <remarks/>
        USR,
        
        /// <remarks/>
        USER_GROUP,
        
        /// <remarks/>
        WORK_CENTER,
        
        /// <remarks/>
        WORKSTATION,
        
        /// <remarks/>
        RESOURCE_TYPE,
        
        /// <remarks/>
        CUSTOMER_ORDER,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class erroAvoidResponse {
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(abstractDataSource))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(baseDataSource))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(systemBase))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class baseObject {
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(baseDataSource))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(systemBase))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public abstract partial class abstractDataSource : baseObject {
        
        private string dataSourceNameField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string dataSourceName {
            get {
                return this.dataSourceNameField;
            }
            set {
                this.dataSourceNameField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(systemBase))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class baseDataSource : abstractDataSource {
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class systemBase : baseDataSource {
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class erroAvoid {
        
        private string arg0Field;
        
        private string arg1Field;
        
        private string arg2Field;
        
        private string arg3Field;
        
        private string arg4Field;
        
        private systemBase arg5Field;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string arg0 {
            get {
                return this.arg0Field;
            }
            set {
                this.arg0Field = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string arg1 {
            get {
                return this.arg1Field;
            }
            set {
                this.arg1Field = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string arg2 {
            get {
                return this.arg2Field;
            }
            set {
                this.arg2Field = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string arg3 {
            get {
                return this.arg3Field;
            }
            set {
                this.arg3Field = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string arg4 {
            get {
                return this.arg4Field;
            }
            set {
                this.arg4Field = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public systemBase arg5 {
            get {
                return this.arg5Field;
            }
            set {
                this.arg5Field = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class machineIntegrationResourceDcResponse {
        
        private int codeField;
        
        private bool codeFieldSpecified;
        
        private string messageField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int code {
            get {
                return this.codeField;
            }
            set {
                this.codeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool codeSpecified {
            get {
                return this.codeFieldSpecified;
            }
            set {
                this.codeFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://machine.ws.atlmes.com/")]
    public partial class dataCollectForResourceFAIResponse {
        
        private machineIntegrationResourceDcResponse returnField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public machineIntegrationResourceDcResponse @return {
            get {
                return this.returnField;
            }
            set {
                this.returnField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    public delegate void dataCollectForResourceFAICompletedEventHandler(object sender, dataCollectForResourceFAICompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class dataCollectForResourceFAICompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal dataCollectForResourceFAICompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public dataCollectForResourceFAIResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((dataCollectForResourceFAIResponse)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    public delegate void erroAvoidCompletedEventHandler(object sender, erroAvoidCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class erroAvoidCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal erroAvoidCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public erroAvoidResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((erroAvoidResponse)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    public delegate void getCustomDataValueExCompletedEventHandler(object sender, getCustomDataValueExCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class getCustomDataValueExCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal getCustomDataValueExCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public getCustomDataValueExResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((getCustomDataValueExResponse)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591