using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
namespace shikii.Hub.WebApi
{
   
    public class ServiceStatusManager
    {
        Object bookedServices = null;


       public Dictionary<String,bool> ServiceSatusDic = new Dictionary<String,bool>();

        public ServiceStatusManager()
        {
           
        }

        public void PrepareBookedServices(Object obj)
        {
            bookedServices = obj;
            FieldInfo[] fifs = bookedServices.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            fifs.ToList().ForEach(f => {
                String fieldValue = f.GetValue(bookedServices).ToString();
                ServiceSatusDic.Add(fieldValue, false);
            });
        }

        public void OnSpyingServiceChanged(LitJson.JsonData dic)
        {
            int count = dic.Count;
            List<String> keys = dic.Keys.ToList();
            for (int i = 0; i < count; i++)
            {
                String key = keys[i];
                this.ServiceSatusDic[key] = (bool)dic[key];
            }
        }

        public void ResetSpyingServiceStatus()
        {
            int count = this.ServiceSatusDic.Count;
            List<String> keys = this.ServiceSatusDic.Keys.ToList();
            for (int i = 0; i < count; i++)
            {
                String key = keys[i];
                this.ServiceSatusDic[key] = false;
            }
        }

       public override string ToString()
        {
           FieldInfo []fifs  = bookedServices.GetType().GetFields();
            StringBuilder sb = new StringBuilder();
            fifs.ToList().ForEach(f => { 
                
                String fieldValue = f.GetValue(bookedServices).ToString();
                sb.AppendFormat("{0};", fieldValue);
            } ) ;
            if(sb.Length > 0)
            sb.Remove(sb.Length - 1, 1); String result = sb.ToString();
            sb.Clear();
            return result;
        }
    }
}
