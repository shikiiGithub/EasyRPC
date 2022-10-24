
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.Threading;
using shikii.Hub.Common;
using shikii.Hub.Helpers;
 
using DryIoc;
using System.Runtime.InteropServices;
using System.IO;

namespace shikii.Hub.DI
{

    public  class DiManager
    {
        public Container container;

        public DiManager()
        {
            container = new Container();  
            ExtensionFuncs.ThisDiManager = this;
            this.RegisterInstance<DiManager>(this);
            Console.WriteLine("DIManager Inited");
        }

        public void RegisterInstance<T>(T obj, String name = null) where T : class
        {
       

            if (name == null)
                 container.RegisterInstance(obj);
            else
            {
                container.RegisterInstance(obj.GetType(), obj, null, null, name);
               
            }
        }

        public void RegisterInstance(Object obj, String name = null)  
        {
            Type type = obj.GetType();
           container.RegisterInstance(type, obj,IfAlreadyRegistered.Throw, null, name);
 
        }

        public bool HasRegistered(Type type)
        {
            List<ServiceRegistrationInfo> df = container.GetServiceRegistrations().ToList();
             int index = df.FindIndex(x => x.ServiceType == type);
            if (index == -1)
            {
                
                    return false;
         
            }
            else
                return true;    
        }

        public List<ServiceRegistrationInfo> Test()
        {
            List<ServiceRegistrationInfo> df = container.GetServiceRegistrations().ToList();
            return df;  
        }
        public void RegisterType(Type type, DIClassAttribute.LifeCycleModes mode, String name = null)
        {

            IReuse reuse = Reuse.Transient;
            switch (mode)
            {
                case DIClassAttribute.LifeCycleModes.Singleton:
                     reuse = Reuse.Singleton;
                    break;
                case DIClassAttribute.LifeCycleModes.Thread:
                    reuse = Reuse.Scoped;
                    break;
                case DIClassAttribute.LifeCycleModes.Transient:
                    reuse = Reuse.Transient;
                    break;
               
            }

          
            if (name == null)
                container.Register(type, reuse, null, null,IfAlreadyRegistered.Keep, null);
            else
            {
                container.Register(type ,reuse,null,null, IfAlreadyRegistered.Keep, name);

            }
        }

        public void RegisterType<T>(DIClassAttribute.LifeCycleModes mode, String name = null)
        {
            Type type = typeof(T);
            RegisterType(type, mode, name);
        }

        public void RegisterMethod<T>(Func<T> method) where T : class
        {
            container.RegisterDelegate<T>(context =>  method() );
        }

        void RegisterDIAttributeClass(Assembly asm)
        {
            Type[] types = asm.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                DIClassAttribute attr = types[i].GetCustomAttribute<DIClassAttribute>();


                if (attr != null)
                {

                    if (attr.typeInterface == null)
                    {
                        this.InternalGetRegistion(types[i], attr.LifeCycleMode);

                    }
                    else
                    {
                      this.InternalGetRegistion(types[i],attr.typeInterface,attr.LifeCycleMode);
                        
                    }
                }
            }
        }


        void InternalGetRegistion(Type type, DIClassAttribute.LifeCycleModes mode)
        {
            
            switch (mode)
            {
                case DIClassAttribute.LifeCycleModes.Singleton:
                    container.Register(type, Reuse.Singleton);
                    break;

                case DIClassAttribute.LifeCycleModes.Thread:
                    container.Register(type, Reuse.Scoped);
                    break;

                case DIClassAttribute.LifeCycleModes.Transient:
                    container.Register(type, Reuse.Transient);
                    break ;
            }

            

        }

        void InternalGetRegistion(Type type,Type interfaceType, DIClassAttribute.LifeCycleModes mode)
        {
            switch (mode)
            {
                case DIClassAttribute.LifeCycleModes.Singleton:
                    container.Register(interfaceType, type,Reuse.Singleton);
                   break ;

                case DIClassAttribute.LifeCycleModes.Thread:
                    container.Register(interfaceType, type, Reuse.Scoped);
                    break;
                case DIClassAttribute.LifeCycleModes.Transient:
                    container.Register(interfaceType, type, Reuse.Transient);
                    break;
            }

        }


       void InternalGetRegistion<T>(DIClassAttribute.LifeCycleModes mode) where T : class
         {
            InternalGetRegistion(typeof(T), mode);
         }

        public void Prepare(params Assembly[] assemblies)
        {
           
            assemblies.ToList().ForEach(
                x => RegisterDIAttributeClass(x)
                );
         
        }

        public static void AssignDiClassAttribute(Type type, params Attribute[] atts)
        {
            System.ComponentModel.TypeDescriptor.AddAttributes(type, atts);
        }
        public T GetService<T>(String name = null) where T : class
        {
            try
            {
                
                if (name == null)
                    return container.Resolve<T>( );
                else
                    return container.Resolve<T>(name);
            }
            catch (Exception ex)
            {

                return null;
            }
           
        }

       

        public Object GetService(Type type, String name = null)
        {
            try
            {
               
                if (name == null)
                    return container.Resolve(type);
                else
                    return container.Resolve(type, name);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void InjectTo(Object wantInjectedObject)
        {
            PropertyInfo [] pifs = wantInjectedObject.GetType().GetProperties();

            Action<PropertyInfo,String> DoInject = (x,name) =>
             {
                 try
                 {
                     Type type = x.PropertyType;
                     if (type.BaseType.Name == "ValueType" || type == typeof(String))
                         return;
                     Object obj = null;
                     if (name != null)
                         name = name.Trim();
                     if (!String.IsNullOrEmpty(name))
                         obj = container.Resolve(type, name);
                     else
                         obj = container.Resolve(type);

                     if (obj != null)
                         x.SetValue(wantInjectedObject, obj, null);
                 }
                 catch (Exception ex)
                 {

                     
                 }
               
             };
            for (int i = 0; i < pifs.Count(); i++)
            {
                if (pifs[i].CustomAttributes.Count() > 0)
                {
                    InjectPropertyAttribute [] attributes  = pifs[i].GetCustomAttributes<InjectPropertyAttribute>().ToArray();
                    if(attributes!= null && attributes.Length > 0)
                    {
                        InjectPropertyAttribute item = attributes.ToList().Find(x => x.Injection == InjectPropertyAttribute.Injections.Ignore);
                        if (item != null)
                            continue;
                        else
                           DoInject(pifs[i],attributes[0].AliasName);

                    }
                    else 
                          DoInject(pifs[i],null);

                }
                else
                {
                  DoInject(pifs[i],null);

                }

            }       
        }

        public Object GetService(String fullClassName)
        {
            Type type = this.GetType().Assembly.GetType(fullClassName);

            return container.Resolve(type);
        }

        ~DiManager()
        {
            // clean up, application exits
            container.Dispose();
        }

    }
}
