using LitJson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace shikii.Hub.Helpers
{
   
    public class EmbeddedResManager
    {
        Dictionary<String, LitJson.JsonData> JsonResDict;
        Dictionary<String, Stream> StreamResDict;
        public EmbeddedResManager()
        {
            JsonResDict = new Dictionary<string, LitJson.JsonData>();
            StreamResDict = new Dictionary<string, Stream>();
            
        }



        public   LitJson.JsonData GetTextRes(Assembly asm, String fileName, Encoding en )
        {
            if(JsonResDict.Keys.Contains(asm.GetName().Name + fileName))
            {
                return JsonResDict[asm.GetName().Name + fileName];
            }
            String[] resArr = asm.GetManifestResourceNames();
            String resStr = resArr.ToList().Find(x => x.EndsWith(fileName));
            System.IO.Stream sm = asm.GetManifestResourceStream(resStr);
            byte[] byts = new byte[sm.Length];
            sm.Read(byts, 0, (int)sm.Length);
            sm.Close();
            String strText = en.GetString(byts);
            JsonData tmpJsonData = LitJson.JsonMapper.ToObject(strText);
            this.JsonResDict.Add(asm.GetName().Name + fileName, tmpJsonData);
            return tmpJsonData;
        }
        public   System.IO.Stream GetStreamRes(Assembly asm, String fileName)
        {
            if (StreamResDict.Keys.Contains(asm.GetName().Name + fileName))
            {
                return StreamResDict[asm.GetName().Name + fileName];
            }
            String[] resArr = asm.GetManifestResourceNames();
            String resStr = resArr.ToList().Find(x => x.EndsWith(fileName));
            System.IO.Stream sm = asm.GetManifestResourceStream(resStr);
            this.StreamResDict.Add(asm.GetName().Name + fileName, sm);
            return sm;
        }



    }
}
