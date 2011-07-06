using System;
using IronJS;
using IronJS.Hosting;
using IronJS.Native;
using Environment = IronJS.Environment;

namespace Uglify.CommonUtils
{
    public class ConsoleObject : CommonObject
    {
        public ConsoleObject(Environment env, CommonObject prototype)
            : base(env, env.Maps.Base, prototype)
        {
        }

        public override string ClassName
        {
            get { return "Console"; }
        }

        internal static void Log(FunctionObject func, CommonObject that, object value)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            if(value is string)
                Console.WriteLine(value);
            else if (value is BoxedValue)
            {
                BoxedValue boxedValue = ((BoxedValue)value);
                Console.WriteLine(TypeConverter.ToString(boxedValue));
            }

            Console.ResetColor();
        }

        internal static void Dir(FunctionObject func, CommonObject that, object value)
        {
            Log(func,that,value.GetType().ToString());
        }
    }

    public static class ConsoleConstructor
    {
        private static CommonObject Construct(FunctionObject ctor, CommonObject _)
        {
            CommonObject prototype = ctor.GetT<CommonObject>("prototype");
            return new CommonObject(ctor.Env, prototype);
        }

        public static void AttachToContext(CSharp.Context context)
        {
            CommonObject prototype = context.Environment.NewObject();
            FunctionObject constructor =
                Utils.CreateConstructor<Func<FunctionObject, CommonObject, CommonObject>>
                    (context.Environment, 0, Construct);
            FunctionObject log = Utils.CreateFunction<Action<FunctionObject, CommonObject, object>>
                (context.Environment, 1, ConsoleObject.Log);
            FunctionObject dir = Utils.CreateFunction<Action<FunctionObject, CommonObject, object>>
                (context.Environment, 1, ConsoleObject.Dir);

            prototype.Prototype = context.Environment.Prototypes.Object;
            prototype.Put("log", log, DescriptorAttrs.Immutable);
            prototype.Put("dir", dir, DescriptorAttrs.Immutable);
            constructor.Put("prototype", prototype, DescriptorAttrs.Immutable);
            context.SetGlobal("Console", constructor);
            context.Execute("console = new Console();");
        }
    }
}