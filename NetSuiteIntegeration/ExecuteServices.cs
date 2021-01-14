using System;
using System.Linq;
using System.Reflection;

namespace NetSuiteIntegeration
{
    public class ExecuteServices
    {
        public static bool ExcuteService(string service_name, string request_id)
        {
            object[] arguments = new object[1];
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