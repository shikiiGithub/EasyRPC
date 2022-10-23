using System;
using System.Collections.Generic;
using System.Text;

namespace shikii.Hub.DI
{
    public  class DIClassAttribute :Attribute
    {
        public Type typeInterface = null;
        public enum LifeCycleModes { Singleton,Thread, Transient }
        public LifeCycleModes LifeCycleMode = LifeCycleModes.Singleton;

    }

    public class InjectPropertyAttribute : Attribute
    {
        public enum Injections { Inject, Ignore }

        /// <summary>
        /// 将要注入的这个属性如果在DI容器中以Named 方式注册了，请给下面的这个属性赋值
        /// </summary>
        public String AliasName { get; set; } = null;

        /// <summary>
        /// 注入方式
        /// </summary>
        public Injections Injection { get; set; }=Injections.Inject;

    }

}
