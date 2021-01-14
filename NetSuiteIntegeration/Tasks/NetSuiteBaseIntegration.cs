using System.Configuration;
using System.Collections.Generic;
using NetSuiteIntegeration.com.netsuite.webservices;
using System;
using System.Net;
using System.Reflection;
using System.Linq;
using NetSuiteIntegration;

namespace NetSuiteIntegeration.Tasks
{
    public abstract class NetSuiteBaseIntegration
    {
        protected TaskType taskType;
        public abstract void Get();
        public abstract Int64 Set(string parametersArr);

        protected bool ExcuteService(string service_name, string table_name, string request_id)
        {
            table_name = table_name.Replace("tbl_", "");

            bool is_excuted = true;

            var assemblyName = "NetSuite.Integration";
            var nameSpace = "NetSuite.Integration";
            var prefixClassName = "Set";
            var postfixClassName = "Task";
            var asm = Assembly.Load(assemblyName);
            var type = asm.GetTypes().Where(p =>
                 p.Namespace == nameSpace && p.IsClass &&
                 p.Name.StartsWith(prefixClassName) &&
                 p.Name.EndsWith(postfixClassName)
            ).ToList().Where(x => x.GetType().Name.Equals(service_name)).FirstOrDefault();

            ConstructorInfo magicConstructor = type.GetConstructor(Type.EmptyTypes);
            object magicClassObject = magicConstructor.Invoke(new object[] { });
            if (type.GetMethod("Get") != null)
            {
                MethodInfo magicMethod = type.GetMethod("Get");
                object magicValue = magicMethod.Invoke(magicClassObject, new object[] { });
            }

            return is_excuted;
        }
        private NetSuiteService _Service = null;
        protected NetSuiteService Service(bool requireLogin = false)
        {
            if (requireLogin || _Service == null)
            {
                bool UseTba = true;
                _Service = new DataCenterAwareNetSuiteService(ConfigurationManager.AppSettings["login.acct"], "true".Equals(ConfigurationManager.AppSettings["promptForLogin"]) && !UseTba);
                _Service.Timeout = 1000 * 60 * 60 * 2;
                //Enable cookie management
                Uri myUri = new Uri("https://webservices.netsuite.com");
                _Service.CookieContainer = new CookieContainer();
                _Service.tokenPassport = Token.ins.CreateTokenPassport();

            }
            return _Service;
        }
        protected Record[] GetLookUp(GetAllRecordType recordType)
        {
            GetAllRecord record = new GetAllRecord();
            record.recordTypeSpecified = true;
            record.recordType = recordType;
            GetAllResult gr = new GetAllResult();
            gr = Service(true).getAll(record);
            return gr.recordList;
        }
        protected string GetCustomizationId(string scriptId, string customType = "customRecordType")
        {
            CustomizationType ct = new CustomizationType();
            ct.getCustomizationTypeSpecified = true;
            ct.getCustomizationType = (GetCustomizationType)Enum.Parse(typeof(GetCustomizationType), customType, true);
            GetCustomizationIdResult getCustIdResult = Service(true).getCustomizationId(ct, false);
            foreach (var customizationRef in getCustIdResult.customizationRefList)
            {
                if (customizationRef.scriptId == scriptId) return customizationRef.internalId;
            }
            return null;
        }
        protected bool FirstTimeRunService()
        {
            /*bool is_first_time = false;
            List<Model.Currency> list = new GenericeDAO<Model.Currency>().GetAll();
            if (list.Count == 0 || list == null)
            {
                is_first_time = true;
            }
            return is_first_time;
            */
            return true;
        }

        public static bool ExcuteService(string service_name, string request_id)
        {
            string[] arguments = new string[1];
            arguments[0] = request_id;

            bool is_excuted = true;
            var assemblyName = "NetSuite.Integration";
            var nameSpace = "NetSuite.Integration";
            var serviceName = service_name + "Task";
            var asm = Assembly.Load(assemblyName);

            var classes = asm.GetTypes().Where(p =>
                 p.Namespace == nameSpace && p.IsClass &&
                 p.Name.Equals(serviceName)).ToList();

            // var type = asm.GetTypes().Where(p => p.Namespace == nameSpace && p.IsClass && p.GetType().Name.Equals(serviceName)).FirstOrDefault();
            foreach (Type type in classes)
            {
                ConstructorInfo magicConstructor = type.GetConstructor(Type.EmptyTypes);
                object magicClassObject = magicConstructor.Invoke(new object[] { });
                if (type.GetMethod("Set") != null)
                {
                    MethodInfo magicMethod = type.GetMethod("Set");
                    object magicValue = magicMethod.Invoke(magicClassObject, arguments);
                }
            }
            return is_excuted;
        }
    }
}
